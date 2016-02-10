namespace FsBlogLib

open FSharp.Data

module UrlUtilities =
    open System

    let get_titles (url:string) =
        let results = HtmlDocument.Load(url)

        results.Descendants ["title"]
        |> Seq.map (fun x -> x.InnerText())
        |> Seq.toArray

    let get_descriptions (url2:string) =
        let uri = Uri(url2)
        let url = 
            if uri.Host.Contains("youtube") then
                let youtubeId = url2.Split('/')
                "https://" + uri.Host + "/watch?v=" + youtubeId.[youtubeId.Length-1]
            else 
                url2
        let results = HtmlDocument.Load(url)

        // Site specific strategies are how Facebook 
        // and such started... I'm sure over time with careful
        // analysis you can come up with a generic set of heuristics
        // for this kind of thing that is *pretty* good but not 100%
        // For us, there is a pretty limited set of sites
        // that host the kinds of talks we're interested in
        match uri.Host with
        | "www.infoq.com"
        | "infoq.com" ->
            results.Descendants ["p"]
            |> Seq.choose (fun x ->
                x.TryGetAttribute("id")
                |> Option.map(fun p -> x.InnerText(), p.Value())
                )
            |> Seq.filter(fun (text, id) -> id = "summary")
            |> Seq.map(fun (text, id) -> text)
            |> Seq.toArray
        | "www.vimeo.com"
        | "vimeo.com" ->
            results.Descendants ["div"]
            |> Seq.choose (fun x ->
                x.TryGetAttribute("class")
                |> Option.map(fun c -> c.Value(), x)
            )
            |> Seq.filter(fun (tag, element) -> tag = "description_wrapper")
            |> Seq.map ( fun (tag, element) -> 
                element.Descendants ["p"] 
                |> Seq.map(fun x -> x.InnerText())
                |> String.Concat
            )
            |> Seq.toArray
        | "www.youtube.com"
        | "youtube.com" ->
            results.Descendants ["p"]
            |> Seq.choose (fun x ->
                x.TryGetAttribute("id")
                |> Option.map(fun p -> p.Value(), x.InnerText())
            )
            |> Seq.filter(fun (id, text) -> id = "eow-description")
            |> Seq.map(fun (id, text) -> text)
            |> Seq.toArray
        | _ -> 
            // Generically, descriptions are probably pretty long,
            // so just return an array of all the strings that are longer than 50 chars
            // in tags which are normally text fields
            // I have no idea if this is a good strategy or not
//            results.Descendants ((fun x-> x.Name), true)
//            |> Seq.choose( fun x -> 
//                if String.length (x.InnerText()) > 100 then Some (x.InnerText()) else None)
//            |> Seq.toArray
            [|""|]

module JobPostings = 
  open System 

  type Postings = HtmlProvider<"https://boards.greenhouse.io/embed/job_board?for=jet&amp;b=https://jet.com/about-us/working-at-jet/jobs">

  let private getPosts = 
      Postings.Load("https://boards.greenhouse.io/embed/job_board?for=jet&amp;b=https://jet.com/about-us/working-at-jet/jobs").Html.Body().Descendants ["div"]
                      |> Seq.filter (fun p -> p.HasAttribute("class", "opening"))
                      |> Seq.filter (fun p -> p.HasAttribute("department_id", "6296"))
                      |> Seq.map (fun a -> 
                                    let link = a.Descendants ["a"] |> Seq.head  
                                    (link.InnerText(), link.Attribute("href").Value())
                                  )

  let getRandomPosting now = 
    let rnd = new Random(now)

    let knuthShuffle (seq : 'b seq) = 
        let arr = seq |> Seq.toArray
        let swap i j =
            let item = arr.[i]
            arr.[i] <- arr.[j]
            arr.[j] <- item
        let ln = arr.Length
        [0..(ln - 2)]
        |> Seq.iter (fun i -> swap i (rnd.Next(i, ln)))
        arr

    let randomPick (arr: 'b array) = 
        let index = rnd.Next arr.Length
        arr.[index]

    getPosts
    |> knuthShuffle
    |> randomPick


