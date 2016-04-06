@{
    Layout = "post";
    Title = "Optimistic Concurrency with F Sharp and Azure Sql";
    AddedDate = "2016-04-04T04:37:39";
    Tags = "";
    Description = "";
    Image = "";
    PostAuthor = "Dana Peele";
}

### Jet’s Catalog Platform  

The catalog platform at Jet consists of a constellation of (partially) reactive micro-services that communicate with one another using queues and message buses. Commands are consumed by various services from the queues and events are emitted onto the message buses. Other services subscribe to these message buses and react to events they receive in a concurrent manner. This concurrency occurs both within services which process multiple events in parallel and among different services that subscribe to the same topic. The upshot is that we are able to process 100’s of millions of events and commands a day. But consuming events in this manner creates the opportunity for contention and potential errors related to race conditions between processes or threads accessing the same data. In the catalog, this is an unacceptable outcome that we prevent. Below I go through an example of such a situation and one way that we ensure it doesn't happen. Obviously, this example is greatly simplified given that a product in the Jet catalog will potentially have 100’s of attributes and given that there are many catalog operations that act on these attributes. In practice we use higher-order functions, asynchronous computation expressions, generics, and many other goodies to handle the terabytes of data we aggregate into our catalog and promote best practices like code reuse, fault tolerance, scalability, etc. But here is a taste...  

### An Example  

Let's assume that somewhere in our catalog, a thread consuming Event A and a thread consuming Event B are both trying to update the title of Product ABC, stored in our Azure Sql database:  
![](/images/Untitled-Diagram.png)  
The title contained in both events has a higher priority than the existing title in the database and Event A’s title has a higher priority than Event B’s. So the correct outcome of these events should be that the database should contain the title from Event A. Let's assume that each thread retrieves the row corresponding to this product from the database at roughly the same time (prior to the other thread updating it) and sees that the priority of the existing title for the product is lower than the priority of the title in its event. Given this situation, both threads attempt to update the title in the database. Since neither thread knows about the other, we have a race condition. And if Event B updates the database after Event A then the wrong title is applied to the product:  
![](/images/Untitled-Diagram12.png)  
![](/images/Untitled-Diagram14.png)  
In this case we have discarded a higher priority title by mistake and this isn’t an acceptable outcome.  

### So How Can We Fix This???

The obvious answer is locking aka pessimistic concurrency. If our threads obtain locks, then we can be sure that no other thread is updating the database at the same time. However, we reduce our ability to concurrently access the data since other threads have to wait for these locks to clear. We also incur the expense of managing these locks. In some cases, this is still a good approach, especially when there is a large degree of contention (when it is common for multiple threads to concurrently access the same record). However, in our use case here at Jet we have relatively infrequent contention so we want another solution. There are several possible solutions besides locking including:

*   Optimistic Concurrency
*   Software Transactional Memory
*   Partitioning message buses
*   Managing concurrent access in the application layer via memory resident queues

We use many of these methods to handle concurrency in different parts of the catalog but today we are going to explore the first tool, optimistic concurrency. Optimistic concurrency (optimistically) assumes that our threads will be able to do their jobs without conflicting with each other. Each thread accesses the database to retrieve data, updates it, and then writes it back to the database. **However**, before it updates the database it confirms that the version of the data that it retrieved is the same version of the data currently contained in the database. So let’s see how this would change the above example. First both threads retrieve the current version (1) of the product data from the database:  
![](/images/Untitled-Diagram4.png)  
Then the thread consuming Event A writes back to the database, updating the title and version:  
![](/images/Untitled-Diagram5.png)  
When the thread consuming Event B tries to write back to the database, it fails since the version has now changed:  
![](/images/Untitled-Diagram6.png)  
The application registers the failure and retries the read, this time (correctly) **not** overwriting the higher priority title:  
![](/images/Untitled-Diagram7.png)  

### Let’s See Some Code

The implementation for this is pretty simple. In the database let’s assume we have a table:  

    CREATE TABLE dbo.products (product_id VARCHAR(64), title VARCHAR(64), priority INT);

We add the following column to the product table:  

    ALTER TABLE products ADD version ROWVERSION;

The ROWVERSION column type will store a version value for each row and will automatically update this value each time a row is updated. Then we create a stored procedure that we will use to update the products table:  

      CREATE PROCEDURE dbo.set_product
          @@product_id VARCHAR(64),
          @@title VARCHAR(64),
          @@priority INT,
          @@version TIMESTAMP
        AS
          BEGIN
            UPDATE products
            SET title = @@title, priority = @@priority
            WHERE @@product_id = product_id AND @@version = version;
          END
          SELECT @@@@ROWCOUNT;

When we call this stored procedure we pass in the version we expect the database to contain (that we selected as part of the product data we retrieved to apply the update on top of) and we will only update the row if this is the current version. We return either 1 or 0 from the stored procedure, 1 indicating success (@@@@ROWCOUNT will tell us the number of rows modified by the previous update statement) and 0 indicating failure (no row updated because version has changed). And now the F# code:  


    let tryChangeTitle (newTitle:string) (newPriority:int) (productId:string): unit =  
      let rec tryAgain() =  
        //helper method to get productData and productVersion from DB  
        let title, priority, version =  
          productId  
          |> getProduct  
        if newPriority > priority then  
          //helper method to call stored procedure to update database if version is current  
          let result = setProduct productId title priority version  
          if result = 0 then tryAgain()  
      tryAgain()  


We pass in the new title, priority, and product id from the event into this method. In the tryAgain interior method we retrieve the current title, priority, and version from the database. We check if the title should be updated and if it should be updated then we attempt to do the update, taking version into account. We call our stored procedure above and if the version has changed and the update fails, we recursively call the tryAgain method again. We repeat until our versions match. And voila! Safe concurrent access with no locking.  