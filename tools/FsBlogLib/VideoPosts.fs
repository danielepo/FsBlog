﻿namespace FsBlogLib
open System.IO
open FSharp.Literate

// --------------------------------------------------------------------------------------
// Parsing blog posts etc.
// --------------------------------------------------------------------------------------
module VideoPosts = 

  open FileHelpers
  open System.Text.RegularExpressions

  /// Type that stores information about blog posts
  type VideoHeader = 
    { Title : string
      Description : string
      AddedDate : System.DateTime
      Url : string
      Tags : seq<string> 
      PostAuthor : string
      ContentAuthor : string }

  /// Simple function that parses the header of the blog post. Everybody knows
  /// that doing this with regexes is silly, but the blog post headers are simple enough.
  let ParseVideoHeader renameTag (blog:string) =
    let concatRegex = Regex("\"[\s]*\+[\s]*\"", RegexOptions.Compiled)
    fun (file:string, header:string, abstr) ->
      let lookup =
        header.Split(';')
        |> Array.filter (System.String.IsNullOrWhiteSpace >> not)
        |> Array.map (fun (s:string) -> 
            match s.Trim().Split('=') |> List.ofSeq with
            | key::values -> 
                let value = String.concat "=" values
                key.Trim(), concatRegex.Replace(value.Trim(' ', '\t', '\n', '\r', '"'), "")
            | _ -> failwithf "Invalid header in the following blog file: %s" file ) |> dict
      let relativeFile = file.Substring(blog.Length)
      let relativeFile = let idx = relativeFile.LastIndexOf('.') in relativeFile.Substring(0, idx)
      try
        { Title = lookup.["Title"]
          Url = relativeFile.Replace("\\", "/")
          Description = lookup.["Description"]
          Tags = lookup.["Tags"].Split([|','|], System.StringSplitOptions.RemoveEmptyEntries) |> Array.map (fun s -> s.Trim() |> renameTag)
          AddedDate = lookup.["AddedDate"] |> System.DateTime.Parse 
          PostAuthor = lookup.["PostAuthor"]
          ContentAuthor = lookup.["ContentAuthor"]}
      with _ -> failwithf "Invalid header in the following blog file: %s" file
