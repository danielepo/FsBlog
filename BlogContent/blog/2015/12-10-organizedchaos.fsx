(*@
    Layout = "post";
    Title = "Organized Chaos with F#";
    AddedDate = "2015-12-15T03:21:57";
    Tags = "f#, chaos engineering, performance engineering";
    Description = "";
    Image = "";
    PostAuthor = "Nora Jones";
*)


(**
<img src="/images/chaosmonkey.png" style="width:400px;border-style:none;background:transparent;" />
While we aim to increase productivity and efficiency among our engineers, we also want to ensure that we are prepared for anything (READ: we need to break lots of things before we release to the live production environment with which our customers interact) as a result, we've been injecting purposeful failure into our test environment.
*)
(*** more ***)
(**
For those who aren't familiar with the general topic, Chaos Monkey, coined by Netflix, helps developers test the resiliency of their applications by terminating their hosted Amazon Web Services instances at random (this helps ensure that individual components can work independently in a cloud-based system, but allows folks to see what happens when one component is taken out of the picture). Typically, as companies grow, so do the instances contained in their system, making it inevitably less resilient and more prone to failure. This failure comes from the combination of both human error and the likelihood that components of the cloud data center could go down at any given time.
Chaos in any cloud-based infrastructure involves an aspect of design creativity while keeping in mind both concurrency of components and independent failure of components. At the end of the day, if your services aren't reliable enough, you will inevitably lose customers, and that is what we are trying to avoid. As stated by many before, "The best way to avoid failure is to fail constantly."
If you use cloud-based technology, services are bound to go down in Production sometimes. We want to help teams be ready and prepared for the disruptions that may occur in Production environments. In other words, please have multiple worker roles in your cloud services. Our Chaos solution uses a [Knuth/Fisher-Yates Shuffle](http://www.dotnetperls.com/fisher-yates-shuffle), which creates a uniformly random permutation of the array of nodes fed into it; see the link for more information. We then use this permutation to select and restart instances in our test environment, especially for pre-Purple (Black) Friday as we geared up for holiday shopping. 
*)


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




module service = 
    open System
    open EventStore.ClientAPI
    open Marvel.EventStore
    open Marvel
    open Types


(**
###Azure+F#+Chaos=<3
The Jet engine is powered by F#, and it's always nice to keep technology uniform where possible. Our technology pioneers at Jet made it easier for Chaos testing to use [microservices](https://opensource.com/resources/what-are-microservices). The Jet tech team builds a ton of F# microservices that abstract away a lot of complexity - and using this, we are also able to log restarts to [Splunk](http://www.splunk.com/) (more on this later). 
*)
compute
|> knuthShuffle
|> Seq.distinctBy(fun a -> a.ServiceName) 
|> Seq.map (fun hostedService -> async { 
    try
        return! restartRandomInstance compute hostedService
    with e -> 
        log.warn "failed: service=%s . %A" hostedService.ServiceName e
        return ()
})
|> Async.ParallelIgnore 1
|> Async.RunSynchronously
(**
As mentioned by previous bloggers, our whole system is hosted on Microsoft Azure. With a quick Google search, you will notice that Azure has its own form of Chaos Monkey, called WazMonkey. While WazMonkey is very successful, we wanted to be able to stop instances rather than entire services (i.e. not bring down the entire cluster but instead bring down nodes). We also wanted to incorporate some of our in-house architecture while also using F# to eventually allow Chaos to be part of the continuous integration cycle (i.e. run it every day).
We have an AzureHelper module that allows us to access Azure's REST API through different libraries and perform tasks such as: getting deployment details (how many instances are contained in a particular cloud service), select a random instance, restart and instance, stop an instance, etc.
*)

let selectRandomInstance (compute:ComputeManagementClient) (hostedService:HostedServiceListResponse.HostedService) = async {
    try
        log.info "Quering cloud_service=%s for instances" hostedService.ServiceName
        let! details = getHostedServiceDetails compute hostedService.ServiceName
        let deployment = getProductionDeployment details
            
        match deployment.RoleInstances.Count with 
        | 1 -> 
            log.warn "cloud_service=%s has only 1 instance" hostedService.ServiceName
        | _ -> log.info "cloud_service=%s has total_instances=%i" hostedService.ServiceName deployment.RoleInstances.Count


        let instance = deployment.RoleInstances |> Seq.toArray |> randomPick
        log.info "Selected cloud_service=%s instanceName=%s hostname=%s" hostedService.ServiceName instance.InstanceName instance.HostName
        return details.ServiceName, deployment.Name, instance
    with e -> 
        log.error "Failed selecting random instance\n%A" e
        return raise e
}

let restartRandomInstance (compute:ComputeManagementClient) (hostedService:HostedServiceListResponse.HostedService) = async {
    try 
        let! serviceName, deploymentId, roleInstance = selectRandomInstance compute hostedService
        match roleInstance.PowerState with
        | RoleInstancePowerState.Stopped -> 
            log.info "Service=%s Instance=%s is stopped...ignoring..." serviceName roleInstance.InstanceName
                
        | _ ->
            do! restartInstance compute serviceName deploymentId roleInstance.InstanceName
    with e -> 
        log.error "%s" e.Message
}



(**
###Socialization
Teams at Jet operate independently and have complete end-to-end responsibility for what they build. These teams own their product lifecycle from inception to deployment. To say that engineers were completely on board with Chaos Engineering would be a fallacy. But, with any new tool, idea or application comes the aspect of socialization and there has been work getting folks to realize the positive impact Chaos Engineering could have on the strength of the environment. Our Chaos solution is run on an automated schedule between 9 AM - 5 PM during the week. We chose business hours to give engineers the opportunity to react quickly when something unexpected happens.
###How do we know the effect the restarts have on the site?
Other than following the scream of a developer ;) we have automated critical-path tests running throughout the day, these tests encompass both general performance tests (GETs and POSTs) as well as front-end JavaScript tests. The critical path tests enable us to abstract information from obscure tests, and pinpoint it to a particular time an instance from a cloud service was started, allowing us to more easily root-cause an error before it reaches Production. Good logging is key for any chaos system to have a positive impact; if chaos causes degradation to services, we need proper notifications in order to investigate and Splunk allows us to do this. We certainly have plenty more we want to do with Chaos, and more that we think it is capable of - this is simply the beginning.
*)