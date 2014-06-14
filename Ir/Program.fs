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

let application env = function
  | [| "recv" |] ->
    Kleisli <| konst (env >>= (uncurry receive))
    >>>> Kleisli (toJSON >> print)

  | [| "send" |] ->
    let send = fun msg -> ((uncurry send) <!> env) >>= ((|>) msg)
    Kleisli (konst getLine)
    >>>> Kleisli (JsonValue.Parse >> fromJSON >> result) 
    >>>> (Kleisli print |||| Kleisli send) 

  | [| "--version" |] ->
    Kleisli <| konst (print "1.0.0.0")

  | _ ->
    Kleisli <| konst (print "help")

[<EntryPoint>]
let main argv = 
  let env = monad {
    let http = new HttpClient()
    let! devices = lookup zeroconfResolver
    let device = devices |> List.head
    return (http, device)
  }

  run (application env argv)
  
  0