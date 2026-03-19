using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;
using System.Security.Claims;

namespace Web_EAMSystem.Controllers
{
    public class AssetInFoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AssetInFoController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult AssetIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AssetCreate()
        {
            return View("AssetCreate");
        }
        [HttpPost]
        public IActionResult AssetCreate(AssetInfo assetInfo)
        {
            //var currentUser = GetCurrentUser();
            //// 排除不需要驗證的欄位 (因為這些是系統產生的或是舊資料)

            //ModelState.Remove("CreatedDate");
            //ModelState.Remove("CreatorId");
            //ModelState.Remove("Creator");
            //ModelState.Remove("ModifiedDate");
            //ModelState.Remove("ModifierId");
            //ModelState.Remove("Modifier");

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        //  檢查是否有「其他筆資料」用了同樣的代號或名稱 (要排除自己)
            //        bool isDuplicate = _context.AssetCategories.Any(c =>
            //            c.MAIN_CAT_ID != id &&
            //            (c.MAIN_CAT_CODE == category.MAIN_CAT_CODE || c.MAIN_CAT_NAME == category.MAIN_CAT_NAME));

            //        if (isDuplicate)
            //        {
            //            TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的大類代號或名稱。";
            //            return View("CategoryEdit", category);
            //        }

            //        //  標準更新流程：先從資料庫拿出舊包裹，再把新東西塞進去
            //        var existingCategory = _context.AssetCategories.Find(id);
            //        if (existingCategory != null)
            //        {
            //            existingCategory.MAIN_CAT_NAME = category.MAIN_CAT_NAME;
            //            existingCategory.MAIN_CAT_CODE = category.MAIN_CAT_CODE;
            //            existingCategory.ModifierId = currentUser.UserId; // 畫面上填寫的異動者，之後改為登入者
            //            existingCategory.Modifier = currentUser.UserName; // 畫面上填寫的異動者，之後改為登入者
            //            existingCategory.ModifiedDate = DateTime.Now;  // 系統押上最新修改時間

            //            _context.Update(existingCategory);
            //            _context.SaveChanges();

            //            TempData["SuccessMessage"] = "資料修改成功！";

            //            return RedirectToAction(nameof(CategoryEdit)); // 修改完，自動跳回列表頁！
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        TempData["ErrorMessage"] = "系統發生錯誤，修改失敗：" + ex.Message;
            //    }
            //}
            //else
            //{
            //    var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            //    TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            //}

            return View("AssetCreate",assetInfo); 
        }
        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }
    }
}
