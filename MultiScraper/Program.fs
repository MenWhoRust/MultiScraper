namespace MultiScraper
module Main =
    open System
    open System.Net
    open System.Text
    open FSharp.Collections.ParallelSeq
    open Sharprompt

    
    let downloadImage (fileName: string, url: string) =
        printfn "Downloading %s" fileName
        use wc = new WebClient()
        wc.DownloadFile(url, $"./{fileName}")
        printfn "Downloaded %s" fileName
    

    [<EntryPoint>]
    let main argv =
        Console.OutputEncoding <- Encoding.UTF8

        let figures = match Prompt.Select("What would you like to scrape?", [ "Wallhaven" ]) with
                      | "Wallhaven" -> downloadWallhaven
                      | _ -> null

        figures
        |> PSeq.withDegreeOfParallelism (4)
        |> PSeq.iter downloadImage




        0 // return an integer exit code
