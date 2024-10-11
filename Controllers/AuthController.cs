using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinhaAPI.DTOs;
using MinhaAPI.Models;
using MinhaAPI.Services;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MinhaAPI.Controllers
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

        public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager,RoleManager<IdentityRole>roleManager, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
                
        }

        [Authorize(AuthenticationSchemes = "Bearer", Policy = "SuperAdminOnly")]
        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation(1, "Roles Added");
                    return StatusCode(StatusCodes.Status200OK,
                            new ResponseDTO
                            {
                                Status = "Success",
                                Message =
                            $"Role {roleName} added successfully"
                            });
                }
                else
                {
                    _logger.LogInformation(2, "Error");
                    return StatusCode(StatusCodes.Status400BadRequest,
                       new ResponseDTO
                       {
                           Status = "Error",
                           Message =
                           $"Issue adding the new {roleName} role"
                       });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest,
              new ResponseDTO { Status = "Error", Message = "Role already exist." });
        }

        [Authorize(AuthenticationSchemes = "Bearer", Policy = "SuperAdminOnly")]
        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                    return StatusCode(StatusCodes.Status200OK,
                           new ResponseDTO
                           {
                               Status = "Success",
                               Message =
                           $"User {user.Email} added to the {roleName} role"
                           });
                }
                else
                {
                    _logger.LogInformation(1, $"Error: Unable to add user {user.Email} to the {roleName} role");
                    return StatusCode(StatusCodes.Status400BadRequest, new ResponseDTO
                    {
                        Status = "Error",
                        Message = $"Error: Unable to add user {user.Email} to the {roleName} role"
                    });
                }
            }
            return BadRequest(new { error = "Unable to find user" });
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO model) //recebe o corpo do request e o objeto tipo LoginModelDTO, que contém as propriedades userName e Password
        {
            
            var user = await _userManager.FindByNameAsync(model.UserName!);//procura o username

            if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))//vê se o user n é null e se a senha fornecida é valida
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                if (userRoles == null || !userRoles.Any())
                {
                    await _userManager.AddToRoleAsync(user, "Admin");

                    // Obtém novamente os papéis do usuário
                    userRoles = await _userManager.GetRolesAsync(user);

                    if (!userRoles.Any())
                    {
                        return BadRequest("Erro ao atribuir papel ao usuário.");
                    }
                }
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),//coloca um identificador no token

                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole)); //adiciona os perfis do usuario nos tokens 
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);//cria o token e passa as claims e a instancia de configuration

                var refreshToken = _tokenService.GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes); //tempo do refresh token, armazenando no refreshTokenValidity...

                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);//convertendo as coisas pra datetime

                user.RefreshToken = refreshToken;
               
                await _userManager.UpdateAsync(user);//armazenamos o refreshToken e a Data de expiração, na tabela do user no Identity
                
                

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    //FALA QUE:n é é possível converter de "object" para "Microsoft.IdentityModel.Tokens.SecurityToken"
                    refreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

            //    if (token is JwtSecurityToken jwtToken)//certifica-se de que token é um JwtSecurityToken antes de chamar WriteToken
            //        {
            //            var refreshToken = _tokenService.GenerateRefreshToken();

            //            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

            //            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
            //            user.RefreshToken = refreshToken;

            //            await _userManager.UpdateAsync(user);

            //            return Ok(new
            //            {
            //                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            //                RefreshToken = refreshToken,
            //                Expiration = jwtToken.ValidTo
            //            });
            //        }

            //        return BadRequest("Erro ao gerar o token.");
            //    }

            //    return Unauthorized();

            //}


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {

            var userExist = await _userManager.FindByNameAsync(model.UserName!);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User já existe" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded) 
            {
                //var errors = result.Errors.Select(e => e.Description).ToList();
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User já existe" });

            }
            return Ok(new { Status = "Success", Message = "User criado com sucesso" });
        }

        [Authorize(AuthenticationSchemes = "Bearer", Policy = "ExclusiveOnly")]
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModelDTO tokenmodel)
        {
            if (tokenmodel is null)
            {
                return BadRequest("Invalido");
            }
            string? accessToken = tokenmodel.AccessToken ?? throw new ArgumentNullException(nameof(tokenmodel));
            string? refreshToken = tokenmodel.RefreshToken ?? throw new ArgumentException(nameof(tokenmodel));
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if (principal == null)
            {
                return BadRequest("O token está inválido");
            }
            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Token invalido");
            }
            var newAccesToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
            if (newAccesToken is JwtSecurityToken JWTnewAccessToken)
            {
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshToken = newRefreshToken;

                await _userManager.UpdateAsync(user);
                return new ObjectResult(new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(JWTnewAccessToken),
                    refreshToken = newRefreshToken,
                });
            }
            return BadRequest("Erro ao gerar o token");
            
        }


        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Nome Inválido");
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return NoContent();

            
        }










       
    }
}
