using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Web_EAMSystem.Controllers
{


    // 告訴全域防護罩：「這個控制器是換證中心，不用檢查 Cookie，讓任何人都能進來！」
    [AllowAnonymous]
    public class AuthController : Controller
    {

        private readonly IConfiguration _config;

        // 建立建構子，讓 ASP.NET Core 自動把設定檔總管導入
        public AuthController(IConfiguration config)
        {
            _config = config;
        }
        // ==========================================
        // 1. 換證櫃台：接收 BPM 傳來的 Token 並發放 Cookie
        // 網址會是：/Auth/SSOLogin?token=xxxxxx
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SSOLogin(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("登入失敗：未提供 Token。");

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                // 拿出一模一樣的防偽印章
                string loginTokenKey = _config["JwtSettings:loginTokenKey"];
                var key = Encoding.UTF8.GetBytes("LoginToEAMSByUOFP@ssWordTempToken");

                // 🌟 開始嚴格驗票
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // 檢查印章對不對
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,  // 暫不檢查發行者名稱
                    ValidateAudience = false,// 暫不檢查接收者名稱
                    ClockSkew = TimeSpan.Zero // 嚴格比對過期時間，不給寬限期
                }, out SecurityToken validatedToken);

                // 如果程式能走到這裡，代表 Token 是「真的」且「沒過期」！
                var jwtToken = (JwtSecurityToken)validatedToken;

                // 從 Token 裡面把當初 BPM 塞進去的帳號跟姓名抽出來
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                // ==========================================
                // 以下跟原本一樣，核發我們自己的 Cookie
                // ==========================================
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
                return Content("拒絕存取：登入連結已逾時 (超過5分鐘)，請重新從 BPM 系統點擊連結。");
            }
            catch (Exception ex)
            {
                // 如果印章不對、或是有人亂改 Token 字串，就會走到這裡
                return Content($"拒絕存取：無效的登入憑證 ({ex.Message})");
            }
        }

        //(僅用於測試產生token登入，要記得刪除)
        //==========================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GenerateTestToken()
        {
            // 1. 設定要放在 Token 裡的假資料 (模擬 BPM 傳過來的使用者)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "B50005"), // 假帳號
                new Claim(ClaimTypes.Name, "測試人曾冠福")         // 假姓名
            };

            // 2. 拿出我們約定好的防偽印章 (必須跟 SSOLogin 解密用的一模一樣！)
            // 長度一定要夠長，不然系統會報錯
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:loginTokenKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. 建立 JWT (設定 5 分鐘後過期)
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            // 4. 產生出那一長串的 Token 字串
            string tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            // 5. 為了方便測試，我們直接在畫面上印出這串密碼，跟一個點擊跳轉的「超連結」
            string loginUrl = $"/Auth/SSOLogin?token={tokenString}";

            string htmlContent = $@"
                <div style='padding: 20px; font-family: sans-serif;'>
                    <h2>🛠️ 開發者專用：Token 印鈔機</h2>
                    <p>這是一串剛出爐、熱騰騰且 5 分鐘內有效的 JWT Token：</p>
                    <div style='background: #eee; padding: 10px; word-break: break-all;'>
                        {tokenString}
                    </div>
                    <hr style='margin: 20px 0;' />
                    <a href='{loginUrl}' style='font-size: 20px; padding: 10px 20px; background: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>
                        🚀 點我模擬從 BPM 跳轉到資產系統！
                    </a>
                </div>";

            return Content(htmlContent, "text/html", System.Text.Encoding.UTF8);
        }

        // ==========================================
        // 3. 登出櫃台：銷毀使用者的 Cookie
        // 網址會是：/Auth/Logout
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // 🌟 關鍵魔法：呼叫 SignOutAsync，系統會立刻把瀏覽器上的 Cookie 註銷銷毀！
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 登出後，顯示一段文字，或者你可以把它導向一個專屬的「已登出」畫面
            return Content("您已成功登出系統。請關閉此視窗，或重新從 BPM 系統點擊連結進入。");
        }

        // ==========================================
        //當沒有 Cookie 的人硬闖其他頁面時，會被踢到這裡
        // 網址會是：/Auth/AccessDenied
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // 現在我們先簡單回傳一段純文字
            return Content("拒絕存取：您尚未登入或登入已過期，請從 BPM 系統重新進入。");
        }
    }
}