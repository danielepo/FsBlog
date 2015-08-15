namespace FsBlogLib

module UrlUtilities =
    open FSharp.Data
    open System

    let get_titles (url:string) =
        let results = HtmlDocument.Load(url)

        results.Descendants ["title"]
        |> Seq.map (fun x -> x.InnerText())
        |> Seq.toArray

    let get_descriptions (url:string) =
        let results = HtmlDocument.Load(url)

        let uri = Uri(url)

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
