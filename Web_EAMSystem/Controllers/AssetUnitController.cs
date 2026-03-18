using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;
using System.Security.Claims;

namespace Web_EAMSystem.Controllers
{
    public class AssetUnitController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AssetUnitController (ApplicationDbContext context)
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
        public IActionResult UnitIndex(string searchBy,string keyword,string statusFilter)
        {
            //建立基礎查詢草稿
            var query = _context.AssetUnits.AsQueryable();

            //篩選指令輸入關鍵字，加入過濾條件
            if (!string.IsNullOrEmpty(keyword))
            {
                if(searchBy == "ASSET_UNIT_CODE")
                {
                    query =query.Where(c=> c.ASSET_UNIT_CODE.Contains(keyword));
                }
                else if (searchBy == "ASSET_UNIT")
                {
                    query = query.Where(c => c.ASSET_UNIT.Contains(keyword));
                }
                else if (searchBy == "Creater")
                {
                    query = query.Where(c => c.Creator.Contains(keyword));
                }
            }
            if(statusFilter == "Active")
            {
                query = query.Where(c => c.IsDisabled == false);
            }
            else if (statusFilter == "Disable")
            {
                query = query.Where(c => c.IsDisabled == true);
            }

            //多重排序
            // OrderBy(IsDisabled)：false(0/使用中) 會排在 true(1/已停用) 的前面！
            // ThenByDescending(CreatedDate)：同狀態的資料，越新的排越上面！
            var unitSort = query.OrderBy(c => c.IsDisabled)
                .ThenByDescending(c => c.CreatedDate)
                .ToList();

            // 4.利用 ViewBag 把剛剛搜尋的條件存起來傳回畫面
            // 使用者搜完之後，下拉選單跟輸入框才不會變回空白
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter; // 記錄目前的狀態篩選

            return View("UnitIndex", unitSort);
        }
        /// <summary>
        /// 新增大類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UnitCreate()
        {
            return View("UnitCreate");
        }
        // 2. POST: 負責接收使用者填寫的資料，並存入資料庫
        [HttpPost]
        [ValidateAntiForgeryToken] // 資安防護：防止跨站請求偽造 (CSRF) 攻擊
        public IActionResult UnitCreate(AssetUnit assetUnit)
        {
            // 防呆機制：檢查資料庫是否已有重複資料
            // ==========================================
            bool isDuplicate = _context.AssetUnits.Any(c =>
                c.ASSET_UNIT == assetUnit.ASSET_UNIT ||
                c.ASSET_UNIT_CODE == assetUnit.ASSET_UNIT_CODE);

            var currentUser = GetCurrentUser();


            if (isDuplicate)
            {
                // 如果發現重複，設定錯誤提示
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的大類代號或大類名稱。";

                // 直接退回新增畫面，並把使用者填到一半的 unit 傳回去
                return View("UnitCreate", assetUnit);
            }

            //  關鍵新增：在驗證之前，手動排除那些不需要使用者從畫面上輸入的欄位！
            ModelState.Remove("ASSET_UNIT_ID");
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
                    assetUnit.ASSET_UNIT_ID = Guid.NewGuid();
                    assetUnit.CreatorId = currentUser.UserId;
                    assetUnit.Creator = currentUser.UserName;
                    assetUnit.ModifierId = currentUser.UserId;
                    assetUnit.Modifier = currentUser.UserName;// 讓異動者等於建檔者
                    assetUnit.CreatedDate = DateTime.Now;
                    assetUnit.ModifiedDate = DateTime.Now;         // 讓異動時間等於現在
                    assetUnit.IsDisabled = false;

                    _context.AssetUnits.Add(assetUnit);
                    _context.SaveChanges();

                    // 存檔成功，設定成功訊息
                    TempData["SuccessMessage"] = "資料大類已成功新增。";
                    return RedirectToAction(nameof(UnitCreate));
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

            return View(assetUnit);
        }
        [HttpGet]
        public IActionResult UnitEdit(Guid id)
        {
            if (id == null) return NotFound();

            //於資料庫查詢此資料
            var unit = _context.AssetUnits.Find(id);
            if (unit ==null) return NotFound();
 
            return View("UnitEdit",unit);
        }
        [HttpPost]
        public IActionResult UnitEdit(Guid id , AssetUnit assetUnit)
        {
            var currentUser = GetCurrentUser();
            // 排除不需要的驗證欄位
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
                    //檢查是否有「其他筆資料」用了同樣代號或名稱
                    bool isDuplicate = _context.AssetUnits.Any(c =>
                    c.ASSET_UNIT_ID != id &&
                    (c.ASSET_UNIT_CODE == assetUnit.ASSET_UNIT_CODE || c.ASSET_UNIT == assetUnit.ASSET_UNIT));
                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的單位代號或名稱。";
                        return View("UnitEdit", assetUnit);
                    }


                    //  標準更新流程：先從資料庫拿出舊包裹，再把新東西塞進去
                    var existingUnit = _context.AssetUnits.Find(id);
                    if (existingUnit != null)
                    {
                        existingUnit.ASSET_UNIT = assetUnit.ASSET_UNIT;
                        existingUnit.ASSET_UNIT_CODE = assetUnit.ASSET_UNIT_CODE;
                        existingUnit.ModifierId = currentUser.UserId; // 畫面上填寫的異動者，之後改為登入者
                        existingUnit.Modifier = currentUser.UserName; // 畫面上填寫的異動者，之後改為登入者
                        existingUnit.ModifiedDate = DateTime.Now;  // 系統押上最新修改時間

                        _context.Update(existingUnit);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(UnitEdit)); // 修改完，自動跳回列表頁！
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，修改失敗：" + ex.Message;
                }
            }
            return View("UnitEdit",assetUnit);
        }

        /// <summary>
        /// 停用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UnitDisable(Guid id)
        {
            // 1. 去資料庫把這筆資料找出來
            var unit = _context.AssetUnits.Find(id);

            if (unit == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                // 2. 執行軟刪除：把停用標記設為 true
                unit.IsDisabled = true;
                unit.ModifierId = currentUser.UserId;
                unit.Modifier = currentUser.UserName;

                // 💡 實務細節：停用也算是一種「異動」，所以我們要更新異動時間
                unit.ModifiedDate = DateTime.Now;
                // (因為這裡沒有表單可以讓使用者輸入名字，異動者就維持上一次的人，或是你可以寫死成 "System")

                // 3. 存檔
                _context.Update(unit);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{unit.ASSET_UNIT}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            // 4. 完成後，跳回列表頁
            return RedirectToAction(nameof(UnitIndex));
        }

        /// <summary>
        /// 啟用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UnitEnable(Guid id)
        {
            var unit = _context.AssetUnits.Find(id);
            if (unit == null) return NotFound();

            try
            {
                unit.IsDisabled = false; // 改為啟用
                unit.ModifiedDate = DateTime.Now;

                _context.Update(unit);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{unit.ASSET_UNIT}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(UnitIndex));
        }

        private (string UserId , string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return(userId, userName);
        }

    }
}
