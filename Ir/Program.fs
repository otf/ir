open FSharpPlus
open System
open System.Net.Http
open System.Json
open Fleece
open IrKit

let uncurry f = (<||) f

[<EntryPoint>]
let main argv = 
  let env = async {
    use http = new HttpClient()
    let! devices = lookup zeroconfResolver
    let device = devices |> List.head
    return (http, device)
  }

  match argv with
  | [| "recv" |] ->
    (uncurry receive) =<< env
    |> Async.RunSynchronously
    |> toJSON
    |> printfn "%O"

  | [| "send" |] ->
    async {
      let! env = env
      let input = Console.ReadLine() |> JsonValue.Parse |> fromJSON
      return! input |> choice (printf "%s" >> result) (env |> uncurry send)
    }
    |> Async.RunSynchronously

  | [| "--version" |] ->
    printf "1.0.0.0"
  | _ ->
    printf "help"
  
  0