@{
    Layout = "post";
    Title = "Software is Changing the World: Qcon London 2016";
    AddedDate = "2016-03-18T01:38:53";
    Tags = "";
    Description = "My recap of Qcon London 2016";
    Image = "";
    PostAuthor = "Nora Jones";
}

---
Last week, I had the pleasure of attending QCon London with Rachel Reese. For those unfamiliar, QCon is a conference with the purpose of "facilitating the spread of knowledge and innovation in the developer community." However, it's not just for developers, a lot of the talks were given by and for architects, directors, and project managers who influence innovation in their teams. The conference was packed with folks from all over the world - both industry and academia, discussing advances from a breadth of topics in the tech world, all with a huge diversity of experiences. To name a few, there were speakers from Netflix, Google, Slack, Uber and this really interesting new company called Jet.

Rachel gave a presentation about a project Ido Samuelson and I worked on involving Chaos Engineering in F#. She did a fantastic job explaining how the microservices architecture created here at Jet enables us to fail often and fail fast (I promise this is a good thing) - and how we are using Chaos Engineering to enhance that.

![RachelChaos](/images/RachelChaos.jpg)
![RachelNoraChaos](/images/RachelNoraChaos.jpg)

Side Note: the entire list of talks are on this  and if you're interested in watching any of them, please let either me or Rachel know and it can be coordinated with Lauren Havens.

There were several amazing talks over the span of the 3-day conference, but there were a few standout ones that resonated with me and contained practices that would be useful here at Jet; I've provided recaps of below:

## Talk: Monkeys in Lab Coats -  Applying Failure Testing Research at Netflix
### Presenter: Kolton Andrus (ex-Netflix, ex-Amazon, current "Chaos Engineer") and Peter Alvaro (ex-Berkeley, ex-Industry), passionate about building resilient systems and loves breaking things on purpose.

Collaborations between industry and academia are rare. In this talk, Kolton and Peter present their experience: a fruitful industry/academic collaboration. They describe how a "big idea" -- lineage-driven fault injection -- evolved from a theoretical model into an automated failure testing system that leverages Netflix's state-of-the-art fault injection and tracing infrastructures. This was an amazing talk, and by far my favorite of the conference - hopefully we can set up a lunch-viewing of this talk at Jet soon.

### Key Takeaways
>"Work backwards from what you know. Meet in the middle. Let the theory come to fit the reality."

* Collaboration in failure testing, when done carefully can be a multiplier.
* You obviously want to avoid playing "murder mystery" across your microservices at 3 AM, but how?
* One thing done at Netflix was bounding potential impact of a failure test and scope potential impact to an individual user/device, then scale it at a percentage of all users from 1% to 10%, and eventually to 100% and gracefully degrade - this is how you test in Production.
* Prove that it works - show that it scales - find real bugs.
* Using your intuition, what do you think are the weaknesses of the service? = Not automatable, gets easier the more people you get.
* Fault tolerance is really, just redundancy and it's all about being able to spot redundancy.

![Redundancy](/images/redundancy.png)

* A system that is fault tolerant must provide more ways of getting to a “good” outcome than we anticipate there will be failures.
* Netflix begins with a “good” outcome – meaning why is this good thing good? In order to to that, they dive into its lineage:
..*Look at the computational steps that led to the outcome.
..*Determine what could have gone wrong – faults are “cuts” in the lineage graphs.
..*Determine what would have to go wrong and end up with a boolean formula.
* Know that no two requests are exactly alike when evaluating the system.
* Adapt the theory to the reality within failure testing, without compromising the vision.
* If you don’t know what your customers are seeing, you will not understand the potential outcome.
* Mixing academia and industry can end up with a very successful solution.

## Talk: Engineering You
### Presenter: Martin Thompson – Developer with over 2 decades of experience building complex and high-performance computing systems.

Martin’s talk was about what makes a good software engineer and dove into practices and techniques that can help bring out the best engineer in you. As a Software Engineer on the productivity and internal tooling team (Forge) – I was especially interested in this talk.

### Key Takeaways
>"We are living in the era of software alchemy."

On the development process…

* The design process is an iterative one.
* Decompose the complexity of what you’re dealing with.
* Reliability is a design issue.
* Tackle unknowns first.
* If you don’t understand associativity, you’re going to run into trouble.
* The more things we have talking to each other, the more we have to learn how that data is exchanged.
* Automate repetitive tasks, focus on feedback cycles and experiment a lot.
* Revisit&Refine.
* Engineer for failure and make war on complexity.
* The rules of abstraction are the same as the rules of fight club:
	1. Don’t do abstraction
	2. Don’t do abstraction

On working together…

* More than 20 programmers working on a project is usually disastrous.
* We are all a product of our own experiences.
* The people that are the best at developing code are the ones that understand what the customer needs, and then doing that – this requires actually talking to the customer.
* If you only work on your own, the sphere of your development is incredibly limited – raise the bar. If you have one thing to influence your career, go find good people to work with.
* Don’t think of lines of code writing, think of lines of code spent.

![EngineeringYou](/images/EngineeringYou.png)

## Talk: Staying in Sync: From Transactions to Streams
### Presenter: Martin Kleppmann – Researcher at University of Cambridge Computer Laboratory, where he works at the intersection of databases, distributed systems, and information security. He previously founded and sold two startups, and worked on data infrastructure at LinkedIn.

Martin Kleppmann explores using event streams and Kafka for keeping data in sync across heterogeneous systems and compares this approach to distributed transaction: What consistency guarantees can it offer, and how does it fare in the face of failure?

### Key Takeaways:

> “Go away from interactive R/W transactions and move towards ordered event logs.”

* True colors in systems are revealed only when they fail.
* The problem with having so many moving parts, is that different data storage systems are not really independent from each other. Infrastructure is storing the same data, but in different forms.
* Application logic keeps most things in sync – however, race conditions can inevitably occur…
	* Two different clients are changing variable X at the same time, causing the writes to arrive in a different order
	* This is perpetual inconsistency until someone comes and overwrites this value
	* What if a write fails and the data store “blows up”?
	* If you want to coordinate two writes with each other, normally we wrap a transaction around the two  – so either both writes happen, or no writes happen and the database simply takes care of this. This is how we’ve typically been handling this for the last 40 years. However, if these are in two different data stores then you don’t have the ability to just wrap the transaction around the two.
* Transactions that cross multiple different storage system…
	* 2-phase commit: begin transaction and then continue reading and writing data stores as you’ve been normally.
	1. Prepare all of the contents and all of the writes that have happened,
	2. Commit – coordinator receives responses from various participants and commits it.
	* How does this solve the race condition issue? By itself, it doesn’t. You need locking in addition – locking any records until the transaction commits. Result is that the second client gets delayed until the first client finishes.
	* If you manipulate how the clients are maneuvering you could end up in a deadlock where both clients are stuck waiting for each other.
	* What if the client suddenly dies? Then the transaction manager and the data stores cannot talk to each other. However, they’re still promised to commit, then all of the data stores are still sitting there because they cannot abort of their own accord. Therefore, we cannot safely timeout the locks in this case.

![Martin1](/images/Martin1.png)

* Can we just make sure everyone applies the writes in the same order?
	* Keep it simple, stupid. (KISS).
	* Totally ordered sequence, append only. (Not a new idea: this is State machine replication).
	* Single writer principle (Thompson 2011)
	* Event sourcing (Vernon 2013) – why not just have have immutable events?
	* You need a replication process from leader to follower. The followers watch a replication log for any new writes that are appended to it. This ensures there is no failure when the network fails, because the leader only appends to the end.
* Kafka:
	* A message broker for log aggregation for web servers and partitions logs across multiple machines aka: if you lose a machine, you don’t lose any data.
	* Each partition is consumed sequentially in a single-threaded manner.
	* Line up the messages in total order.
	* Take advantage of Kafka changelog compaction feature.

![Martin2](/images/Martin2.png)
![Martin3](/images/Martin3.png)
