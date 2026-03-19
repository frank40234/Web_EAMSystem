using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    public class ItemNameController : Controller
    {
        // 宣告一個私有變數來存放「資料庫總管」
        private readonly ApplicationDbContext _context;
        //自動設定好DBContext
        public ItemNameController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 大類清單
        /// </summary>
        /// <param name="searchBy"></param>
        /// <param name="keyword"></param>
        /// <param name="statusFilter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ItemNameIndex(string searchBy, string keyword, string statusFilter)
        {
            //建立基礎查詢草稿
            var query = _context.ItemNames.Include(c => c.SubAssetCategory).AsQueryable();

            // 如果使用者有輸入關鍵字，我們就根據選擇的欄位加入過濾條件
            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "IN")
                {
                    // Contains 在 SQL 裡會被翻譯成 LIKE '%關鍵字%'
                    query = query.Where(c => c.IN.Contains(keyword));
                }
                else if (searchBy == "IN_CODE")
                {
                    query = query.Where(c => c.IN_CODE.Contains(keyword));
                }
                else if (searchBy == "Creator")
                {
                    query = query.Where(c => c.Creator.Contains(keyword));
                }
                else if (searchBy == "SUB_CAT_CODE")
                {
                    // 💡 導師重點：這就是 Include 的威力！跨表查詢對應的類別代號
                    query = query.Where(c => c.SubAssetCategory.SUB_CAT_CODE.Contains(keyword));
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

            // 3. 新增：多重排序 (先按狀態排，再按日期排)
            // OrderBy(IsDisabled)：false(0/使用中) 會排在 true(1/已停用) 的前面！
            // ThenByDescending(CreatedDate)：同狀態的資料，越新的排越上面！
            var itemNameSort = query.OrderBy(c => c.IsDisabled)
                                    .ThenBy(c => c.SubAssetCategory.SUB_CAT_CODE) // 加入此行，依子類別代碼正序排列
                                    .ThenBy(c => c.IN)
                                    .ThenBy(c => c.IN_CODE)
                                    .ToList();

            // 4.利用 ViewBag 把剛剛搜尋的條件存起來傳回畫面
            // 這樣使用者搜完之後，下拉選單跟輸入框才不會變回空白
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter; // 記錄目前的狀態篩選

            return View("ItemNameIndex", itemNameSort);
        }

        /// <summary>
        /// 新增大類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ItemNameCreate()
        {
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

            return View("ItemNameCreate");
        }
        // 2. POST: 負責接收使用者填寫的資料，並存入資料庫
        [HttpPost]
        [ValidateAntiForgeryToken] // 資安防護：防止跨站請求偽造 (CSRF) 攻擊
        public IActionResult ItemNameCreate(ItemName itemName)
        {
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");
            // 防呆機制：檢查資料庫是否已有重複資料
            // ==========================================
            bool isDuplicate = _context.ItemNames.Any(c =>
                c.IN_CODE == itemName.IN_CODE &&
                c.IN == itemName.IN);

            var currentUser = GetCurrentUser();


            if (isDuplicate)
            {
                // 如果發現重複，設定錯誤提示
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的大類代號或大類名稱。";

                // 直接退回新增畫面，並把使用者填到一半的 category 傳回去
                return View("ItemNameCreate", itemName);
            }

            //  關鍵新增：在驗證之前，手動排除那些不需要使用者從畫面上輸入的欄位！
            ModelState.Remove("IN_ID");
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
                    itemName.IN_ID = Guid.NewGuid();
                    itemName.CreatorId = currentUser.UserId;
                    itemName.Creator = currentUser.UserName;
                    itemName.ModifierId = currentUser.UserId;
                    itemName.Modifier = currentUser.UserName;
                    itemName.CreatedDate = DateTime.Now;
                    itemName.Modifier = itemName.Creator;         // 讓異動者等於建檔者
                    itemName.ModifiedDate = DateTime.Now;         // 讓異動時間等於現在
                    itemName.IsDisabled = false;

                    _context.ItemNames.Add(itemName);
                    _context.SaveChanges();

                    // 存檔成功，設定成功訊息
                    TempData["SuccessMessage"] = "資料大類已成功新增。";
                    return RedirectToAction(nameof(ItemNameCreate));
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

            return View(itemName);
        }

        /// <summary>
        /// 大類編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ItemNameEdit(Guid id)
        {
            // 1.資料庫撈取「大類資料」。
            // 讓使用者選擇「尚未停用(IsDisabled == false)」的大類
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

            if (id == null) return NotFound();

            //於資料庫查詢此筆資料
            var itemName = _context.ItemNames.Find(id);
            if (itemName == null) return NotFound();

            return View("ItemNameEdit", itemName);
        }
        [HttpPost]
        public IActionResult ItemNameEdit(Guid id, ItemName itemName)
        {
            // 1.資料庫撈取「大類資料」。
            // 讓使用者選擇「尚未停用(IsDisabled == false)」的大類
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    // 將存檔的值改為 SUB_CAT_ID
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    // 畫面上顯示的文字保持不變，讓使用者看得很舒服
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            // 綁定 SelectList 時，第二個參數對應為 "SUB_CAT_ID"
            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

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
                    bool isDuplicate = _context.ItemNames.Any(c =>
                        c.IN_ID != id &&
                        (c.IN_CODE == itemName.IN_CODE && c.IN == itemName.IN));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的大類代號或名稱。";
                        return View("CategoryEdit", itemName);
                    }

                    //  標準更新流程：先從資料庫拿出舊包裹，再把新東西塞進去
                    var existingItemName = _context.ItemNames.Find(id);
                    if (existingItemName != null)
                    {
                        existingItemName.SUB_CAT_ID = itemName.SUB_CAT_ID;
                        existingItemName.IN = itemName.IN;
                        existingItemName.IN_CODE = itemName.IN_CODE;
                        existingItemName.ModifierId = currentUser.UserId; // 畫面上填寫的異動者，之後改為登入者
                        existingItemName.Modifier = currentUser.UserName; // 畫面上填寫的異動者，之後改為登入者
                        existingItemName.ModifiedDate = DateTime.Now;  // 系統押上最新修改時間

                        _context.Update(existingItemName);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(ItemNameEdit)); // 修改完，自動跳回列表頁！
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

            return View("ItemNameEdit", itemName);
        }

        /// <summary>
        /// 停用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ItemNameDisable(Guid id)
        {
            // 1. 去資料庫把這筆資料找出來
            var itemName = _context.ItemNames.Find(id);

            if (itemName == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                // 2. 執行軟刪除：把停用標記設為 true
                itemName.IsDisabled = true;
                itemName.ModifierId = currentUser.UserId;
                itemName.Modifier = currentUser.UserName;

                // 💡 實務細節：停用也算是一種「異動」，所以我們要更新異動時間
                itemName.ModifiedDate = DateTime.Now;
                // (因為這裡沒有表單可以讓使用者輸入名字，異動者就維持上一次的人，或是你可以寫死成 "System")

                // 3. 存檔
                _context.Update(itemName);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{itemName.IN}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            // 4. 完成後，跳回列表頁
            return RedirectToAction(nameof(ItemNameIndex));
        }

        /// <summary>
        /// 啟用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ItemNameEnable(Guid id)
        {
            var itemName = _context.ItemNames.Find(id);
            if (itemName == null) return NotFound();

            try
            {
                itemName.IsDisabled = false; // 改為啟用
                itemName.ModifiedDate = DateTime.Now;

                _context.Update(itemName);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{itemName.IN}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(ItemNameIndex));
        }

        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }

    }
}

