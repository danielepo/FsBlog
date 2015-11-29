(*@
    Layout = "post";
    Title = "How Jet Build Microservices with F#";
    AddedDate = "2015-11-27T12:27:02";
    Tags = "f#, microservices";
    Description = "";
    Image = "";
    PostAuthor = "Rachel Reese";
*)

(**
Happy first day of F# Advent! 

I've had a lot of questions lately about our microservices, and how we're using F# to build them. So, I wanted to take this opportunity to go a bit more in-depth on what they look like. Let's start in the most basic place: 

### Definitions
What do microservices even mean to us? We belong to the SRP camp: 

A microservice is an application of the single responsibility principle at the service level. 
==================

This means that a microservice should have one -- and only one -- reason to exist. Not necessarily that there should be only one function within the service, just that the microservice should have only one job.

We also note, generally, that we should strive to create microservices that have an input and produce an output. This will eliminate many side effects, and considering up front the inputs, outputs, and transformations that the service will have, helps us to do so. While this isn't always possible (logging and sending emails happens in the real world, but when they do, they should be isolated to their own service), abiding by this rule goes far in eliminating unnecessary side effects. 
*)
(*** more ***)

(*** hide ***)
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/NLog.dll"
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/EventStore.ClientAPI.dll"
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/Newtonsoft.Json.dll"
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/Marvel.FSharp.dll"
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/Marvel.Json.FSharp.dll"
#r "C:/Jet-code/PriceCheckNile/PriceCheckNile/bin/Release/Marvel.EventStore.FSharp.dll"

module Types = 
    open Marvel.Json
    open FSharp.Data
    open FSharp.Data.JsonExtensions
    open Newtonsoft.Json
    open Marvel.EventStore

(**
### Benefits
The benefits of microservices fall into three main categories: 

1. _Easy scalability._ It's only a matter of scaling a single service as needed. Did we receive a large shipment to the warehouse today, putting the related services -- and people ;) -- under heavy load? We can scale out just those services without regard for the rest of the system. 

2. _Independent releasability._ While it is possible to release a single service at once, we organize ourselves into teams, and our microservices into groups within our teams. Because we're using F#, each group of microservices is usually within the same Visual Studio solution. When we talk about independent releasability, we usually mean that we'll promote an entire solution of related services -- often a group of 5-10 -- at the same time.

3. _A more even distribution of complexity._ By this, I mean that it generally becomes much more simple to create and maintain your services, but that it tends to be more difficult to manage all your services. Recently, during Jet's [Tech Talk Weekly]( http://techgroup.jet.com/blog/2015/07-14-tech-talk-weekly/index.html) series, we watched [Tammer Saleh giving his "Microservices Anti-Patterns" talk]( https://www.youtube.com/watch?v=I56HzTKvZKc). One of the anti-patterns he mentioned was doubling down on microservices as a pattern and creating an application from the ground up with microservices. He suggests instead starting with a monolithic application and breaking off services as it becomes obvious that they're needed. This isn't how we proceeded at Jet, so I won't speak to it as a piece of advice, but I will add that because of the additional layers of complexity added with needing to manage your services, it's crucial to at least have your management story thought through, if not fully determined, right up front before you start to break off those services. 

### Communication
We use a couple different methods for our microservices to communicate: Kafka, EventStore, and Azure ServiceBus with Azure Queues. For my examples today, I'll show off EventStore. However, in many cases, we are starting to move away from EventStore in favor of Kafka, as it is more in-line with our current needs, specifically: very low latencies and strict message ordering from the producer. 

### Code! 
So without further adieu, I present some example code. In this example, I've created a microservice that price checks a product on a competing site, in this case, the "Nile" shopping site. ;-) 

We won't get very far if we don't define our types up front. Let's start with a ``Product`` record type that contains information about the product we want to price check. 
*)
    type Product = {
        Sku : string
        ProductId : int
        ProductDescription : string
        CostPer : decimal
        }
(**
We also need some additional members on the ``Product`` so that it's easily serializable: ``ToJson`` and ``FromJson``, which convert a ``Product`` record type to and from ``JSON``, using one of our internal libraries. We also want to create a type for the failure case. For that case, we'll need just the product id that we were trying to price check, and the failure message. 
*)

    type Product with
        static member ToJson(x:Product) =
            jobj [|
                "sku" .= x.Sku
                "productId" .= x.ProductId
                "productDescription" .= x.ProductDescription
                "costper" .= x.CostPer
            |]

        static member FromJson (_:Product) =
            parseObj <| fun json -> jsonParse {
                let! sku = jget json "sku"
                let! productid = jget json "productId"
                let! productdescription = jget json "productDescription"
                let! costper = jget json "costper"
                return {
                    Product.Sku = sku
                    ProductId = productid
                    ProductDescription = productdescription
                    CostPer = costper
                }
            }
        static member EventCodec : EventCodec<Product> = jsonValueCodec "product"

    type PriceCheckFailed = {
        ProductId : int
        Message : string
    }

(*** hide ***)
module service = 
    open System
    open EventStore.ClientAPI
    open Marvel.EventStore
    open Marvel
    open Types

(**
Once we have our types set up, we can start to construct the service itself. We tend to use a similar format for many of our services so that developers can get up to speed more quickly, as well as move across teams faster. Services have five main sections. 

First, we set up our inputs and our outputs. Here, we are taking in the ``Product`` type that we just defined, and we need to return either a tuple of ``Product`` type and a ``decimal`` cost, or a ``PriceCheckFailed`` type, that will contain information about the reasons we can't currently check this price on the Nile site. 
*)

    type Input = 
      | Product of Product

    type Output = 
      | ProductPriceNile of Product * decimal
      | ProductPriceCheckFailed of PriceCheckFailed

(**
Second, we define how to handle the input -- specifically, how to convert from the input to the output. In this case, I've cheated and constructed a tuple that fits the successful output type. However, this is the section of the microservice that would contain the longest bit of code, going out to the competing site and gathering the information, say through their API. Note that we're also using ``Option`` types, so that we have an additional check for failure. This will become important in our next step. 
*)
    let handle (input:Input) =
        async {
            return Some(
                ProductPriceNile(
                    {Sku="343434"; 
                     ProductId = 17; 
                     ProductDescription = "My amazing product"; 
                     CostPer=1.96M}, 
                    3.96M))
        }

(**
Third, it's necessary to define how to interpret the output once we have received it. Here, we pattern match on what we received from the ``handle`` function. If we receive a successful response, we write to Kafka or EventStore. If we receive a failed response, we might log that failure and try again. The final case takes into account an unanticipated failure where we get no reponse at all, so we might log that failure, then raise an alert to the teams involved. By using an option type here, we have a third check -- for an unknown and unanticipated failure -- that would not have otherwise been used. 
*)
    let interpret id output =
        match output with
        | Some (Output.ProductPriceNile (e, price))  -> async {()} // write to event store, kafka, etc. 
        | Some (Output.ProductPriceCheckFailed e) -> async {()} // log specific failure and try again
        | None -> async {()} // log failure and raise alert

(**
Fourth, we gather these functions into a ``consume`` method. This calls another ``consume`` method in one of our shared libraries, passing in a decoded input, as well as our ``handle`` and ``interpret`` methods. 
*)
    let consume = EventStoreQueue.consume (decodeT Input.Product) handle interpret

(**
Finally, we can subscribe to a specific event stream, calling our ``consume`` function for each event we receive. 
*)
    EventStoreQueue.subscribeBufferedWithCheckpointStream 
        (EventStore.connHost "MyEventStoreConnection") 
        "$myeventstream" 
        true 
        500 
        100 
        (TimeSpan.FromSeconds 1.0) 
        "Check_price_on_Nile"
    |> AsyncSeq.concatSeq
    |> AsyncSeq.iterAsyncParThrottled 200 consume

(**
### Conclusion
I've set up an example to show off the style we use at Jet for our F# microservices code. We aim to keep our microservices set up in the same manner across projects so that folks are easily able to transfer teams, as well as to get up to speed when they first start at Jet. Hopefully the tools and techniques outlined here can help you and your company get started with microservices in F#. 

*)
