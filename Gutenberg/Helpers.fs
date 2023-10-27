namespace Gutenberg.Helpers

[<RequireQualifiedAccess>]
module Array =
    let indexed arr = arr |> Array.mapi (fun i x -> i, x)

[<RequireQualifiedAccess>]
module String =
    open System
    let implode (s: string seq) = String.Join("", s)
