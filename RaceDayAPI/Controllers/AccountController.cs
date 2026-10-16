using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

using RaceDayAPI.DTOs.Authentication;
using RaceDayAPI.Model;
using RaceDayAPI.Services;

using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        // POST: api/Account/register/participant
        [HttpPost("register/participant")]
        public async Task<IActionResult> RegisterParticipant(
            [FromBody] ParticipantRegistrationDTO dto)
        {
            try
            {
                var account =
                    await _accountService.RegisterParticipantAsync(dto);

                await SignInAccountAsync(account);

                return StatusCode(201, new
                {
                    message = "Participant registered successfully.",
                    accountID = account.AccountID,
                    userName = account.UserName,
                    accountType = account.AccountType
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Account/register/organizer
        [HttpPost("register/organizer")]
        public async Task<IActionResult> RegisterOrganizer(
            [FromBody] OrganizerRegistrationDTO dto)
        {
            try
            {
                var account =
                    await _accountService.RegisterOrganizerAsync(dto);

                await SignInAccountAsync(account);

                return StatusCode(201, new
                {
                    message = "Organizer registered successfully.",
                    accountID = account.AccountID,
                    userName = account.UserName,
                    accountType = account.AccountType
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDTO dto)
        {
            try
            {
                var account = await _accountService.LoginAsync(dto);

                await SignInAccountAsync(account);

                return Ok(new
                {
                    message = "Login successful.",
                    accountID = account.AccountID,
                    userName = account.UserName,
                    accountType = account.AccountType
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/Account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        private async Task SignInAccountAsync(Account account)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    account.AccountID.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    account.UserName),

                new Claim(
                    ClaimTypes.Role,
                    account.AccountType)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });
        }
    }
}

