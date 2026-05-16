using backend.domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace backend.api.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>Maps a Result to an appropriate HTTP response.</summary>
        public static IActionResult ToActionResult(this Result result)
            => result.IsSuccess
                ? new NoContentResult()
                : result.ToProblem();

        /// <summary>Maps a Result&lt;T&gt; to 200 OK or a problem response.</summary>
        public static IActionResult ToActionResult<T>(this Result<T> result)
            => result.IsSuccess
                ? new OkObjectResult(result.Value)
                : result.ToProblem();

        /// <summary>Maps a Result&lt;T&gt; to 201 Created or a problem response.</summary>
        public static IActionResult ToCreatedResult<T>(
            this Result<T> result, string routeName, object? routeValues = null)
            => result.IsSuccess
                ? new CreatedAtRouteResult(routeName, routeValues, result.Value)
                : result.ToProblem();

        private static IActionResult ToProblem(this Result result)
        {
            var error = result.Error;

            var statusCode = error.Code switch
            {
                var c when c.StartsWith("Auth.") => 401,
                var c when c.StartsWith("User.Unauth") => 403,
                var c when c.Contains("NotFound") => 404,
                var c when c.Contains("AlreadyExists") => 409,
                "Validation.Failed" => 422,
                _ => 400,
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = error.Code,
                Detail = error.Message,
            };

            return new ObjectResult(problem) { StatusCode = statusCode };
        }
    }

}
