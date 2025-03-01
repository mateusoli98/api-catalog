using APICatalog.DTOs;
using APICatalog.Models;
using APICatalog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdminOnly")]
        [Route("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response()
                    {
                        Status = "Error",
                        Message = "Role already exists"
                    });
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                _logger.LogInformation(2, "Role creation failed");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response()
                    {
                        Status = "Error",
                        Message = "Role creation failed"
                    });
            }

            _logger.LogInformation(1, "Role Added");
            return Ok(new Response()
            {
                Status = "Success",
                Message = "Role created successfully"
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdminOnly")]
        [Route("add-user-to-role")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                _logger.LogInformation(2, $"User {email} not found");
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response()
                    {
                        Status = "Error",
                        Message = "User not found"
                    });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                _logger.LogInformation(2, $"User {user.Email} creation failed");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response()
                    {
                        Status = "Error",
                        Message = "User creation failed"
                    });
            }

            _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role successfully");
            return Ok(new Response()
            {
                Status = "Success",
                Message = "User added to role successfully"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.UserName!);

            if (user is null)
            {
                return BadRequest("Usuário não encontrado");
            }

            var checkPassword = await _userManager.CheckPasswordAsync(user, loginModel.Password!);

            if (!checkPassword)
            {
                return Unauthorized("Senha inválida");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var userClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new("id", user.UserName!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = _tokenService.CreateToken(userClaims, _configuration);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out var refreshTokenValidityInMinutes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken,
                Expiration = token.ValidTo
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO registerModel)
        {
            var userExists = await _userManager.FindByNameAsync(registerModel.Email!);

            if (userExists is not null)
            {
                return BadRequest("Usuário já existe");
            }

            ApplicationUser user = new()
            {
                UserName = registerModel.Username,
                Email = registerModel.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, registerModel.Password!);

            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response()
                    {
                        Status = "Error",
                        Message = "User creation failed"
                    });
            }

            return Ok(new Response()
            {
                Status = "Success",
                Message = "User created successfully"
            });
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModelDTO tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));
            string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModel));

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if (principal is null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var username = principal.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username!);

            if (user is null
                || user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = _tokenService.CreateToken(principal.Claims.ToList(), _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost("revoke/{username}")]
        [Authorize(Policy = "ExclusivePolicyOnly")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
            {
                return BadRequest("Invalid user name");
            }

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }

    }
}
