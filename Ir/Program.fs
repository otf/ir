open FsControl.Core.Types
open FSharpPlus
open System
open System.Reflection
open System.Net.Http
open System.Json
open Fleece
open IrKit

let uncurry f = (<||) f
let run (Kleisli f) = f () |> Async.RunSynchronously
let getLine = async { return System.Console.ReadLine() }
let print x = async { printf "%O" x}

let printHelp = print """
usage: ir [--version] [--help]
          <command> [<args>]

commands are:
  recv  receive a message, and print the message to standard output.
  send  read a message from standard input, and send the message.
"""

let printVersion = 
  Assembly.GetExecutingAssembly().GetName().Version
  |> print

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
    Kleisli <| konst (printVersion)

  | _ ->
    Kleisli <| konst (printHelp)

[<EntryPoint>]
let main argv = 
  let env = monad {
    let! devices = lookup zeroconfResolver
    return (new HttpClient(), devices |> List.head)
  }

  run (application env argv)
  
  0