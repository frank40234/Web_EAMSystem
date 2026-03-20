using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var query = _context.SubAssetCategories.Include(c => c.AssetCategory).AsQueryable();

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
            
            // 1.資料庫撈取「大類資料」。
            // 讓使用者選擇「尚未停用(IsDisabled == false)」的大類
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            return View("SubCategoryCreate");
        }

        // 2. POST: 負責接收使用者填寫的資料，並存入資料庫
        [HttpPost]
        [ValidateAntiForgeryToken] // 資安防護：防止跨站請求偽造 (CSRF) 攻擊
        public IActionResult SubCategoryCreate(SubAssetCategory subCategory )
        {
            
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            // 防呆機制：檢查資料庫是否已有重複資料
            // ==========================================
            bool isDuplicate = _context.SubAssetCategories.Any(c =>
                 c.MAIN_CAT_ID == subCategory.MAIN_CAT_ID &&
                (c.SUB_CAT_CODE == subCategory.SUB_CAT_CODE || c.SUB_CAT_NAME == subCategory.SUB_CAT_NAME ));

            var currentUser = GetCurrentUser();

            if (isDuplicate)
            {
                // 如果發現重複，設定錯誤提示
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的類別代號和類別名稱。";

                // 直接退回新增畫面，並把使用者填到一半的 category 傳回去
                return View("SubCategoryCreate", subCategory);
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

        /// <summary>
        /// 大類編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SubCategoryEdit(Guid id)
        {
            
            if (id == null) return NotFound();

            //於資料庫查詢此筆資料
            var category = _context.SubAssetCategories.Find(id);
            if (category == null) return NotFound();

            // 1.資料庫撈取「大類資料」。
            // 讓使用者選擇「尚未停用(IsDisabled == false)」的大類
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            return View("SubCategoryEdit",category);
        }
        [HttpPost]
        public IActionResult SubCategoryEdit(Guid id, SubAssetCategory category)
        {
            
            // 1.資料庫撈取「大類資料」。
            // 讓使用者選擇「尚未停用(IsDisabled == false)」的大類
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            var currentUser = GetCurrentUser();
            // 排除不需要驗證的欄位 (因為這些是系統產生的或是舊資料)
            ModelState.Remove("CreatedDate");
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("ModifiedDate");
            ModelState.Remove("ModifierId");
            ModelState.Remove("Modifier");



            if (ModelState.IsValid)
            {
                try
                {
                    //  檢查是否有「其他筆資料」用了同樣的代號或名稱 (要排除自己)
                    bool isDuplicate = _context.SubAssetCategories.Any(c =>
                        c.SUB_CAT_ID != id && c.MAIN_CAT_ID == category.MAIN_CAT_ID&&
                        (c.SUB_CAT_CODE == category.SUB_CAT_CODE || c.SUB_CAT_NAME == category.SUB_CAT_NAME ));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的類別資料。";
                        return View("SubCategoryEdit", category);
                    }

                    //  標準更新流程：先從資料庫拿出舊包裹，再把新東西塞進去
                    var existingCategory = _context.SubAssetCategories.Find(id);
                    if (existingCategory != null)
                    {
                        existingCategory.MAIN_CAT_ID = category.MAIN_CAT_ID;
                        existingCategory.SUB_CAT_NAME = category.SUB_CAT_NAME;
                        existingCategory.SUB_CAT_CODE = category.SUB_CAT_CODE;
                        existingCategory.ModifierId = currentUser.UserId; // 畫面上填寫的異動者，之後改為登入者
                        existingCategory.Modifier = currentUser.UserName; // 畫面上填寫的異動者，之後改為登入者
                        existingCategory.ModifiedDate = DateTime.Now;  // 系統押上最新修改時間

                        _context.Update(existingCategory);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(SubCategoryEdit)); 
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，修改失敗：" + ex.Message;
                }
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            }

            return View("SubCategoryEdit", category);
        }

        /// <summary>
        /// 停用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SubCategoryDisable(Guid id)
        {
            
            // 1. 去資料庫把這筆資料找出來
            var category = _context.SubAssetCategories.Find(id);

            if (category == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                // 2. 執行軟刪除：把停用標記設為 true
                category.IsDisabled = true;
                category.ModifierId = currentUser.UserId;
                category.Modifier = currentUser.UserName;

                // 💡 實務細節：停用也算是一種「異動」，所以我們要更新異動時間
                category.ModifiedDate = DateTime.Now;
                // (因為這裡沒有表單可以讓使用者輸入名字，異動者就維持上一次的人，或是你可以寫死成 "System")

                // 3. 存檔
                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{category.SUB_CAT_NAME}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            // 4. 完成後，跳回列表頁
            return RedirectToAction(nameof(SubCategoryIndex));
        }

        /// <summary>
        /// 啟用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SubCategoryEnable(Guid id)
        {
            
            var category = _context.SubAssetCategories.Find(id);
            if (category == null) return NotFound();

            try
            {
                category.IsDisabled = false; // 改為啟用
                category.ModifiedDate = DateTime.Now;

                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{category.SUB_CAT_NAME}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(SubCategoryIndex));
        }

        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }

    }
}
