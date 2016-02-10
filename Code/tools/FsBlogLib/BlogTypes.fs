namespace FsBlogLib

module BlogTypes = 

  /// Type that stores information about blog posts
  type BlogHeader = 
    { Title : string
      Abstract : string
      Description : string
      AddedDate : System.DateTime
      Url : string
      Tags : seq<string> 
      Image : string
      PostAuthor : string
      JobPostingName : string
      JobPostingUrl : string      
      }
      
  /// Type that stores information about video posts
  type VideoHeader = 
    { Title : string
      Description : string
      AddedDate : System.DateTime
      Url : string
      ContentUrl : string
      Image : string
      Tags : seq<string> 
      PostAuthor : string 
    }

  type Header = 
        | BlogHeader of BlogHeader
        | VideoHeader of VideoHeader
