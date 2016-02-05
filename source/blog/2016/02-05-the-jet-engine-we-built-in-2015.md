@{
    Layout = "post";
    Title = "The Jet Engine we built in 2015.";
    AddedDate = "2016-02-05T11:14:03";
    Tags = "F#, Jet.com, Jet Technology, Jet.com Technology, Azure, Cloud, F#, Micro Services";
    Description = "";
    Image = "";
    PostAuthor = "Louie Bacaj";
}

---

![Jet Engine](/images/jetengine.PNG)

###So you want to build a massive online eCommerce platform?

>"Because it's probably harder to build a massive eCommerce platform than an actual Jet engine."

A lot of friends, and former colleagues, want to know what its been like to build Jet.com. I figured I would take a little time to talk about some of the things we've built as a team as well as reflect on the last year. I'd like to share some things that are working great, some of the things that aren't and what the game plan is for the new year; meaning what are we going to build next.

<!--more--> 
So although jet.com has had a colossal amount of media attention over the last year, we are still a pretty young company. Two years ago, this company didn't exist and less than seven month ago it hadn't launched and had under two hundred employees. Now we have over a thousand people and are growing at a blistering pace, with over two million shoppers on the platform. The media attention hasn't all been good and it's due in part to the staggering amount of money we have raised; it's made us as much a target as it has empowered us to deliver a tremendous amount of technological value.


That technological value is what I would like to talk about in this blog post and I will refer to it as the *Jet Engine*, which sounds a little cliche, but I assure you it's probably harder to build an innovative eCommerce platform in 2015-2016 that will scale to hundreds of millions of customers than it is to build an actual Jet Engine. *(Full disclosure: I've never actually built an actual airplane engine, so who really knows which one is harder to build, I bet you the airplane engineers never built a massive eCommerce platform.)*

---

###The tech stack.

The tech stack has been talked about [quite a bit](https://customers.microsoft.com/Pages/CustomerStory.aspx?recid=23392&utm_content=buffer1ba62&utm_medium=social&utm_source=twitter.com&utm_campaign=buffer), in a few places. We are using Azure as our cloud provider and as the saying goes "when in Rome do as the Romans". We have decided to build on top of the .NET technology stack. This stack continues to and has undergone a radical transformation in the last couple of years. That transformation towards open source has been amazing. We are already starting to see some of those benefits as we prepare to run .NET Micro Services on Linux boxes with support from Microsoft; that alone is going to be something else. Like most fads in technology, Microsoft is starting to come back full circle again these days. With all of the open source commitments Microsoft has made lately, dare I say the stack might actually be pretty cool again.

Look... the CTO and the lead engineers that were here from day one, didn't decide to use .NET because it was cool or use it in the traditional ways. Meaning they didn't pick up this technology and use C# to build things the same old way, which is to say using object oriented principles. Those principles were popular during the era of single core single computer applications. No, they decided to build this engine for scale from the get go and for the era of distributed computing.

You might be asking what does that even mean? And rightfully so, it sounds outlandish even to me when I say it. But hear me out because I believe in this. Most startups build an **MVP** *(minimum viable product)* and as they grow, they realize they need more. Twitter, for example, is famous for building everything in Ruby, hitting a massive brick wall called scale and turning right around and writing things in Scala. Many of you reading this will know Scala is a functional language and that functional languages tend to be stateless and some are even immutable first. Twitter was able to overcome these scale problems not by throwing more hardware at them but by using the right tools, in this case for massive distributed systems **Functional Languages** are the right tool.

>"...it sounds outlandish even to me when I say it but hear me out because I believe in this.... Twitter for example is famous for building everything in Ruby hitting a massive brick wall called scale....They were able to overcome these Scale problems ... by using the right tools...in this case... Functional Languages are the right tool."

Immutability at scale is really important because it allows you to go from **30k** members to a staggering **2.5 Million+** and not even feel blip within 3 months, like we did here at Jet from the end of July to October. **That's powerful!** I'm not saying we couldn't do the same thing another way, but I am saying we would have needed more discipline to not fall back into the old traps of having state in our Micro Services. All of our Micro Services *(or almost all)* are stateless, which means you put the same thing in you get the same thing out, not once or twice but every single time. In the math world they call this referential integrity and we try to stick to it because it works well for scale.

I believe this was the most forward thinking decision made so far and the end of 2015, during the holiday season, proved to be absolutely true. I am not taking credit for this decision because when it was being made, I was a SOLID principles man at another company. But by my teammates made a great decision. Like most of the people reading this blog post, I didn't know about most of this before I joined jet.com.  I learned it here at Jet and, more importantly, I now truly believe if you want to build for scale and do it fast you need to use right tools and F# proved to be the right tool for us. Whenever we build something we always consider scale as the top priority item.

>"I believe [using F#] was the most forward thinking decision made so far"

However, as it turns out, scale isn't the only good reason to use F#. The productivity it provides is unmatched and the lines of code needed to write something in F#, when compared to anything object oriented is something you can't compare. A fifty line Micro Service in F# may end up being hundreds of lines of code in any object oriented language, certainly true of Java or C# which I've used extensively professionally. So conciseness and the fantastic type system that pushes you toward correct easy to reason about code is another great reason why we picked F#. Besides the productivity, the massive amount of parallelism you get right out of the box with F# simply because you never need to worry about a mutex, a lock, or a race condition is something you can't even compare to in Object Oriented languages when writing fast code.

Azure as a cloud provider is also pretty damn good. Maybe at the moment not as good as AWS but that's not important. It's not that important because they are gaining ground every single day, and in some ways are even better for a price conscious startup. It is certainly comparable in feature for feature parity and at the end of the day every engineer knows it comes down to the code, not the hardware or the cloud provider **(see my Twitter example above)**. As a company looking to grow to hundreds of millions of shoppers, we were looking for a cloud provider that is going to be around for a while and grow with us.


So I've spent a lot of time talking about F# as it is baked in our core. However, we leverage so many other technologies. Our front end uses Node.js as the entry API to the consumer side of things. Lots of our internal tools that are built in house for member services and retail partners use React, Angular 2, and many other front end technologies supported by F# services in the back. We even use C# and ASP.NET, dare I say maybe soon even some vNext stuff without IIS, in many cases supported by F# Micro Services that house most of the logic and are the work horses. We have some beautiful Android and iOS apps as well.

>"[Jet] is an amazing place, trying to accomplish some pretty damn challenging and amazing things."

We use things like Redis for fast in memory cache, things like EventStore as immutable backend storage, HBase and HDFS, Azure SQL, Kafka and Azure Queues, Storm and even Spark. Among many other technologies that would be too much to talk about in this one blog post. The truth is each team will look into the things they are building and pick the best tools for the job. It's an amazing place, trying to accomplish some pretty damn challenging and amazing things.

---

###Building this kind of engine is not all roses.

So some of the things that haven't worked so well on the tech stack side of things are mostly related to our pain points and our outages. We've had a few outages, the consolation here is that none of these have been related to scale. Almost all of our outages were short blips that got resolved fairly quickly. The one thing we are pretty proud of is that during the highest volume shopping season, the holidays, when other eCommerce shops were dropping like flies we had no outages.

Redis has caused us a few pain points. More specifically, if you could look at our internal post mortem's it's been Azure Redis. However, as we have evolved our usage of Redis we are starting to see much better uptime. EventStore is a great technology, an immutable data store that goes really nicely with an immutable functional language. You can never update an event or record, only add more to it, which is really nice given storage is cheap and holding onto things forever is a really nice thing to have. When we run EventStore on SSD's its fantastic but not great on the cloud where network attached disks tend to have lots of failures. In some ways we are also to blame for using it as a persistence layer and as a Bus at times when we were projecting off of it.


The real **winner** here for us in the technology side has been **using F#** in that it's infinitely scalable in terms of the level of parallelism and productivity. At the same time, it gives us the blistering speed we need to build the core of this engine, which is a realtime pricing algorithm that tries to find the consumer the most savings it can find by pulling all kinds of cost out of the supply chain in realtime.

---

###The teams and the architecture.


One of the unique things about Jet, I think, is the amount of ownership they expect out of their people. That is to say engineers are expected to own the things they build, and business folks are expected to own the things they are working on. Teams build their Micro Services and architecture in a way they want and other teams consume from them or read from one of their immutable stream based systems. The level of ownership each team has serves as motivation and fosters an engineering first culture.

Ownership is very important to engineers, take a look at this post by Maxime where he explains [what ownership means](http://gingearstudio.com/why-i-quit-my-dream-job-at-ubisoft): 
>"No matter what's your job, you don't have a significant contribution on the game. You're a drop in a glass of water, and as soon as you realize it, your ownership will evaporate in the sun. And without ownership, no motivation."

This engineering first culture we are striving for at Jet, I would argue is much like what Google has tried to  create. This means that we as software engineers have to take pride in the things we build and are treated as owners in the business and we have to ensure the things we put out there in the world are of the highest quality.

As an example of this ownership; the pricing team which is known internally as Superman has a set of services and tools that they own. If you want to know about a price in realtime you ask Superman they are leveraging, the markets, the supply chain, and geographical factors among many other things to provide you a realtime price. You can get this by going to one of their RESTful end points, projecting from one of their realtime streams in Kafka. You have options like projecting that onto a database or an immutable data store. Obviously this is a powerful thing because if you want to get at this data programmatically you have multiple options. The ordering team, which is known internally as Gambit, similarly has a set of services and tools that they own. If you want to know about a members purchase history or look up some specific things about a user or order you have similar options. 

The other important factor here is the folks building these features own those exact tools and services so that when things go wrong they are expected to fix them. There isn't some bug team for you to dump your code onto. You and your team own the code you're writing and it's speed, testing, and quality are reflection of you and your team's abilities.


As another example there is a team, known internally as Flash, that is downstream consuming all of this massive amounts of data and trying to make sense of it. The data generated by all the other teams is aggregated and reporting is built on top of it in Flash. If you want reports or you want to answer tough business questions you go to Flash and if something is wrong then flash will fix it. Likewise the spiderman team provides data at scale and this data then informs the realtime pricing engine so that Jet is constantly trying to find you the cheapest price. There are dozens of teams doing extremely challenging and interesting things at Jet.com. I hope you are seeing the patterns here.

This type of technology and engine isn't trivial to build and each team has ownership and control of their architecture. An engineering driven company and culture is also not trivial to build. Areas where multiple teams are impacted are decided by a design review sessions that include all of the engineers and require all the teams to come to hash things out and come to an agreement. 

There are many technology teams at Jet; they all own their destiny and respective architectures, tools, and technology decisions. If you land on the Nova team your worried about products titles, descriptions, the overall catalog and it's data quality. You will build heuristics and rules to clean the data. If you land on the batman team you're in charge of the front end and the consumer facing experience, and the first line of interaction with the user.

I am sure you've seen the pattern already in that each team is named after some Superhero. It probably sounds silly but we take this very seriously as our code bases are named in this way. If you look at Thor you will see one of the best Warehouse Management Systems that's been built internally at an ecommerce platform. The Professor X team is in charge of search and is dealing with optimizing that set of services and building interesting things on top of that. 

Meanwhile if you happen to land on my team, Storm, you will be working on building the levers required for marketing to get things done. That means feeds, integrations, and lots of internal tools to give marketing the power they need to grow this Jet engine.

Jet.com at its core is a distributed, immutable, realtime system. At the very top level the architecture can be described as event sourced but it's much deeper than that. Building a near realtime distributed system has some challenges but coupled with our micro service architecture, F# as the core language, complexity has been greatly reduced. This means it's fairly easy and straight forward to add functionality. Which is a great thing given how fast this engine is moving and evolving.

---

###What's the plan going forward?

Given all of the exciting things we've already built, you'd be surprised to find out we have a ton of things still left to build. We will be focusing on better ingesting the massive amounts of data we are creating and focus on making that the lifeblood of our system.

Ultimately, we want to save the consumer as much money as possible by taking out as much supply chain cost as we can from the whole ecommerce experience. This isn't that easy to do. I think much of this has come together pretty nicely in **the smart cart technology** in that *the more you shop the more prices drop*. We will continue to grow this technology and iterate on it, make it more intuitive, make it more innovative and take into account all kinds of factors.

We will continue to leverage F# and try to build better personalization's and recommendations. We will empower merchants and brands to flow promotions more seamlessly through our system rather than having to enter promotion codes and other old and frustrating mechanisms.  Other things you can count on us doing is market to you like crazy in hopes of convincing you that Jet is the best place for you to shop.

Overall, Jet is simplicity on the side of a very complex industry. Jet started with the premise of pulling out as much cost as possible for the consumer and jet has the consumers back. The savings from not having brick and mortar stores isn't going to consumers in the current state of this industry, the savings that goes from shipping from the same warehouse and sending it all in one box isn't going to the consumer, unless of course you're shopping with Jet. So far we have demonstrated that this industry can have much more efficient systems. Our engine aims to save consumers lots of money. You can count on us improving and perfecting that part of the jet engine into 2016.