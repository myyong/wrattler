﻿module Wrattler.Common

open Fable.Core
open Fable.Import.JS
open Fable.Import.Browser
open System.Collections.Generic

module FsOption = Microsoft.FSharp.Core.Option

[<Emit("""(function(s){
  var hash = 0;
  if (s.length == 0) return hash;
  for (var i = 0; i < s.length; i++) {
    var char = s.charCodeAt(i);
    hash = ((hash<<5)-hash)+char;
    hash = hash & hash; 
  }
  var res = Math.abs(hash).toString(16);
  while(res.length < 8) res = '0' + res;
  return res; })($0)""")>]
let getHashCode (code:string) : string = failwith "JS"

[<Emit("eval($0)")>]
let eval<'T> (code:string) : 'T = failwith "JS"

[<Emit("$0[$1]")>]
let getProperty<'T> (obj:obj) (name:string) : 'T = failwith "JS"

[<Emit("parseInt($0, $1)")>]
let parseInt (s:string) (b:int) : int = failwith "JS"

[<Emit("$0.toString($1)")>]
let formatInt (i:int) (b:int) : string = failwith "JS"

[<Emit("(typeof($0)=='number')")>]
let isNumber(n:obj) : bool = failwith "!"

[<Emit("($0 instanceof Date)")>]
let isDate(n:obj) : bool = failwith "!"

[<Emit("$0.toISOString()")>]
let toISOString(o:obj) : string = failwith "!"

[<Emit("new Date($0)")>]
let asDate(n:float) : System.DateTime = failwith "!"

[<Emit("($0 instanceof Date) ? $0.getTime() : $0")>]
let dateOrNumberAsNumber(n:obj) : float = failwith "!"

[<Emit("""($0.toLocaleString("en-US",{day:"numeric",year:"numeric",month:"short"}))""")>]
let formatDate(d:obj) : string = failwith "!"

[<Emit("""$0.toLocaleString("en-GB",{day:"numeric",year:"numeric",month:"long"})""")>]
let formatLongDate(d:obj) : string = failwith "!"

[<Emit("""($0.toLocaleString("en-US",{hour:"numeric",minute:"numeric",second:"numeric"}))""")>]
let formatTime(d:obj) : string = failwith "!"

[<Emit("""($0.toLocaleString("en-US",{hour:"numeric",minute:"numeric",second:"numeric"}) + ", " +
    $0.toLocaleString("en-US",{day:"numeric",year:"numeric",month:"short"}))""")>]
let formatDateTime(d:obj) : string = failwith "!"

[<Emit("(typeof($0)=='object')")>]
let isObject(n:obj) : bool = failwith "!"

[<Emit("(typeof($0)=='string')")>] 
let isString (o:obj) : bool = failwith "JS"

[<Emit("Array.isArray($0)")>]
let isArray(n:obj) : bool = failwith "!"

[<Emit("isNaN($0)")>]
let isNaN(n:float) : bool = failwith "!"

let niceNumber num decs =
  let str = string num
  let dot = str.IndexOf('.')
  let before, after = 
    if dot = -1 then str, ""
    else str.Substring(0, dot), str.Substring(dot + 1, min decs (str.Length - dot - 1))
  let after = 
    if after.Length < decs then after + System.String [| for i in 1 .. (decs - after.Length) -> '0' |]
    else after 
  let mutable res = before
  if before.Length > 5 then
    for i in before.Length-1 .. -1 .. 0 do
      let j = before.Length - i
      if i <> 0 && j % 3 = 0 then res <- res.Insert(i, ",")
  if Seq.forall ((=) '0') after then res
  else res + "." + after

[<Emit("JSON.stringify($0)")>]
let jsonStringify json : string = failwith "JS Only"

[<Emit("JSON.parse($0)")>]
let jsonParse<'R> (str:string) : 'R = failwith "JS Only"

[<Emit("console.log.apply(console, $0)")>]
let consoleLog (args:obj[]) : unit = 
  let format = args.[0] :?> string
  let mutable argIndex = 1
  let mutable charIndex = 0
  let mutable res = ""
  while charIndex < format.Length do
    if format.[charIndex] = '%' then
      res <- res +
        match format.[charIndex+1] with
        | 'c' -> ""
        | 's' -> args.[argIndex].ToString()
        | 'O' -> sprintf "%A" (args.[argIndex])
        | _ -> failwith "consoleLog: Unsupported formatter"
      argIndex <- argIndex + 1
      charIndex <- charIndex + 2
    else 
      res <- res + format.[charIndex].ToString()
      charIndex <- charIndex + 1
  printfn "%s" res

[<Emit("typeof window == 'undefined'")>]
let windowUndefined () : bool = true

let isLocalHost() = 
  windowUndefined () ||
  window.location.hostname = "localhost" || 
  window.location.hostname = "127.0.0.1"

let mutable enabledCategories = 
  if not (isLocalHost ()) then set []
  else set ["*"] 

let getColor =   
  let colorMap = System.Collections.Generic.Dictionary<_, _>()
  let mutable index = -1
  let colors = 
    [| "#393b79"; "#637939"; "#8c6d31"; "#843c39"; "#7b4173" 
       "#3182bd"; "#e6550d"; "#31a354"; "#756bb1"; "#636363" |]
  fun cat -> 
    if not (colorMap.ContainsKey(cat)) then
      index <- (index + 1) % colors.Length
      colorMap.Add(cat, colors.[index])
    colorMap.[cat]
  
type Log =
  static member setEnabled(cats) = enabledCategories <- cats

  static member message(level:string, category:string, msg:string, [<System.ParamArray>] args) = 
    let args = if args = null then [| |] else args
    let category = category.ToUpper()
    if level = "EXCEPTION" || level = "ERROR" || enabledCategories.Contains "*" || enabledCategories.Contains category then
      let dt = System.DateTime.Now
      let p2 (s:int) = (string s).PadLeft(2, '0')
      let p4 (s:int) = (string s).PadLeft(4, '0')
      let prefix = sprintf "[%s:%s:%s:%s] %s: " (p2 dt.Hour) (p2 dt.Minute) (p2 dt.Second) (p4 dt.Millisecond) category
      let color = 
        match level with
        | "TRACE" -> "color:" + getColor category
        | "EXCEPTION" -> "color:#c00000"
        | "ERROR" -> "color:#900000"
        | _ -> ""
      consoleLog(FSharp.Collections.Array.append [|box ("%c" + prefix + msg); box color|] args)

  static member trace(category:string, msg:string, [<System.ParamArray>] args) = 
    Log.message("TRACE", category, msg, args)

  static member exn(category:string, msg:string, [<System.ParamArray>] args) = 
    Log.message("EXCEPTION", category, msg, args)

  static member error(category:string, msg:string, [<System.ParamArray>] args) = 
    Log.message("ERROR", category, msg, args)

type Http =
  /// Send HTTP request asynchronously
  static member Request(meth, url, ?data, ?cookies) =
    Async.FromContinuations(fun (cont, econt, _) ->
      let xhr = XMLHttpRequest.Create()
      xhr.``open``(meth, url, true)
      match cookies with 
      | Some cookies when cookies <> "" -> xhr.setRequestHeader("X-Cookie", cookies)
      | _ -> ()
      xhr.onreadystatechange <- fun _ ->
        if xhr.readyState > 3. && xhr.status = 200. then
          cont(xhr.responseText)
        if xhr.readyState > 3. && xhr.status = 0. then
          econt(System.Exception(meth + " " + url + " failed: " + xhr.statusText))
        obj()
      xhr.send(defaultArg data "") )

type Future<'T> = 
  abstract Then : ('T -> unit) -> unit

type Microsoft.FSharp.Control.Async with
  static member AwaitFuture (f:Future<'T>) = Async.FromContinuations(fun (cont, _, _) ->
    f.Then(cont))

  static member Future (n:string option) op start = 
    let mutable res = Choice1Of3()
    let mutable handlers = []
    let mutable running = false

    let trigger h = 
      match res with
      | Choice1Of3 () -> handlers <- h::handlers 
      | Choice2Of3 v -> h v
      | Choice3Of3 e -> raise e

    let ensureStarted() = 
      if not running then 
        n |> FsOption.iter (fun n -> Log.trace("system", "Starting future '%s'....", n))
        running <- true
        async { try 
                  let! r = op
                  n |> FsOption.iter (fun n -> Log.trace("system", "Future '%s' evaluated to: %O", n, r))
                  res <- Choice2Of3 r                  
                with e ->
                  Log.exn("system", "Evaluating future failed: %O", e)
                  res <- Choice3Of3 e
                for h in handlers do trigger h } |> Async.StartImmediate
    if start = true then ensureStarted()

    { new Future<_> with
        member x.Then(f) = 
          ensureStarted()
          trigger f }

  static member CreateFuture(op) = Async.Future None op false
  static member StartAsFuture(op) = Async.Future None op true
  static member CreateNamedFuture name op = Async.Future (Some name) op false
  static member StartAsNamedFuture name op = Async.Future (Some name) op true

module Async = 
  module Array =
    module Parallel =
      let rec map f (ar:_[]) = async {
        let res = FSharp.Collections.Array.zeroCreate ar.Length
        let work = 
          [ for i in 0 .. ar.Length-1 -> async {
              let! v = f ar.[i]
              res.[i] <- v } ] |> Async.Parallel
        let! _ = work
        return res }

    let rec map f (ar:_[]) = async {
      let res = FSharp.Collections.Array.zeroCreate ar.Length
      for i in 0 .. ar.Length-1 do
        let! v = f ar.[i]
        res.[i] <- v
      return res }

  let rec collect f l = async {
    match l with 
    | x::xs -> 
        let! y = f x
        let! ys = collect f xs
        return List.append y ys
    | [] -> return [] }

  let rec choose f l = async {
    match l with 
    | x::xs -> 
        let! y = f x
        let! ys = choose f xs
        return match y with None -> ys | Some y -> y::ys 
    | [] -> return [] }

  let rec map f l = async {
    match l with 
    | x::xs -> 
        let! y = f x
        let! ys = map f xs
        return y::ys
    | [] -> return [] }

  let rec foldMap f st l = async {
    match l with
    | x::xs ->
        let! y, st = f st x
        let! st, ys = foldMap f st xs
        return st, y::ys
    | [] -> return st, [] }

  let rec fold f st l = async {
    match l with
    | x::xs ->
        let! st = f st x
        return! fold f st xs 
    | [] -> return st }

/// Symbol is a unique immutable identiifer (we use JavaScript symbols)
type Symbol = 
  { ID : string; Unique : obj }
  override x.ToString() = x.ID

module SymbolHelpers =   
  [<Emit("Symbol()")>]
  let nativeSymbol() : obj = obj()
  
let createSymbol (s:string) = { ID = s; Unique = SymbolHelpers.nativeSymbol() }
let createAutoSymbol () = { ID = ""; Unique = SymbolHelpers.nativeSymbol() }

type ListDictionaryNode<'K, 'T> = 
  { mutable Result : 'T option
    Nested : Dictionary<'K, ListDictionaryNode<'K, 'T>> }

type ListDictionary<'K, 'V> = Dictionary<'K, ListDictionaryNode<'K, 'V>>

module JsHelpers = 
  type KeyValue = 
    abstract key : string
    abstract value : obj

  [<Emit("(function(o) { return Object.keys(o).map(function(k) { return {\"key\":k, \"value\":o[k] }; }); })($0)")>]
  let properties(o:obj) : KeyValue[] = failwith "!"

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ListDictionary = 
  let tryFind ks dict = 
    let rec loop ks node =
      match ks, node with
      | [], { Result = Some r } -> Some r
      | k::ks, { Nested = d } when d.ContainsKey k -> loop ks (d.[k])
      | _ -> None
    loop ks { Nested = dict; Result = None }

  let set ks v dict =
    let rec loop ks (dict:ListDictionary<_, _>) = 
      match ks with
      | [] -> failwith "Empty key not supported"
      | k::ks ->
          if not (dict.ContainsKey k) then dict.[k] <- { Nested = Dictionary<_, _>(); Result = None }
          if List.isEmpty ks then dict.[k].Result <- Some v
          else loop ks (dict.[k].Nested)
    loop ks dict

  let count (dict:ListDictionary<_, _>) = 
    let rec loop node = 
      let nest = node.Nested |> Seq.sumBy (fun kv -> loop kv.Value)
      if node.Result.IsSome then 1 + nest else nest
    dict |> Seq.sumBy (fun kv -> loop kv.Value)

module List = 
  let groupWith f list = 
    let groups = ResizeArray<_ * ResizeArray<_>>()
    for e in list do
      let mutable added = false 
      let mutable i = 0
      while not added && i < groups.Count do
        if f e (fst groups.[i]) then 
          (snd groups.[i]).Add(e)
          added <- true
        i <- i + 1
      if not added then 
        groups.Add(e, ResizeArray<_>([e]))
    groups |> Seq.map (snd >> List.ofSeq) |> List.ofSeq

  let unreduce f s = s |> Seq.unfold (fun s -> 
    f s |> Microsoft.FSharp.Core.Option.map (fun v -> v, v)) |> List.ofSeq
