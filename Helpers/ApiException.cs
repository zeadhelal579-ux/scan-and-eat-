// FILE: Helpers/ApiException.cs
// PURPOSE: A small, deliberate exception type Services throw for expected
//          business-rule failures (not found, invalid input, a conflicting
//          state) so Controllers never need try/catch blocks of their own -
//          Helpers/ExceptionHandlingMiddleware.cs turns one of these into a
//          clean HTTP response in exactly one place.
// LOGIC:   The JSON body it produces - { "message": "..." } - is not
//          arbitrary: it is exactly the shape every already-built frontend
//          error handler already reads from a failed response
//          (see shared/js/api-client.js and customer/js/api.js:
//          `(parsedBody && parsedBody.message) || res.statusText`), so a
//          rejected order, a bad login, or a blocked delete all surface as
//          a readable message in the existing UI with no frontend changes.

namespace ScanEat.Api.Helpers;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public static ApiException BadRequest(string message) => new(StatusCodes.Status400BadRequest, message);
    public static ApiException Unauthorized(string message) => new(StatusCodes.Status401Unauthorized, message);
    public static ApiException NotFound(string message) => new(StatusCodes.Status404NotFound, message);
    public static ApiException Conflict(string message) => new(StatusCodes.Status409Conflict, message);
}
