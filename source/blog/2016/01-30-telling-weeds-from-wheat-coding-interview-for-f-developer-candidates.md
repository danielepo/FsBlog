@{
    Layout = "post";
    Title = "Telling Weeds from Wheat: Coding Interview for F# Developer Candidates";
    AddedDate = "2016-01-30T03:49:06";
    Tags = "F#, interview";
    Description = "";
    Image = "";
    PostAuthor = "Gene Belitski";
}

Life is full of extraordinary coincidences.
Couple of years ago my job was managing few folks like this [valuable member of F# community](https://github.com/dmitry-a-morozov). I was toying with F#
just for the sake of not looking a complete idiot while communicating with my subordinates. As my F# skills were insufficient for messing with production code
I was diligently working through [Project Euler](https://projecteuler.net/) tasks at my leisure time and capturing my modest achievements into this
[YAPES](https://infsharpmajor.wordpress.com/) (Yet Another Project Euler Series). It took me about a year to grok expressing myself in F# along solving the first
90 problems of Project Euler in more or less idiomatic way, thanks to colleagues’ helpful critique. 
 
Being already able "thinking functionally" I once bumped into [this book](http://www.amazon.com/coding-interview-problems-Google-solutions/dp/1482799014).
The book itself wasn't great, but it prompted a hypothetical setting: what if Google would be hiring F# programmers?
Then how solutions to (supposedly) top 10 Google coding interview problems may look like in idiomatic F#? I’ve posted my
[solution to the first of the problems](https://infsharpmajor.wordpress.com/2013/04/24/if-google-would-be-looking-to-hire-f-programmers-part-1/) and then all of a sudden
number of visits to my blog skyrocketed. Came out my post was captured by [F# Weekly](https://sergeytihon.wordpress.com/category/f-weekly/) and followed by
[Don Syme's](http://research.microsoft.com/en-us/people/dsyme/) twit, so significant portion of active F# community took a peek too.

Today is 2016, for the second year my full-time job is F# developer, and [Jet.com is really hiring F# programmers](http://stackoverflow.com/jobs/companies/jet-com). Beats me, who could predict?
<!--more-->

Anyway, as I still recall details of my own transformation into F#-er I share below some my thoughts on how
to recognize a future F# developer during a 45 minute coding interview session.
Is this doable? How the process may be structured?

When a candidate claims existing F# skills, the problem suddenly gets simpler. Interviewer’s task is just to see if the claim is genuine and how far the skills reach.
This should take around 10 minutes. 
Much more challenging is the case of recognizing *potential* F# programmer when the list of claimed skills is plain and simple C# or Java. Then what to look for? Whom to avoid?

In my humble opinion the most important quality to seek for is candidate's mental ability of bending without breaking. Interviewer should find out
to what extent the following two casting quotations are applicable to the candidate:

> [The determined Real Programmer can write FORTRAN programs in any language](http://web.mit.edu/humor/Computers/real.programmers).

and (originally about students that have had a prior exposure to BASIC)
> [... as potential programmers they are mentally mutilated beyond hope of regeneration](https://en.wikiquote.org/wiki/Edsger_W._Dijkstra).

You may substitute FORTRAN and BASIC to your taste, we are not talking about concrete expressive tools, but on scraping off arrogant or blinkered mindsets.

Personally I conduct these interviews by the lines of this practical mantra [“Make it work, make it right, make it fast”](http://c2.com/cgi/wiki?MakeItWorkMakeItRightMakeItFast)
in the following 2-step manner.

#####Step 1 – Offering candidate a trivial problem, but having both breadth and potential

By *breadth* I mean an opportunity to reveal the due diligence in coding (“make it work”, and, to some extent, “make it right”).
The code authored by the candidate **must solve** the offered problem. Corner cases must be covered. If the task description allows for too much freedom, further details must
be requested.

Surprisingly significant amount of candidates cannot overcome *Step 1/Breadth* phase. They burn the full duration of the interview painfully and gradually approaching the
sought dozen lines of C# code. Hardly understanding interviewer's prompts and clues. Fixing bugs, but making new ones. Undoubtfully these folks are not good enough to be hired.

If the whole *breadth* portion is satisfactory covered by the candidate in around 10-15 minutes, then I turn to digging for *potential*.
By *potential* I mean the candidate's ability of moving from
just some solution to the perfect solution. This part sits somewhere between “make it right” and “make it fast” and involves discussing performance. Candidates
should be able estimating how performant is their code and improving its performance. Another big chunk of candidates fails to make this portion of the distance towards the
interview passing grade.
Those who got “almost there”, those who completed *Step 1*, but the time has ran out. In this case the interview pass-or-fail decision is fully at your discretion
and coming up with unequivocal recommendations is hard. Recall the details, check your records, and just decide!
If you have a gut feeling that the candidate might be good, then don’t hesitate to go for it, in the end there is still a face-to-face phase ahead for making the final corrections.

If a candidate have completed *Step 1* in 20 minutes or faster, most likely the candidate is good. Now interviewer’s task is to find out how much good. Time for *Step 2* comes.

#####Step 2 - Offering another rather trivial problem, but with a surprising twist

The twist might be imposing an unexpected frames upon the implementation. Usually those who quickly find the way of overcoming the limitation do deserve firm passing grade
and face-to-face interview in the future. Those who fail still might be considered at your discretion.

In order to illustrate the above said with concrete examples I provide below the sample tasks for *Step 1* and *Step 2* of coding interview. I took the freedom of showing F# solutions
and leaving porting to C# or other language of choice for you as an exercise (-8. The same applies to improving the given solutions.

#####Step 1 Sample Assignment: Power of a Number

Implement a library function **power(float base, int exp)** that calculates a power **exp** of a value **base**. That is, power **exp** of not negative float number **base**
for not negative integer **exp** is **base** multiplied by itself **exp** times.

Why such assignment may be good for the purpose? To begin with, it requires a candidate to show a fair amount of attention to details right away.
Really shrewd folks may immediately ask about the valid value ranges for **base** and **exp**. Others (either math inclined, or just reckless)
jump into the coding without any reservations, in 99.5% of cases eventually producing a cycle where **base** gets multiplied by itself **exp** times. Here the task *breadth*
should kick in and you may observe how many times on the scale of 1 to at least 3 the candidate tends stepping on the same rakes taking care of all corner cases.

If and when the corner cases and argument validity are satisfactory covered the time comes for candidate *potential* determination. Ideally the candidate should be able
to achieve performance improvement from **O(n)** to **O(log n)**. For the reference - not perfect, but a good enough implementation probably deserving
a face-to-face invite may look close to this:

![power.fsx](/images/Power.png)

As expected it should take just 10 multiplication steps to calculate a number power 1000. I leave for you to spot and perhaps correct few blemishes that the above code continues to carry.

#####Step 2 Sample Assignment: FizzBuzz? Oh No, Not Again!

You may challenge unsuspecting candidates with the following twist of good ol’ [FizzBuzz test](http://c2.com/cgi/wiki?FizzBuzzTest)
– implement it for any sequence of **n** consecutive numbers starting with positive integer **s**. For the twist – the use of division and modulo operations is prohibited.

Feel free and try challenging yourself with solution prior to looking at suggested one below:

![fizzbuzz.fsx](/images/FizzerBuzzer.png)

Assignments of such sort provide plenty of opportunities to explore candidate’s skills and ways of thinking, indeed. *Step 2* code also might be transitioned to
face-to-face phase of the interviewing process for further discussion.

Wrapping it up: I do believe that thinking over the coding interview along the lines suggested above may improve the quality of hiring decisions.
I wish you enjoyable interviewing that brings onboard great F# developers.