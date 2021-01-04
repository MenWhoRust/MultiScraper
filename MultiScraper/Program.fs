namespace MultiScraper

open System.Threading

module Main =
    open System
    open System.Net
    open System.Text
    open Sharprompt


    let mutable numberOfRows = Console.CursorTop

    let downloadImage (fileName: string, url: string) =
        async {
            let currentRow = Interlocked.Increment(&numberOfRows)
            use wc = new WebClient()
            wc.DownloadProgressChanged.Add(fun i ->
                    Console.SetCursorPosition(0, currentRow)
                    Console.WriteLine($"{i.ProgressPercentage} {fileName}")
                )
            

            wc.DownloadFileCompleted.Add(fun _ ->
                Console.SetCursorPosition(0, currentRow)
                Console.WriteLine()
                Interlocked.Decrement(&numberOfRows)|> ignore
                )


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
        |> Seq.map downloadImage
        |> fun i -> (i, 5)
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously




        0 // return an integer exit code
