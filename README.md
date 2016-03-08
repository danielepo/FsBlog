# Jet Technology Blog 

The Jet Technology blog is powered by [FsBlog](https://github.comfsprojects/FsBlog), a blog-aware static site generator, mostly built in `F#`, and hosted with [GitHub Pages](http://pages.github.com/). 

FsBlog uses some of the following community projects:

* [FAKE](http://fsharp.github.io/FAKE/) for the automation and scripting of the different tasks.
* [F# Formatting](http://tpetricek.github.io/FSharp.Formatting/) which deals with the Markdown and F# processing/colorization.
* [RazorEngine](https://github.com/Antaris/RazorEngine) which is used for the templating and embedded C# code.
* Some of the code that calls *RazorEngine* from F# is based on [Tilde](https://github.com/aktowns/tilde).
* [Bootstrap 3](http://getbootstrap.com/).

## Getting started
Fork this repo! Then, from a command line, run 
```
build
```
This will generate the tools to create the blog. 

## Creating a new post for the blog 
To create a new blog post, run 
```
	fake new post="My markdown post"
```
This generates a new markdown file in BlogContent/blog/current-year/. If your post contains images (it should!) create a new folder under BlogContent/images/ and place them in there. Copying to Code/content/images is often needed as well.

You may also want to write a code-heavy post. To do so, run
```
	fake new fsx="My script post" 
```
This will create a new blank .fsx file in BlogContent/blog/current-year/. To find out more about using .fsx files as the source of your posts, check out [F# Formatting: Literate programming](http://tpetricek.github.io/FSharp.Formatting/demo.html). 

## Adding a new video to the sidebar
To add a Jet-recommended video to our list, run 
```
	fake new video="video url" name="your name" tags="comma-delimited list of tags"
```
This will automatically generate the title and description for youtube, vimeo, and infoQ videos (thanks Jim!), and then create an md file in BlogContent/videos/current-year/. **NB: For a youtube video, you must use the embed link! (otherwise we can't find the description.)**

## Previewing your changes
To double-check everything once you're done, run
```
	fake preview
```
## Final steps
Once you're finished, send a PR. We'll merge and your post will be live.
