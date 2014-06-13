[<EntryPoint>]
let main argv = 
  match argv with
  | [| "--version" |] ->
    printf "1.0.0.0"
    0
  | _ ->
    printf "help"
    0