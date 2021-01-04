// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Net
open System.Text
open HtmlAgilityPack
open ScrapySharp.Network
open ScrapySharp.Extensions
open System.Linq
open Newtonsoft.Json
open FSharp.Collections.ParallelSeq
open Sharprompt


type urlContainer =
    { total: int
      current: int
      url: string }

let wallHavenUrl = Uri("https://wallhaven.cc/random")

let isPng (node: HtmlNode) = node.CssSelect("span.png").Any()

let getDownloadUrl (node: HtmlNode) =
    printfn "Getting download link for node"
    let wallpaper_base_url = "https://w.wallhaven.cc/full/"
    let fileExtension = if isPng (node) then "png" else "jpg"

    let wallpaperId =
        node
            .Attributes
            .First(fun x -> x.Name = "data-wallpaper-id")
            .Value

    let fileName =
        $"wallhaven-{wallpaperId}.{fileExtension}"

    let downloadUrl =
        $"{wallpaper_base_url}{wallpaperId.[0..1]}/{fileName}"

    printfn "Got download link for %s" fileName
    (fileName, downloadUrl)



let downloadImage (fileName: string) (url: string) =
    printfn "Downloading %s" fileName
    use wc = new WebClient()
    wc.DownloadFile(url, $"./{fileName}")
    printfn "Downloaded %s" fileName

let handleIter node = getDownloadUrl node ||> downloadImage
let getPaginationFromWebPage (webPage: WebPage) = 
        webPage.Html
         |> fun i -> i.CssSelect("ul.pagination")
         |> Seq.head
         |> fun i -> i.Attributes
         |> Seq.tryFind (fun i -> i.Name = "data-pagination")
         |> fun v ->
             match v with
             | Some x -> x.Value
             | _ -> ""



[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- Encoding.UTF8

    let selectedScrape =
        Prompt.Select("What would you like to scrape?", [ "Wallhaven" ])

    printfn "Creating Browser"
    let scraper = ScrapingBrowser()
    printfn "Created Browser"
    printfn "Navigating To Page"
    let webPage = scraper.NavigateToPage(wallHavenUrl)
    printfn "Navigated To Page"

    printfn "Getting page information"

    let convertedPaginationAttr = getPaginationFromWebPage webPage

    printfn "Got page information"

    printfn "Parsing page seed"

    let seedUrlObject =
        JsonConvert.DeserializeObject<urlContainer>(convertedPaginationAttr)

    let seedUrl = seedUrlObject.url
    printfn "Parsed page seed"

    printfn "Getting figure nodes"
    let htmlNodes = webPage.Html.CssSelect("figure")
    printfn "Got figure nodes"

    printfn "Iterating over figures"

    htmlNodes
    |> PSeq.withDegreeOfParallelism (4)
    |> PSeq.iter handleIter

    printfn "Iterated over figures"



    0 // return an integer exit code
