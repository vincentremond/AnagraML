namespace Gutenberg.Helpers

[<RequireQualifiedAccess>]
module Array =
  let indexed arr = arr |> Array.mapi (fun i x -> i, x)
  let mapWith f = Array.map (fun i -> (i, f i))

[<RequireQualifiedAccess>]
module String =
  open System
  let implode (s: string seq) = String.Join("", s)
