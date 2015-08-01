@{
    Layout = "post";
    Title = "On how Jet chose F#";
    Date = "2015-03-22T12:38:14";
    Tags = "F#";
    Description = "";
    Image="";
    Author="Rachel Reese";
}

## In the beginning

The first Jet prototype was written by our CTO, Mike Hanrahan, in his basement, in WCF. After the first developers were brought on, the first proper version of our site was spun up with CoffeeScript. However, Mike knew from the outset that he wanted to use F#, at least for the pricing engine, because of the benefits you often hear about F#: less code leading to fewer bugs, as well as code that is more robust and safer because of the type system, to name a few. As a company, we weren't sure that we should have F# anywhere else, though. After all, we're building an e-commerce web site. Isn't the web fully in the domain of C#? Plus, many of the developers on the team had an object-oriented or imperative background -- what if we wasted too much time figuring out this new language before we could even be productive?

So, we started building two solutions: a C# solution and an F# solution, to see where they would take us. In the end, we chose to stick with the F# path. The main reason: we were able to deliver the same functionality with far less code. This clearly eases maintainability and reduces bugs. If you've been part of the F# community for any length of time, you know that this is a very well known feature of the language and a commonly cited reason to switch to F#. However, we had a few other unique-to-Jet reasons that we wanted to share.

<!--more-->

## The user groups & start-up scene

In general, the developers who attend user groups do so because they are passionate about a certain technology, and they want to learn more by engaging and networking with the local community (this is certainly true of most of the user groups I've attended), but as we looked at F# user groups -- both locally in New York City and worldwide -- we realized something important: the number of attendees who were full-time F# developers was significantly lower than for other functional user groups.

Those of us who attend the F# user groups are not doing so to learn new tools for our day jobs, we're attending as hobbyists whose day jobs require other languages. We're driven by our passion for learning, for the language, and for the community. However, there is also a frustrating inability, in many cases, to find a day job that embraces the language and community about which we're so passionate. It struck us at Jet that these are exactly the types of engineers that any company would want to hire.

[Personally, as a developer, Jet is exactly the kind of company that I was looking to work for: a company that is passionate about technology and one that wants to invest in and build both our own developers, and the local community, as the company grows.]

In the same vein: as we looked around at the start-up scene in New York City, we noticed a surprising lack of start-ups betting on F#. The Scala and Clojure communities (for example), while larger to be sure, were no less passionate about their respective languages, and each community had several start-ups competing with each other over top developers. Less competition over top developers can only mean good things for Jet.

## Code
### Aspect-oriented programming

Like many large-scale projects, we needed a way to handle validation, logging, and other cross-cutting concerns. Traditionally in ASP.NET MVC, this is done through the use of Action Filters. To apply a filter to a single function (or a controller), you would simply decorate it with the relevant attribute. ASP.NET will then inject an extra call before your function call to handle authorization.

For example, to ensure authorization for an entire controller named MyController, you could use the following code:

	[lang=csharp]
	[Authorize]
	public class MyController
	{
	    ...
	}

An extremely similar implementation occurs for ASP.NET Web API: using attributes to inject an extra function call before your function. Other frameworks also utilize similar approaches. However, in Jet's case, we needed this same ability to handle cross-cutting concerns for services <strong>not</strong> based on HTTP. Unfortunately, after several attempts, we discovered this would prove extremely difficult to do in a generic way. Once we began to look to F#, we realized we would be able to cover our needs easily. As a bonus, we found we could do it in a way that neither pollutes the codebase with fabricated interfaces, nor requires adapting a specific architecture. And so we introduce:

### Jet's Filters Module

In our filters module, rather than using attributes, we take a much more functional and composable approach to AOP. Any filter in our module can be used to decorate functions that have type `'Input -> Async<'Output>`, and all filters in our module are composable, using our ``andThen`` operation, like so:
	
	let compositeFilter = filter1 |> Filter.andThen filter2

For example, if you had a service that simply echoes its output:

	let echoService : Service<string, string> = fun str -> async.Return str

and a filter that will print messages before and after a service is run:

	let printBeforeAndAfterFilter : Filter<string, string> = 
		Filter.beforeAfterSync (fun _ -> printfn "before") (fun _ -> printfn "after")

then, we can simply compose the two, and the messages will print before and after the service is invoked:

	let filteredEchoService : Service<string, string> =
    		printBeforeAndAfterFilter |> Filter.apply echoService

This is clearly a very simple, intuitive, and effective way of handling filters. 

### Category Theory

And now, as a bonus for those of you who stuck with us this long: it happens that a filter is just a kleisli arrow specialized to the continuation monad.

![Monads. Monads everywhere.](/images/monads-everywhere.jpg)
