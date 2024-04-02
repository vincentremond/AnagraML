open System.Diagnostics
open System.Runtime.InteropServices
open Microsoft.FSharp.Core.Operators.Checked // prevent overflowing bytes
open Gutenberg.Helpers
open FSharp.Core.Operators.Checked
open FsToolkit.ErrorHandling

let timer = Stopwatch.StartNew()

let dic =
  @"C:\Users\remond\TMP\2024-02\2024-02-05--fix-ISO-8859-15\FixEncoding\FixEncoding\gutenberg"

let allWords =
  dic
  |> System.IO.File.ReadAllLines
  |> Array.map (_.Trim().ToLower())
  |> Array.sortByDescending (_.Length)
  |> Array.distinct

let chars = allWords |> Seq.collect id |> Seq.distinct |> Seq.sort |> Seq.toArray
printfn $"%A{chars.Length} chars"

let maxCharValue = chars |> Array.map int |> Array.max
let charIndexMap = Array.init (maxCharValue + 1) (fun _ -> -1)

for i, c in chars |> Array.indexed do
  charIndexMap.[int c] <- i
  printfn $"%i{i} %A{c} %A{int c}"


let wordMap word =
  word
  |> Seq.fold
    (fun (result: sbyte array) (c: char) ->
      let index = charIndexMap.[int c]
      result[index] <- (result[index] + 1y)
      result
    )
    (Array.init chars.Length (fun _ -> 0y))


let mappedWords = allWords |> Array.map (fun word -> (word, wordMap word))

// memcmp
[<DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)>]
extern int memcmp(byte[] b1, byte[] b2, uint32 count)

let input =
  [ "Anne"; "Vincent"; "Antoine" ]
  |> List.map (_.ToLower())
  |> String.implode
  |> wordMap

let allMappedWords = allWords |> Array.mapWith wordMap

type Word = string

type ItemTestResult =
  | TooHighFound
  | Positive
  | Zero

type TestResult =
  | TooHigh
  | Final of Word
  | More of Word * sbyte array

let isOk (counts: sbyte array) (word: string, mappedWord: sbyte array) =
  let allPairs = Array.zip mappedWord counts

  let newCount, testState =
    allPairs
    |> Array.mapFold
      (fun currentState (mapped, count) ->
        let diff = count - mapped

        let currentItemState =
          if diff = 0y then Zero
          elif diff > 0y then Positive
          elif diff < 0y then TooHighFound
          else failwith "Impossible case !"

        let newState =
          match currentState, currentItemState with
          | TooHighFound, _ -> TooHighFound
          | _, TooHighFound -> TooHighFound
          | Positive, _ -> Positive
          | _, Positive -> Positive
          | Zero, Zero -> Zero

        diff, newState
      )
      Zero

  match testState with
  | TooHighFound -> TooHigh
  | Zero -> Final word
  | Positive -> More(word, newCount)

let rec getSomeWords state acc =
  // allMappedWords
  // |> Array.choose (fun mappedWord ->
  //   let x = isOk state mappedWord
  //
  //   match x with
  //   | Final word -> Some(word :: acc)
  //   | More(word, newCount) ->
  //     let more = getSomeWords newCount acc
  //     match more with
  //     | [||] -> None
  //     | _ -> Some(
  //   | TooHigh -> None
  // )

  seq {
    for mappedWord in allMappedWords do
      let x = isOk state mappedWord

      match x with
      | Final word ->
        printfn $"! %A{word :: acc}"
        yield word :: acc
      | More(word, newCount) ->
        printfn $"? %A{word :: acc}"
        yield! getSomeWords newCount (word :: acc)
      | TooHigh -> ()
  }


let result =
  getSomeWords input []
  |> Seq.toList
  |> List.map (List.sort)
  |> List.distinct

printfn $"Done in {timer.Elapsed}"
