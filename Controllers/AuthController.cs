// FILE: Controllers/AuthController.cs
// PURPOSE: Staff login -> issues JWT (04-BACKEND-ARCHITECTURE.md section 2).
//          Route/behavior verified against shared/js/login-form.js: the
//          response only ever needs { token, role }.
// LOGIC:   Controllers stay thin (04-BACKEND-ARCHITECTURE.md section 3) -
//          this one talks directly to ASP.NET Core Identity's SignInManager
//          / UserManager (the standard, security-reviewed way to check a
//          password hash) and to JwtTokenGenerator; there is no separate
//          "AuthService" because there is no other business logic here to
//          separate out.
// SECURITY (security-test-report.md F7 / F14): now uses
//          SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)
//          instead of UserManager.CheckPasswordAsync, so the account
//          lockout options configured on AddIdentity in Program.cs actually
//          take effect (5 failed attempts -> 5-minute lockout per account),
//          on top of the separate per-IP "login" rate-limit policy also
//          registered in Program.cs. Every attempt is now logged
//          (success/failure/lockout, with the caller's IP, never the
//          password) so repeated failures are at least visible after the
//          fact even where the two defenses above don't fully stop them.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScanEat.Api.Helpers;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        JwtTokenGenerator tokenGenerator,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    // POST /api/auth/login - the only Auth route in 08-API-CONTRACT.md section 2.
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var username = dto.Username.Trim();

        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            // Deliberately the same message whether the username or the
            // password was wrong - never reveal which one to a caller.
            _logger.LogWarning("Login failed (unknown username {Username}) from {Ip}", username, ip);
            throw ApiException.Unauthorized("Incorrect username or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login blocked: {Username} from {Ip} is locked out after too many failed attempts", username, ip);
            throw new ApiException(StatusCodes.Status429TooManyRequests,
                "Too many failed attempts for this account. Try again in a few minutes.");
        }
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed (wrong password) for {Username} from {Ip}", username, ip);
            throw ApiException.Unauthorized("Incorrect username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenGenerator.GenerateToken(user, roles);
        // Admin is a superset of Staff (08-API-CONTRACT.md's Auth column) -
        // if a user somehow holds both roles, report the higher one.
        var role = roles.Contains("Admin") ? "Admin" : (roles.FirstOrDefault() ?? "Staff");

        _logger.LogInformation("Login succeeded for {Username} ({Role}) from {Ip}", username, role, ip);
        return Ok(new AuthResponseDto { Token = token, Role = role });
    }
}
