namespace Wrattler.Ast
open Wrattler.Common

// ------------------------------------------------------------------------------------------------

type Value = 
  | Nothing
  | Outputs of (string -> unit)[]
  | Frame of obj[]
  | Frames of Map<string, obj[]>

type Name = string

type EntityKind = 
  | Root 
  | Code of lang:string * code:string * frames:Entity list
  | DataFrame of var:string * rblock:Entity
  | CodeBlock of lang:string * code:Entity * vars:Entity list
  | Notebook of blocks:Entity list

and Entity = 
  { Kind : EntityKind
    Symbol : Symbol
    mutable Value : Value option }

type ParsedCode = 
  | RSource of string 
  | JsSource of string

type Block = 
  | CodeBlock of code:ParsedCode
  | MarkdownBlock of obj list

type Node<'T> = 
  { Node : 'T
    mutable Entity : Entity option }