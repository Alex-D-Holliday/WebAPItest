using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPItest.DbContext;
using WebAPItest.Models;

namespace WebAPItest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }
        
        [HttpGet]
        [Route("Gets all users")]
        public List<ApplicationUser> GetAll()
        {
            return _context.Set<ApplicationUser>().ToList();
        }

        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Login = model.Login
            };

            var result = await _userManager.CreateAsync(applicationUser, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = new JwtSecurityToken();
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }
    }
}
