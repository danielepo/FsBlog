@{
    Layout = "post";
    Title = "Realtime Pricing, Realtime Advertising.";
    AddedDate = "2015-08-17T01:49:01";
    Tags = "F#, microservices, Marketing Tech";
    Description = "Google PLAs";
    Image = "";
    PostAuthor = "Louie Bacaj";
}


![Google PLAs](/images/blogcover.png)



When you have millions of products for sale and you have an algorithmic and dynamic pricing system for price changes on those products, what can possibly go wrong when advertising them?


<!--more--> 
So to give you a little context, I work as a software engineer here at Jet.com, and I was recently tasked with adding our products to Google for advertising on Google's Product Listing Adds. You know, the ones that pop up on Google Shopping like this when you search for something you want to buy.

![Cetaphil on Google PLAs](/images/Cetaphil.PNG)



The problem is Google will ban you from advertising if they catch your price as being off; this of course is can be considered false advertising. Not to mention the terrible customer experience you get from seeing one price on an advertisement then finding out its actually different when you get to the product.



####Why so many prices?

The challenge here at Jet is the system is always trying to find the lowest price for the consumer, the algorithm is tuned to find efficiencies in the supply chain and the markets and pass back those savings. This generates millions of price changes in realtime, in an event driven system with lots of different streams. Google will take action against your account even if your price is lower than your advertised price, which is often the case with Jet, since our prices start low and get lower.


####Who knew trying to save people money could be so hard?

Our business model is a little bit different when compared to other large E-Commerce players in the industry. Our pricing algorithm reacts to lots of market conditions, merchant partners selling the item you want to buy, our warehouses having that item and other items in your basket in a close vicinity to you; among many other factors including competitor prices. We don't make a profit on the items we sell, only on the membership fee and because of that all aspects of the system are tuned to find the consumer the most savings they can. Our system has more in common with financial institutions than it has with other E-Commerce sites.


####How can we manage all of this?

I'm going to switch gears a little bit and talk about the technology that makes this possible. Of course technology lends itself very well to this sort of problem. For example, our whole system is cloud based on Windows Azure. All of our backend systems use F#, which is a functional first language that does really well when parallelism and scaling is a top priority. It is fantastic for managing and controlling state and has helped us scale from beta to hundreds of thousands of users without a hitch.

It is safe to say we are pushing the .NET stack with F# to places where it's never been before. Even the people on the other end at Google are telling us they have never seen this kind of volume before. The volume of data we are pushing and the frequency in which we are changing prices are something other e-commerce companies never do with Googles Shopping APIs. We blew through their standard API per month limit within a few minutes of opening the firehouse.


> *It is safe to say we are pushing the .NET stack with F# to places where it's never been before.*

The way we use F#, I think, is pretty unique. We build lots of F# Micro Services that are supported by some very cleaver in house libraries that abstract away lots of complexity and greatly simplify development. Given that F# is a functional first language that defaults to immutable constructs, this alone is extremely helpful in being able to manage this type of complex system.

To add to that, we have a strong propensity towards immutable constructs. We believe state is the root of all evil, when it comes to scale. That is why we tend to favor things like EventStore for backend event dumps. EventStore is an immutable data store and, as immutable implies, never allows you to update anything, rather write new items only. This way you always have a record of what the item looked like at any point in time.

F# and these types of immutable data stores seem to go extremely well together and help us build a powerful scalable system that is low on complexity. Some parts of our system, that are very stream heavy, use Kafka and Storm, again F# seems to work very well here as well. Don't get me wrong, we have lots of supporting services that rely on state like Redis and even Azure SQL databases but these are more like peripheral and support systems rather than core. They support Business Analysts rather than build the backbone of our system. If these things are ever out of Sync or go down we can simply rerun our projections and rebuild them from our immutable data stores.

Another thing that helps us manage complexity is, as I mentioned earlier, our use of Micro Services, which consume these very large streams of data and react accordingly. Things here do one thing and do it well. So if a price changes there is an event emitted that we can react to and tell Google about. The problem is the calls to Google are obviously over the wire and through REST APIs, which makes pushing millions of changes very difficult in a timely manner.


####Scaling and parallelism

Back to the problem at hand, getting these price changes to Google. One of the first things that come to mind, when dealing with this sort of problem, is to batch the changes and send them in that manner. Which of course is a great idea but it's still not enough, because you will still fall behind in this stream of changes, our prices, inventory, and many other aspects of the products will be out of sync.

As a software engineer I have been apart of many large software projects and implementations and certainly this one qualifies as one of them. However, Given our architecture and the forward thinking technology decisions that were made early on by members of the Jet engineering team, it turns out this problem is actually pretty easy to solve here at Jet. We just build a few Micro Services to react to the different streams, some react to price changes, others to availability and some micro services to other product level changes.

We can then easily parallelize the consumption of these events and chunk them into batches to go out as RESTful calls to Google. Before they go out they are first dumped into Asynchronous pieces of work that F# supports very well and can use the Network when it's available. During peak the Network looks something like this.

 ![The Requests for a weekend.](/images/PlaRequests.PNG)
 ![PLA Traffic](/images/PlaTraffic.PNG)

Clearly we are saturating the whole thing.


Keep in mind that Google PLAs are just a small part of our system. There are actually quite a few tech teams at Jet and they all have their own streams and data to manage. The reality is that there are millions of streams and millions of events in each one of those streams being fired every day at Jet.

I think what we are doing is unique and definitely not something I have seen done before, at this scale, not even in finance. Trying to do this sort of thing with some of the systems we had at the Bank would have been near impossible and taken months to deliver. The technology decisions made early on have allowed us to solve a very difficult problem in a very simple and robust way.