namespace Sunset

module Class1=

    open FSharp.Data
    open Deedle
    open System
    open XPlot.GoogleCharts
    open XPlot.GoogleCharts.Deedle
    open Suave
    open Suave.Web
    open Suave.Http.Successful

    let apiUrl (date : DateTime) =
        let root = "http://api.sunrise-sunset.org"
        let lat = "48.897174"
        let lng = "2.156543"
        let format = "yyyy-MM-dd"
        sprintf "%s/json?lat=%s&lng=%s&date=%s" root lat lng (date.ToString(format))


    type Simple = JsonProvider<"http://api.sunrise-sunset.org/json?lat=36.7201600&lng=-4.4203400&date=2015-11-28">

    let dates =
        let day1 = DateTime(2016,1,1)
        [for d in [0 .. 366] do yield day1.AddDays((float)d)]

    let queries = 
        dates
        |> List.map apiUrl


    let results () = 
        queries
        |> List.map Simple.AsyncLoad
        |> Async.Parallel
        |> Async.RunSynchronously

    let sunset () = 
        [for s in results() do yield s.Results.Sunset]
        |> List.zip dates 
        |> series

    let sunrise ()= 
        [ for s in results () do yield s.Results.Sunrise]
        |> List.zip dates 
        |> series

    let hmltPage () =
        let chart  = 
            [sunset(); sunrise()]
            |> Chart.Line
        "<!DOCTYPE html><html><head><title>Chart</title></head><body>" + chart.Html + "</body></html>" 


    let display () =
        let result =
            hmltPage ()
            |> OK
        //Start your web browser
        System.Diagnostics.Process.Start("http://localhost:8083/") |> ignore
        // start Web Server
        result
        |> startWebServer defaultConfig
        

    [<EntryPoint>]
    let main argv =
        display ()
        0