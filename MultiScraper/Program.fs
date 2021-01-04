namespace MultiScraper

open System.Threading

module Main =
    open System
    open System.Net
    open System.Text
    open Sharprompt



    let downloadImage (fileName: string, url: string, number: int) =
        async {
            use wc = new WebClient()

            wc.DownloadProgressChanged.Add(fun i ->
                Console.SetCursorPosition(0, number)
                Console.WriteLine($"{i.ProgressPercentage} {i.UserState}"))

            wc.DownloadFileCompleted.Add(fun _ ->
                Console.SetCursorPosition(0, number)
                Console.WriteLine())


            do! wc.DownloadFileTaskAsync(Uri(url), $"./{fileName}")
                |> Async.AwaitTask

        }


    [<EntryPoint>]
    let main argv =
        Console.OutputEncoding <- Encoding.UTF8
        Console.CursorVisible <- false

        let figures =
            match Prompt.Select("What would you like to scrape?", [ "Wallhaven" ]) with
            | "Wallhaven" -> downloadWallhaven
            | _ -> null


        figures
        |> Seq.mapi (fun i x ->
            let a, b = x
            (a, b, i))
        |> Seq.map downloadImage
        |> fun i -> (i, 5)
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously




        0 // return an integer exit code
