open System.Runtime.InteropServices
open Microsoft.FSharp.Core.Operators.Checked // prevent overflowing bytes
open Gutenberg.Helpers
open FSharp.Core.Operators.Checked

let dic = @"D:\TMP\2023-09-08--gutenberg\gutenberg.txt"

let words =
    dic
    |> System.IO.File.ReadAllLines
    |> Array.map (fun s -> s.Trim().ToLower())
    |> Array.distinct

let chars = words |> Seq.collect id |> Seq.distinct |> Seq.sort |> Seq.toArray
printfn $"%A{chars.Length} chars"

let maxCharValue = chars |> Array.map int |> Array.max
let charIndexMap = Array.init (maxCharValue + 1) (fun _ -> -1)

for i, c in chars |> Array.indexed do
    charIndexMap.[int c] <- i
    printfn $"%i{i} %A{c} %A{int c}"


let wordMap word =
    word
    |> Seq.fold
        (fun (result: byte array) (c: char) ->
            let index = charIndexMap.[int c]
            result[index] <- (result[index] + 1uy)
            result
        )
        (Array.init chars.Length (fun _ -> 0uy))


let mappedWords = words |> Array.map (fun word -> (word, wordMap word))

// memcmp
[<DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)>]
extern int memcmp(byte[] b1, byte[] b2, uint32 count)

let input =
    [ "Anne"; "Vincent"; "Antoine" ]
    |> List.map (fun s -> s.ToLower())
    |> String.implode
    |> wordMap
