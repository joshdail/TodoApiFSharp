open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Linq

// Define the model
type Todo = {
    Id: int
    Title: string
    IsDone: bool
}

let storageFile = "todos.json"

let loadTodos () =
    if File.Exists(storageFile) then
        let json = File.ReadAllText(storageFile)
        JsonSerializer.Deserialize<List<Todo>>(json)
    else
        List<Todo>()

let saveTodos (todos: List<Todo>) =
    let json = JsonSerializer.Serialize(todos, JsonSerializerOptions(WriteIndented = true))
    File.WriteAllText(storageFile, json)

let builder = WebApplication.CreateBuilder()
let app = builder.Build()

// GET /todos
app.MapGet("/todos", Func<Task<IResult>>(fun () ->
    task {
        let todos = loadTodos ()
        return Results.Ok(todos)
    }
)) |> ignore

// GET /todos/{id}
app.MapGet("/todos/{id:int}", Func<int, Task<IResult>>(fun id ->
    task {
        let todos = loadTodos ()
        let todo = todos |> Seq.tryFind (fun t -> t.Id = id)
        match todo with
        | Some t -> return Results.Ok(t)
        | None -> return Results.NotFound()
    }
)) |> ignore

// POST /todos
app.MapPost("/todos", Func<HttpContext, Task<IResult>>(fun ctx ->
    task {
        let! newTodo = ctx.Request.ReadFromJsonAsync<Todo>()
        let todos = loadTodos ()
        let nextId =
            if todos.Count = 0 then 1
            else (todos |> Seq.maxBy (fun t -> t.Id)).Id + 1
        let todo = { newTodo with Id = nextId }
        todos.Add(todo)
        saveTodos todos
        return Results.Created($"/todos/{todo.Id}", todo)
    }
)) |> ignore

// PUT /todos/{id}
app.MapPut("/todos/{id:int}", Func<int, HttpContext, Task<IResult>>(fun id ctx ->
    task {
        let! updated = ctx.Request.ReadFromJsonAsync<Todo>()
        let todos = loadTodos ()
        let index = todos.FindIndex(fun t -> t.Id = id)
        if index = -1 then
            return Results.NotFound()
        else
            todos.[index] <- { updated with Id = id }
            saveTodos todos
            return Results.Ok(todos.[index])
    }
)) |> ignore

// DELETE /todos/{id}
app.MapDelete("/todos/{id:int}", Func<int, Task<IResult>>(fun id ->
    task {
        let todos = loadTodos ()
        let removed = todos.RemoveAll(fun t -> t.Id = id)
        if removed = 0 then
            return Results.NotFound()
        else
            saveTodos todos
            return Results.NoContent()
    }
)) |> ignore

app.Run()