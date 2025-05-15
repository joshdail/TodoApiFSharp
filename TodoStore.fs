namespace TodoApi

open System.Collections.Generic

module TodoStore =
    let private todos = Dictionary<int, Todo>()
    let mutable private nextId = 1

    let getAll () = todos.Values |> Seq.toList

    let getById id =
        match todos.TryGetValue id with
        | true, todo -> Some todo
        | _ -> None

    let add title =
        let todo = {
            Id = nextId;
            Title = title;
            IsDone = false
        }
        todos.Add(nextId, todo)
        nextId <- nextId + 1
        todo

    let update id updatedTodo =
        if todos.ContainsKey(id) then
            todos.[id] <- updatedTodo
            Some updatedTodo
        else
            None

    let delete id =
        todos.Remove(id)
    