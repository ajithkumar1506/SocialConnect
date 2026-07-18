using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.ChangePassword;
using SocialConnect.Application.Features.Auth.Commands.ForgotPassword;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Application.Features.Auth.Commands.Logout;
using SocialConnect.Application.Features.Auth.Commands.RefreshToken;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Application.Features.Auth.Commands.ResetPassword;
using SocialConnect.Application.Features.Auth.Commands.RevokeToken;
using SocialConnect.Application.Features.Auth.Commands.VerifyEmail;

namespace SocialConnect.API.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Register(
        [FromBody] RegisterCommand command
    )
    {
        var result = await Mediator.Send(command);
        return Ok(ApiResponse<AuthResult>.Ok(result, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(ApiResponse<AuthResult>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> RefreshToken(
        [FromBody] RefreshTokenCommand command
    )
    {
        var result = await Mediator.Send(command);
        return Ok(ApiResponse<AuthResult>.Ok(result, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutCommand command)
    {
        await Mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(new { }, "Logged out successfully."));
    }

    [HttpPost("revoke-token")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeToken(
        [FromBody] RevokeTokenCommand command
    )
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    "Failed to revoke token.",
                    new List<string> { result.Error ?? "Unknown error." }
                )
            );
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Token revoked successfully."));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(
        [FromBody] VerifyEmailCommand command
    )
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    "Verification failed.",
                    new List<string> { result.Error ?? "Unknown error." }
                )
            );
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Email verified successfully."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordCommand command
    )
    {
        await Mediator.Send(command);
        return Ok(
            ApiResponse<object>.Ok(
                new { },
                "If the email matches an active account, a password reset link has been sent."
            )
        );
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordCommand command
    )
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    "Password reset failed.",
                    new List<string> { result.Error ?? "Unknown error." }
                )
            );
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Password has been reset successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordCommand command
    )
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    "Password change failed.",
                    new List<string> { result.Error ?? "Unknown error." }
                )
            );
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Password has been changed successfully."));
    }
}
