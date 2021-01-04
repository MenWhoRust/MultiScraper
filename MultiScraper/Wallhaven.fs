namespace MultiScraper

[<AutoOpen>]
module Wallhaven =
    open System
    open HtmlAgilityPack
    open ScrapySharp.Extensions
    open ScrapySharp.Network
    type private urlContainer =
        { total: int
          current: int
          url: string }
    let private wallpaper_base_url = "https://w.wallhaven.cc/full/"
    let private wallHavenUrl = Uri("https://wallhaven.cc/random")

    let private isPng (node: HtmlNode) =
        node.CssSelect("span.png") |> Seq.length > 0

    let figureNodes (webPage: WebPage) = webPage.Html.CssSelect("figure")
    
    let private getDownloadUrl (node: HtmlNode) =
        printfn "Getting download link for node"
        let fileExtension = if isPng (node) then "png" else "jpg"

        let wallpaperId =
            node.Attributes
            |> Seq.find (fun x -> x.Name = "data-wallpaper-id")
            |> fun x -> x.Value

        let fileName =
            $"wallhaven-{wallpaperId}.{fileExtension}"

        let downloadUrl =
            $"{wallpaper_base_url}{wallpaperId.[0..1]}/{fileName}"

        printfn "Got download link for %s" fileName
        (fileName, downloadUrl)

    let private getPaginationFromWebPage (webPage: WebPage) =
        webPage.Html
        |> fun i -> i.CssSelect("ul.pagination")
        |> Seq.head
        |> fun i -> i.Attributes
        |> Seq.tryFind (fun i -> i.Name = "data-pagination")
        |> fun v ->
            match v with
            | Some x -> x.Value
            | _ -> ""

    let downloadWallhaven =
       wallHavenUrl
       |> ScrapingBrowser().NavigateToPage
       |> figureNodes
       |> Seq.map getDownloadUrl
       
       