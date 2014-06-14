open FsControl.Core.Types
open FSharpPlus
open System
open System.Net.Http
open System.Json
open Fleece
open IrKit

let uncurry f = (<||) f
let runIO = Async.RunSynchronously
let run (Kleisli f) = f () |> runIO
let getLine = async { return System.Console.ReadLine() }
let print x = async { printf "%O" x}

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
    Kleisli <| konst (env >>= (uncurry receive))
    >>>> Kleisli (toJSON >> print)
    |> run

  | [| "send" |] ->
    let send = fun msg -> ((uncurry send) <!> env) >>= ((|>) msg)
    Kleisli (konst getLine)
    >>>> Kleisli (JsonValue.Parse >> fromJSON >> result) 
    >>>> (Kleisli print |||| Kleisli send) 
    |> run

  | [| "--version" |] ->
    print "1.0.0.0" 
    |> runIO

  | _ ->
    print "help"
    |> runIO
  
  0