@{
    Layout = "post";
    Title = "Azure Cloud Services at Jet.com";
    AddedDate = "2016-03-07T11:48:24";
    Tags = "F#, Jet.com, Jet Technology, Jet.com Technology, Azure, Cloud, F#, Cloud Services";
    Description = "";
    Image = "";
    PostAuthor = "Drew Schaeffer";
}
---

![Jet Cloud](/images/jetcom.png)

Jet makes heavy use of Azure Cloud Services for running our platform. We have hundreds of services that power Jet.com whether it's pricing, order processing, user management, product catalog, etc - it's running in a Cloud Service. These services have different requirements around scale, security, availability and deployment. I will cover how Azure helps us meet these requirements and where Azure PaaS falls a bit short.
<!--more--> 
> Azure Cloud Services provide an easy way to create, package and deploy highly scalable, highly available services in the cloud. More info can be found here - https://azure.microsoft.com/en-us/services/cloud-services/

---

###Scalability
To achieve scale you typically have two options - scale up or scale out. 

Scale up refers to running your application on a bigger machine. To scale up an Azure Cloud Service you would need to update the cloud service definition. There are several reasons this is not a recommended approach - cost is not linear as you scale up to bigger machines, performance is not linear as you scale up, there is a cap to the size of VM you can provision on Azure and the 'Upgrade' feature of a Cloud Service does not provide the elasticity you want from a scale perspective. 

Scale out refers to adding more commodity machines to run your service. This approach has several favorable features - cost is generally low for smaller machines, performance tends to scale closer to linear as you scale out, there is practically no cap to the number of instances you can add (up to 1000) and the Auto-Scaling features of Cloud Services allows you to scale based on cpu or work load. This elasticity is one of the big benefits of running in a cloud environment, specifically Azure PaaS. When you have multiple instances of a service you want to distribute the load between all of the services. Each Cloud Service is registered to the Azure Internal Load Balancer which distributes traffic to you service to each instance using your choice of a few different routing strategies.

> More about the routing strategies can be found here.
> https://azure.microsoft.com/en-us/blog/azure-load-balancer-new-distribution-mode/

An example of this at Jet are the pricing services that serve price, availability and savings to Jet.com for the search and product detail pages. Search load  changes based on the day/week, promotions, marketing campaigns, holidays, etc. The pricers need to handle the change in load but ideally we do no want to be scaled to handle the peaks at all times. We can elastically scale our prices by time of day, work load or cpu usage. This allows us to have more control over our costs while handling variablility of load.

###Security

When defining a Cloud Service you must specify the endpoints through which others may communicate with your service. By default there are no exposed endpoints and the service is not accessible by anyone. There are two types of endpoints you can define. Input endpoints and Internal endpoints. An Input endpoint is a public facing endpoint through which you define the protocol, public port and local port. There is a flavor of Input endpoint referred to as Instance Input endpoint which allows you to map ports to service instances through port forwarding. Internal endpoints are for opening endpoints for instance to instance communication and is not accessible by anyone except other instances of the Cloud Service.

At Jet, we expose Input endpoints that are the basis for any service that requires request/reply style messaging. It's important to note that the "public" facing endpoints are only accessible to services running on the same virtual network (or those connected through point-to-site vpn).

###Availability

Availability is achieved through a combination of having multiple instances running in different Update Domains and Fault Domains. The scope of a Fault Domain is the physical layout of the computer or rack that the computer is on. If the rack loses power all computers on the rack fail. By running services in two different Fault Domains you are ensuring a single power supply failure will not bring down all instances of your service. An Update Domain is a logical construct that determines how updates to your service instances are rolled. By running a service in multiple Update Domains you are ensuring that an upgrade to your Cloud Service or a scheduled update by Azure will not bring down all instances of your service at a given time. These updates will roll to each instance one Update Domain at a time.

###Deployment - staging -> prod

Cloud Services provide Staging and Production deployment slots which are two sets of Cloud Service instances behind an ILB. This allows you to deploy new code to the Staging slot while your existing production deployment is running. When you are comfortable with your change you can perform a swap operation that swaps the VIP of the ILB between staging and prod. This in theory is a powerful feature as traffic will be routed to your new code at a click of the button. No need to worry about registering/removing ip's from the load balancer.

There are a two limitations/issues that we have found using the Cloud Service swap function. Firstly, a worker role can only swap slots for a Cloud Service with a single instance. There are likely some very good reasons for this limitation but it severely diminishes some of the core benefits of using PaaS that we just reviewed. Secondly, the TTL after a swap in theory is instantaneous, however because of http keep-alive and dns caching the clients of these service often hang on to the old slot ip and continue to hit these now non-prod services for minutes after the swap operation. We see requests being routed to both old and new prod services for an unpredictable amount of time. We could ask the clients to change how they connect to our service but we do not want to rely on client connection configuration to ensure that our production deployments are taking effect in a timely manner.

##Jet Services
At Jet we generally have two types of services - reactive, event driven services and request / reply style services. 

The reactive services use a pub/sub model or queue based model for recieving input and producing output. The Request / Reply services use a transport such as http. Each service may have different requirements around scale, availability and security. As such, we can tailor our Cloud Service definitions based on these requirements. Our pricing services, for example, require elastic scalability, high availability and access restrictions to allow only parties within the virtual network. These services expose themselves via simple http endpoints that are consumed by Jet.com to serve prices for search, product detail pages, cart details and checkout. Because of the request / reply symantics we need the ability to balance the load between all available instances without the client having to route the traffic to specific instance. 

Initially, we made use of the Azure ILB mentioned earlier to do this load balancing. This ILB as a service was fast to develop and allowed us to concentrate on our service functionality rather than the infrastructure. As discussed earlier, one important feature for availability is to deploy the latest version of the service to the Staging slot and perform a swap so all instances in current prod are swapped out for the new version at pratically the same time. We cannot use the Cloud Service swap feature for the previously mentioned reasons but we still want the capability of deploy to "staging" and swap to production. We chose to create two Cloud Services, pricing-slot1 and pricing-slot2, and put a 3rd party load balancer in front of the services.
  
![nginx](/images/nginx.png)

By having two Cloud Services we effectively mimic the Prod and Staging slots provided by Cloud Services. We always deploy to the inactive slot then perform a "swap" operation that adds nginx upstream ips to reflect the new set of ips. Not only do we have swapping for multiple instance Cloud Services but we have the basis for our A/B testing infrastructure as nginx allows us to define routing rules based on various attributes of a request. 

Azure Cloud Services have been a great resource for Jet as the allowed us to build quickly while achieving the scale and availability that we need without worrying too much about the infrastructure. As we move forward we will have more challenges that will require to get more out of our cloud environment. We are evaluating solutions such as building our F# services for Mono and running them in Docker using Mesosphere. Another promising technology coming out of Microsoft is Azure Service Fabric which is still in its infancy but has some great potential. 