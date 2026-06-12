using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 身分驗證控制器，提供基於 Cookie 驗證的使用者登入、登出以及 BPM/UOF 傳入之 JWT Token 單點登入
    /// </summary>
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// 初始化身分驗證控制器
        /// </summary>
        /// <param name="config">系統組態配置</param>
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // ==========================================
        // 1. 單點登入：由 BPM 傳入 Token 並發放 Cookie
        // 網址：/Auth/SSOLogin?token=xxxxxx
        // ==========================================
        /// <summary>
        /// 接收外部系統傳入的 JWT Token，驗證成功後於本系統發行 Cookie 登入身分
        /// </summary>
        /// <param name="token">由 BPM/UOF 單點登入重導向傳入的 JWT 字串</param>
        /// <returns>重新導向至首頁或返回錯誤訊息</returns>
        [HttpGet]
        public async Task<IActionResult> SSOLogin(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("登入失敗：未傳入 Token。");

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // 從組態配置檔中讀取 JWT 密鑰，避免硬編碼金鑰
                string? loginTokenKey = _config["JwtSettings:loginTokenKey"];
                if (string.IsNullOrEmpty(loginTokenKey))
                {
                    throw new Exception("組態設定中缺少 JwtSettings:loginTokenKey 密鑰配置。");
                }
                var key = Encoding.UTF8.GetBytes(loginTokenKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, 
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,  
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // 從驗證通過的 JWT Token 中取出使用者帳號與姓名
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            catch (SecurityTokenExpiredException)
            {
                return Content("登入失敗：登入憑證已過期，請重新從 BPM 系統連線。");
            }
            catch (Exception ex)
            {
                return Content($"登入失敗：無效的登入憑證 ({ex.Message})");
            }
        }

        // ==========================================
        // 2. 開發階段測試：產生測試 Token 頁面 (請勿於生產環境公開)
        // 網址：/Auth/GenerateTestToken
        // ==========================================
        /// <summary>
        /// 開發測試頁面，用以產生一組有效期限為 5 分鐘的 JWT Token 以進行一鍵 SSO 登入測試
        /// </summary>
        /// <returns>HTML 測試頁面檢視</returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GenerateTestToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "B50005"), 
                new Claim(ClaimTypes.Name, "測試人員")         
            };

            string? loginTokenKey = _config["JwtSettings:loginTokenKey"];
            if (string.IsNullOrEmpty(loginTokenKey))
            {
                return Content("組態設定中缺少 JwtSettings:loginTokenKey 密鑰配置，無法產生測試 Token。");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(loginTokenKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            string loginUrl = $"/Auth/SSOLogin?token={tokenString}";

            string htmlContent = $@"
                <div style='padding: 20px; font-family: sans-serif;'>
                    <h2>EAM 系統開發專用：JWT Token 產生器</h2>
                    <p>以下為有效期限 5 分鐘的測試用 JWT Token（使用 appsettings 密鑰加密）：</p>
                    <div style='background: #eee; padding: 10px; word-break: break-all; font-family: monospace;'>
                        {tokenString}
                    </div>
                    <hr style='margin: 20px 0;' />
                    <a href='{loginUrl}' style='font-size: 20px; padding: 10px 20px; background: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>
                        一鍵模擬 BPM 單點登入！
                    </a>
                </div>";

            return Content(htmlContent, "text/html", Encoding.UTF8);
        }

        // ==========================================
        // 3. 登出功能
        // 網址：/Auth/Logout
        // ==========================================
        /// <summary>
        /// 清除登入狀態，移除系統核發的 Cookie
        /// </summary>
        /// <returns>登出完成畫面提示</returns>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Content("您已成功登出系統。可關閉此頁面或重新由 BPM 連線登入。");
        }

        // ==========================================
        // 4. 存取拒絕提示頁
        // 網址：/Auth/AccessDenied
        // ==========================================
        /// <summary>
        /// 當使用者未登入或存取受限時重導向的畫面
        /// </summary>
        /// <returns>權限不足提示訊息</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("存取拒絕：您尚未登入或登入已逾時，請重新從 BPM 系統連線登入。");
        }
    }
}