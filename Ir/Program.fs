open FSharpPlus
open System
open System.Net.Http
open System.Json
open Fleece
open IrKit

let uncurry f = (<||) f
let runIO = Async.RunSynchronously
let getLine = async { return System.Console.ReadLine() }
let print x = async { printf "%s" x}

[<EntryPoint>]
let main argv = 
  let env = monad {
    let http = new HttpClient()
    let! devices = lookup zeroconfResolver
    let device = devices |> List.head
    return (http, device)
  }

  match argv with
  | [| "recv" |] ->
    (uncurry receive) =<< env
    |> runIO
    |> toJSON
    |> printf "%O"

  | [| "send" |] ->
    monad {
      let! env = env
      let! line = getLine
      let input = line |> JsonValue.Parse |> fromJSON
      return! input |> choice print (env |> uncurry send)
    }
    |> runIO

  | [| "--version" |] ->
    printf "1.0.0.0"
  | _ ->
    printf "help"
  
  0