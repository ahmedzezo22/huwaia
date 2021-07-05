using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZawagProject.Data;
using ZawagProject.DTO;
using ZawagProject.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ZawagProject.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        // private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager,IConfiguration config, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _config = config;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {
            // Api Controller used instead if (!ModelState.IsValid) return BadRequest(ModelState);
            if (ModelState.IsValid)
            {
                // userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();
                // if (await _repo.UserExists(userForRegisterDto.UserName))
                // {
                //     return BadRequest("هذا المستخدم موجود بالفعل");
                // }
                var userToCreate = _mapper.Map<User>(userForRegisterDto);

                // var user = await _repo.Register(userToCreate, userForRegisterDto.Password);
                var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
               
                var userReturn = _mapper.Map<UsersForDetailsDto>(userToCreate);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userToCreate,"Member");
                     // return StatusCode(201);
                    return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userReturn);
                }
                return BadRequest(result.Errors);
               
            }
            else
            {
                var modelErrors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        modelErrors.Add(modelError.ErrorMessage);
                    }
                }
                return BadRequest(modelErrors);
            }
        }
        //Login 
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginForDTO userLogin)
        {

            // var user = await _repo.Login(userLogin.username.ToLower(), userLogin.password);
            // if (user == null) return Unauthorized();
            var user = await _userManager.FindByNameAsync(userLogin.username);
            var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.password, false);
            if (result.Succeeded)
            {
                var appUser = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.NormalizedUserName == userLogin.username.ToUpper());
                var userReturn = _mapper.Map<UsersListDto>(appUser);
                return Ok(new
                {
                    token = GenerateJwtTokenForUser(user).Result,
                    userReturn,
                    message = "Login successfully"
                });
            }
            return Unauthorized();
        }
        private async Task<string> GenerateJwtTokenForUser(User user)
        {
            var claims = new List<Claim>
           {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),

                new Claim(ClaimTypes.Name,user.UserName)
            };
            var roles=await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var TokenHandler = new JwtSecurityTokenHandler();
            var Token = TokenHandler.CreateToken(TokenDescriptor);
            return TokenHandler.WriteToken(Token);
        }
    }
}
