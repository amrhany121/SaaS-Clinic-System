using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaaS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null) return BadRequest("المستخدم موجود بالفعل!");

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                TenantId = Guid.NewGuid(),
                IsActive = false,
                CreatedAt = DateTime.UtcNow,

                // --- التعديل السحري هنا ---
                // نخليه مأكد الإيميل أوتوماتيك عشان الـ Identity ميرفضش دخوله بعد التفعيل
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
                // --------------------------
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // إعطاؤه دور Admin فوراً ليكون ظاهر للسوبر أدمن
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok("تم تسجيل طلب العيادة بنجاح. بانتظار تفعيل الإدارة.");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            // 1. فحص الباسورد
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // 2. القفلة الأهم: التأكد من التفعيل
                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "الحساب قيد المراجعة، يرجى انتظار تفعيل السوبر أدمن." });
                }

                // 3. جلب الـ Roles
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("TenantId", user.TenantId.ToString()),
                    new Claim("FullName", user.FullName ?? "")
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                // 4. توليد التوكن
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    role = userRoles.FirstOrDefault(),
                    fullName = user.FullName
                });
            }

            return Unauthorized(new { message = "خطأ في الإيميل أو كلمة المرور" });
        }

        public record RegisterModel(string Email, string Password, string FullName);
        public record LoginModel(string Email, string Password);
    }
}