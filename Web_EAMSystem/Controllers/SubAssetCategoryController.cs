using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    public class SubAssetCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SubAssetCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult SubCategoryIndex(string searchBy, string keyword, string statusFilter)
        {
            //建立基礎查詢草稿
            var query = _context.SubAssetCategories.AsQueryable();

            // 如果使用者有輸入關鍵字，我們就根據選擇的欄位加入過濾條件
            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "SUB_CAT_NAME")
                {
                    // Contains 在 SQL 裡會被翻譯成 LIKE '%關鍵字%'
                    query = query.Where(c => c.SUB_CAT_NAME.Contains(keyword));
                }
                else if (searchBy == "SUB_CAT_CODE")
                {
                    query = query.Where(c => c.SUB_CAT_CODE.Contains(keyword));
                }
                else if (searchBy == "Creator")
                {
                    query = query.Where(c => c.Creator.Contains(keyword));
                }
            }
            if (statusFilter == "Active")
            {
                query = query.Where(c => c.IsDisabled == false); // 只找使用中
            }
            else if (statusFilter == "Disabled")
            {
                query = query.Where(c => c.IsDisabled == true);  // 只找已停用
            }

            // 3. 🌟 新增：多重排序 (先按狀態排，再按日期排)
            // OrderBy(IsDisabled)：false(0/使用中) 會排在 true(1/已停用) 的前面！
            // ThenByDescending(CreatedDate)：同狀態的資料，越新的排越上面！
            var categories = query.OrderBy(c => c.IsDisabled)
                                  .ThenByDescending(c => c.CreatedDate)
                                  .ToList();

            // 4.利用 ViewBag 把剛剛搜尋的條件存起來傳回畫面
            // 這樣使用者搜完之後，下拉選單跟輸入框才不會變回空白
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter; // 記錄目前的狀態篩選
            return View("SubCategoryIndex",categories);
        }
        [HttpGet]
        public IActionResult SubCategoryCreate()
        {
            return View("SubCategoryCreate");
        }
        // 2. POST: 負責接收使用者填寫的資料，並存入資料庫
        [HttpPost]
        [ValidateAntiForgeryToken] // 資安防護：防止跨站請求偽造 (CSRF) 攻擊
        public IActionResult SubCategoryCreate(SubAssetCategory subCategory , AssetCategory category)
        {
            // 防呆機制：檢查資料庫是否已有重複資料
            // ==========================================
            bool isDuplicate = _context.AssetCategories.Any(c =>
                c.MAIN_CAT_CODE == subCategory.SUB_CAT_CODE ||
                c.MAIN_CAT_NAME == subCategory.SUB_CAT_NAME);

            var currentUser = GetCurrentUser();


            if (isDuplicate)
            {
                // 如果發現重複，設定錯誤提示
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的大類代號或大類名稱。";

                // 直接退回新增畫面，並把使用者填到一半的 category 傳回去
                return View("CategoryCreate", subCategory);
            }

            //  關鍵新增：在驗證之前，手動排除那些不需要使用者從畫面上輸入的欄位！
            ModelState.Remove("SUB_CAT_ID");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("ModifierId");
            ModelState.Remove("Modifier");      // 告訴系統：不要檢查 Modifier！
            ModelState.Remove("ModifiedDate");  // 告訴系統：不要檢查 ModifiedDate！

            // 排除完畢後，再進行驗證
            if (ModelState.IsValid)
            {
                // 使用 try-catch 保護資料庫存檔過程
                try
                {
                    subCategory.SUB_CAT_ID = Guid.NewGuid();
                    subCategory.CreatorId = currentUser.UserId;
                    subCategory.Creator = currentUser.UserName;
                    subCategory.ModifierId = currentUser.UserId;
                    subCategory.Modifier = currentUser.UserName;
                    subCategory.CreatedDate = DateTime.Now;
                    subCategory.Modifier = subCategory.Creator;         // 讓異動者等於建檔者
                    subCategory.ModifiedDate = DateTime.Now;         // 讓異動時間等於現在
                    subCategory.IsDisabled = false;

                    _context.SubAssetCategories.Add(subCategory);
                    _context.SaveChanges();

                    // 存檔成功，設定成功訊息
                    TempData["SuccessMessage"] = "資料類別已成功新增。";
                    return RedirectToAction(nameof(SubCategoryCreate));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，存檔失敗：" + ex.Message;
                }
            }
            else
            {

                var errors = string.Join("; ", ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage));

                TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            }

            return View(subCategory);
        }









        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }

    }
}
