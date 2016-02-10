@{
    Layout = "post";
    Title = "Microservices & Messaging";
    AddedDate = "2016-01-26T10:49:42";
    Tags = "microservices, F#";
    Description = "";
    Image = "";
    PostAuthor = "Krishna Vangapandu";
}

Microservices are used heavily at Jet. We blogged about this in the [past](http://techgroup.jet.com/blog/2015/11-27-how-jet-build-microservices-with/index.html) and spoke about it in a [few](https://vimeo.com/109343720) [presentations](https://vimeo.com/144692770). Microservices communicate with each other through remote procedure calls or asynchronous messaging.

## The Jet Microservice template

Microservices in Jet typically follow the pattern described below.
 
1. A set of `input` that this microservice can understand
    
        type Input = 
            | Shipped of MerchantOrderShipped 
            | Cancelled of MerchantOrderCancelled

2. A set of `output` that this microservice will generate. 

        type Output = 
            | Update of Order

3. A `decode` phase that basically deserializes the incoming message on a pipe to an appropriate strongly typed input.

        let decode msg = 
             match msg.eventType with 
              | "Shipped" -> Input.Shipped(msg.payload |> toMerchantOrderShipped) 
              | "Cancelled" -> Input.Cancelled (msg.payload |> toMerchantOrderCancelled)

4. A `handle` phase that takes an input, runs some business logic to calculate what side-effects should occur and generates the `Output` accordingly.

        let handle = function
            | Shipped (mo) -> async {
                    //load the order
                    //update the order
                    return Output.Update(updatedOrder)
                }
            | Cancelled (mo) -> async {
                    //load the order
                    //update the order
                    return Output.Update(updatedOrder)
                }

5. An `interpret` phase that takes an `Output` and executes the side-effects that the output usually represents.

        let interpret = function
            | Update order -> async {
                    //store the updated order in the source-of-truth
                    //or send a message to another microservice queue
                }

<!--more-->

Note that the `decode -> handle -> interpret` pipeline itself may be made of some time-consuming or blocking operations which is to be fed with messages that the microservice receives. Some of us use [Reactive Extensions](https://msdn.microsoft.com/en-us/data/gg577609.aspx) to convert the incoming messages into observables which then gets fed into the pipeline. And a few of us use the [AsyncSeq](http://fsprojects.github.io/FSharp.Control.AsyncSeq/library/AsyncSeq.html) library to pull the messages from the stream. Either way, one can feed the incoming messages either 1) serially, 2) in parallel or 3) in some partial order (hybrid). 

## Communication between microservices
When it comes to communication between microservices, there are so many options to chose from. The simplest option is to have each microservice expose some form of an endpoint that the other can connect to and communicate directly. This may be a viable approach for having synchronous communication between services. As long as the services are connected to each other on the network, this approach can work. For the synchronous communication to be reliable, both the microservices (sender and receiver) should be running. Service-to-service communication begins to get tricky when one microservice need to send the same message to multiple other microservices. 

Asynchronous communication between microservices may be accomplished using message-passing. In most cases, some form of message broker is involved, which both the microservices talks to. The broker may provide additional features such as reliability, delivery guarantees, security, etc. This model can also help in PUB/SUB kind of scenario where multiple microservices may be interested in the same message. The broker can take care of routing the message to appropriate consumers. [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) is one such message broker which supports both traditional message queues as well publish-subscribe topics. [Kafka](http://kafka.apache.org) is another popular messaging platform.

## Checkpoint & delivery semantics
[Checkpointing](https://en.wikipedia.org/wiki/Application_checkpointing) is a common technique used to build fault-tolerant systems. In case of reading messages from a queue/topic, we checkpoint the current position in the channel so that if the consuming process goes down; it can resume at where it left off instead of starting at the beginning. When reading messages from the channel, if we commit the checkpoint position before actually processing it, we get `at-most once delivery`. If we commit the position after processing the message, we get `at-least once delivery`. In systems such as Service Bus, checkpoint is not done explicitly. Instead the consumer is responsible to "remove" the message from the queue. The delivery guarantee will then depend on when the message is ack'd/removed - before processing (at-most once) or after processing (at-least once). 

## Serial processing of incoming messages
The simplest case to reason about is when we process messages one after the other. While serial processing gives you simplicity, the trade-off is the increased latency (Queuing theory 101 states latency is the time spent by the message waiting to be processed). Also, one could argue that serial processing does not fully take advantage of the underyling resources. But if you have to maintain the ordering of messages, then you will need some form of serial execution. Scaling out a microservice such as this, where serial/ordered execution is absolutely critical, will have its own set of challenges. 

## Parallel processing of incoming messages
Processing messages in parallel, as fast as they arrive is ideal provided ordering of messages is not a requirement. Processing every incoming message as it arrives may reduce the latency and also help with efficient utilization of resources. The implementation may also warrant some careful examination. One could easily overwhelm the underlying threading system by spawning an unbounded number of threads or break the threadpool by having to manage an overly large pool of threads. Depending on the underlying implementation, one may even need to limit the max number of messages being processed in parallel. Moreover, this breaks ordering among the incoming messages. If ordering of messages is not critical, then one could have multiple instances of these microservices working off the same queue (in a competing-consumer fashion).

## Hybrid model - partial order of messages
A more realistic need may be to process all messages of a group (for example, a customer order) serially. Messages from different groups need not be executed in order. This gives the simplicity of reasoning as in serial execution - we know all messages of a customer order are processed one-by-one. This also brings in the efficiency of parallel processing. Now our microservice can process on multiple orders at once. We will still need to keep an eye on the parallelism at play. And the hybrid implementation will increase the code complexity as well.

The rest of the post discusses some techniques to do the hybrid processing of the messages on the queue.

## Partition for parallelism in the communication layer

Consider microservice `A`, which generates output messages that microservice `B` needs to process. 


     [A] ========== [B]

 
Instead of using one queue for communication; `A` can write to `n` queues. 


        ==========
        ==========
    [A]     .      [B]
            .
        ==========


Each message can then go to one of the `n` queues depending on how you hash out its group identifier (say Customer Order ID). We can then run multiple instances of the microservice `B` (atmost `n`) and have it read inputs from one of more queues. These instances can all be on the same machine, if needed. 


        ========== [B]
        ========== [B]
    [A]     .       .
            .       .
        ========== [B]

An advantage of this model is that the producing & consuming microservices can remain agnostic to overall setup of the services. You may still need to apply some additional techniques such as consistent hashing w/ virtual nodes to support a dynamic number of microservice `B` instances. But the downside remains that you will need to manage the `n` queues that are required for each `topic`. 

Kafka's model of a topic & partitions is a good fit here. The management of those `n` "queues" (actually, partitions) are handled within Kafka through partitions. The group ID of an outgoing message can be mapped as a partition key. The messages are then spread across multiple partitions while those of the same "group" are within the same partition. And a single instance of a microservice can process multiple partitions concurrently (within the same process) while maintaining order only within the partitions. The parallelism will then depend on the number of partitions you choose for a given topic.

## Parallelism within the microservice
Partitions are not free. As explained in the confluent [blog post](http://www.confluent.io/blog/how-to-choose-the-number-of-topicspartitions-in-a-kafka-cluster/), the number of partitions can have an impact on the overall system. For example, increasing the number of partitions will increase the number of open file handles, the stress on IO and more. So one would eventually settle on a large enough number of partitions that meets the needs for distributing the workload across multiple instances of microservices. 

If we need to push the boundaries further, we can use some instance local techniques to process messages within the partition in parallel and of-course maintaining order within the same group. We will look at a couple of ways to accomplish this..

### Parallelism using Actors
Each microservice instance can internally have "P" actors - one for each group. Each message will be sent to one of those "P" actors. Within the actor (`MailboxProcessor` in F#), it can perform the `decode -> handle -> interpret` cycle. If the rate of incoming messages is high compared to the rate of processing, the messages gets accumulated inside the actor's mailbox (internal queue). Eventually having an unbounded queue as an actor's mailbox leads to more memory consumption which may increase message loss when the process goes down. An additional problem with an Actor is that there is no knowledge of when a message is actually received and processed by the actor. This makes coordination of "read position" on partition/queue more explicit and complex. One way around this problem is to have these P actors send a message to a single actor "K" whose responsibility it is to just update the read commit. In this case, tracking the checkpoint problem can be solved but backpressure still remains a problem unless handled explicitly.

This post - ["Actors are not a good concurrency model"](http://pchiusano.blogspot.com/2010/01/actors-are-not-good-concurrency-model.html) offers more insights into additional factors to consider about the Actor model.

### Parallelism via Fork-Join model
Our microservice local parallelism needs can be expressed with the flow:
1. Read N messages out of a single partition (queue, whatever) 
2. Partition these N messages across "P" groups while maintaining order within each group
3. Process each group in parallel
4. Once all groups are completed, commit the read position to some f(last position, N).
5. Repeat.

The above flow describes a [Fork-Join concurrency model](https://en.wikipedia.org/wiki/Fork–join_model). Now in a reactive environment, we wish to process messages as they arrive, but still have some form of the above model. 

#### a highly opinionated simplification
Translating the above model, while further simplifying to reduce complexity, the incoming stream of messages can be processed as such:
1. As a message arrives, check the current state to see if it can be processed immediately. If not, "block" on the task that is processing a different message on the same group.
2. If yes, then check to see if we have reached the max level of concurrency. If yes, "block" until one of the other tasks are completed.
3. If no, then spawn a task that processes this message. Update the state to include this new task.

Consider the flow below.

                                         | -> ... a2  
    [a6 a5 a4 b6 b5 b4 c6 c5 c4 a3 ] ->  | -> ... b3
                                         | -> ... 
             [incoming messages]    <-   { ->> state <<- }

On the left side of the flow, we have the incoming messages. On the right, each `-> ...` indicates an execution and these executions forms the current state. The messages labeled with the same character belong to the same group (eg: a1, a2, a3 are one group and b1,b2, b3 form another group, etc). Order must be maintained within the group but messages across the groups can be executed in any order. From the current state, notice that `a2` and `b3` are being executed. The next message - `a3` cannot be executed since another message of the same group `a2` is actively being processed and thus we block until `a2` gets processed completely. 

In the simplified model, we will not move to `c4` until the message before it (`a3`) gets dispatched for execution. Note that, when we extend this simplified model to support putting `a3` in some of queue and move over to the next message `c4`; we mimic the actor-style processing of messages mentioned in the previous section.

The simplified model does bring in additional challenges around accurately committing the read postion to get an `at-least once` delivery semantic. A simpler alternative would be to return back the Task that is processing the message and then set a continuation on the task that writes the message's position in the log/partition/queue as the commit-position. In some cases, when you restart the service, you may end up reading in duplicate messages. But still within the same `at-least once delivery` semantics.

So let us see what an F# function that handles the above model may look like:

  runParallelOrBlock 
            //specify the level of parallelism
            (parallelism:int)                 
            //can two inputs run simultaneously?
            (canRunTogether:'input -> 'input -> bool)
            //current state
            (state: (Task<'b>*'input) array)           
            //action to execute on the input
            (f:'input -> Async<'b>)         
            //current input           
            (args:'input)                              
            //task for the current input, new state
            : Task<'b> * (Task<'b> * 'input) array     

The functional way of managing state is to pass the current state as a parameter and then have the function return back the updated state. 

    let (result, updatedState) = update currentState currentArgs

We follow the same pattern for our function `runParallelOrBlock`. A template implementation for such a function can be as:

    let runParallelOrBlock parallelism canRunTogether state f args =             
      //clean up the state to remove any tasks that are already completed.
      let newState = (state |> Array.filter (fun (x,_) -> not x.IsCompleted)) 
      
      //check if we should block
      let doBlock = block canRunTogether state args
      
      //if we are to block, then we block. Otherwise, we will proceed to execution
      if doBlock then          
          waitForCompletion state |> ignore
          //when we reach here, we are good to go
          let task = execute f args
          task, [||]
      else
          let rec innerRun (current:_ []) = 
              let currentLimit = current.Length
              //if we are at the concurrency limit, we block. Else we execute
              if currentLimit < parallelism then
                  let task = execute f args
                  task, Array.append current [|(task, args)|]
              else
                  //we are at concurrency limits, wait for any task and try again.
                  waitForAny current |> ignore
                  current 
                  |> Array.filter (fun (x,_) -> not x.IsCompleted)
                  |> innerRun
          
          innerRun newState

We can delegate the responsibility of managing the state to the consumer of this function. 
The usage can be something like :

    let task, state = 
        runParallelOrBlock 8 canBeParallel [||] (handle >> Async.bind(interpret)) currentMessage

So the caller can keep track of the `state` returned and reuse it during the subsequent calls. The state in this case is basically a collection of running tasks.
We can have the `runParallelOrBlock` take care of cleaning up the state so as to remove any tasks that are completed. We can also use the `task` and the `input` together to decide when to set the commit position. 

As mentioned earlier, if we look at incoming messages as a stream, an [FRP](https://gist.github.com/staltz/868e7e9bc2a7b8c1f754)-style of state management would be to `fold` over the stream. The `fold` takes care of maintaining the state and passes the latest state for the next iteration. The function passed to the fold is expected to return an updated state back. `IObservable<T>` is one way to have a stream of messages and the Rx API has support for functional folds.

    //functional style to keep state around is to fold over the inputs and keep the latest state.
    //hence the aggregate
    let initialState = [||]               
    stream.SubscribeOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
            .Aggregate(initialState, 
                        fun state (input,_) -> 
                        let (_,newState) = runner state processInput input
                        newState)
            .RunAsync(System.Threading.CancellationToken.None)


Note that the above implementation is highly opinionated & simplified. But it can still be made to support any of the three ways to process the messages - serial, parallel, hybrid. To see how, lets define some helper functions.

    module Parallel = 
        let Always = fun _ _ -> true
        let Never  = fun _ _ -> false

Now serial processing of messages can be as:

    let serialExecute = 
        runParallelOrBlock 1 Parallel.Never

Always parallel processing of messages can be as:

    let parallelExecute = 
        runParallelOrBlock Environment.ProcessorCount Parallel.Always


Additional concerns include testability, debuggability and logging. For example, logging from those actions running concurrently is a little tricky. In the simplest case where logs from all the actions goes to the same logger; statements from one action will be interleaved with those from the others. This makes log analysis tricky unless we log additional context/group information to the log statement.

# Conclusion
Microservices and asynchronous messaging comes with an interesting set of challenges. While scale-out is one of the ways that message consumption throughput may be increased, it is worthwhile to have individual services be as efficient as they can be in processing the incoming messages. Message-ordering poses additional restrictions on when a message gets processed in addition to the existing challenges of reliability and fault-tolerance. A couple of ways to tackle this problem have been presented here - within a single service and across multiple services.

*A big thanks to Jeremy Kimball, Scott Havens, Gad Berger and Rachel Reese for their suggestions to improve this post.*