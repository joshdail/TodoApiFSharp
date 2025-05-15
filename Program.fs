open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.HttpResults
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open TodoApi
open TodoApi.TodoStore
open TodoApi.Validation
open TodoApi.HttpHelpers

let builder = WebApplication.CreateBuilder()
let app = builder.Build()

app.MapGet("/todos", Func<IResult>(fun () ->
    Results.Ok(getAll())
)) |> ignore

app.MapGet("/todos/{id:int}", Func<int, IResult>(fun id ->
    match getById id with
    | Some todo -> Results.Ok(todo)
    | None -> Results.NotFound()
)) |> ignore

app.MapPost("/todos", Func<{| Title: string |}, IResult>(fun input ->
    match validateTitle input.Title with
    | Some error -> jsonError error
    | None ->
        let newTodo = add input.Title
        Results.Created($"/todos/{newTodo.Id}", newTodo)
)) |> ignore

app.MapPut("/todos/{id:int}", Func<int, Todo, IResult>(fun id updated ->
    match validateTitle updated.Title with
    | Some error -> jsonError error
    | None ->
        match update id updated with
        | Some todo -> Results.Ok(todo)
        | None -> Results.NotFound()
)) |> ignore

app.MapDelete("/todos/{id:int}", Func<int, IResult>(fun id ->
    if delete id then
        Results.Ok()
    else
        Results.NotFound()
)) |> ignore

app.Run()