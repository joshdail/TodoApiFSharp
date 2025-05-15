namespace TodoApi

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.HttpResults

module HttpHelpers = 

    let jsonError (message: string) =
        Results.BadRequest({| error = message |})