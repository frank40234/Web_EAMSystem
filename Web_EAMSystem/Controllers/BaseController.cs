using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web_EAMSystem.Data;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 所有控制器的抽象基底類別，封裝資料庫上下文與當前登入使用者的識別取得邏輯
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// 資料庫上下文，供繼承的控制器直接使用
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// 初始化基底控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        protected BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 從 Claims 中提取當前登入使用者的工號/帳號與姓名。若未登入則預設為 "System"
        /// </summary>
        /// <returns>包含工號與姓名的元組</returns>
        protected (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }
    }
}
