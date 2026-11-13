// FILE: Models/DTOs/AuthDtos.cs
// PURPOSE: The wire shapes for POST /api/auth/login (08-API-CONTRACT.md
//          section 2), verified against shared/js/login-form.js: the login
//          response only needs { token, role } - the frontend takes the
//          username straight from its own form input, not from this
//          response, so AuthResponseDto deliberately stays minimal.

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

public class LoginRequestDto
{
    // security-test-report.md F8: bounds on both fields so a request can't
    // send an unreasonably large body just to make the server hash-compare
    // (or, before that, model-bind/validate) a huge string.
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
