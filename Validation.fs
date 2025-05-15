namespace TodoApi

open System

module Validation = 
    let validateTitle (title: string) : string option =
            if System.String.IsNullOrWhiteSpace(title) then
                Some "Title must not be empty."
            elif title.Length > 256 then
                Some "Title must be 256 characters or fewer."
            else
                None