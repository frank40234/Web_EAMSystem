using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 資產資訊控制器，負責具體資產主檔的建立、查詢、修改與啟用/停用
    /// </summary>
    public class AssetInFoController : BaseController
    {
        /// <summary>
        /// 初始化資產資訊控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public AssetInFoController(ApplicationDbContext context) : base(context)
        {
        }

        // ==========================================
        // 1. 列表查詢 (Index)
        // ==========================================
        /// <summary>
        /// 資產清單列表與模糊查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（ASSET_CODE、MODEL、BRAND、ROOM_NAME）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>資產清單檢視</returns>
        [HttpGet]
        public IActionResult AssetIndex(string searchBy, string keyword, string statusFilter)
        {
            // 因為畫面需要顯示大類、類別、品名和單位名稱，我們必須把它們全部 Include 進來！
            var query = _context.AssetInfos
                .Include(a => a.AssetUnit)
                .Include(a => a.ItemName)
                    .ThenInclude(i => i!.SubAssetCategory)
                        .ThenInclude(s => s!.AssetCategory)
                .Include(b => b.StorageBin)
                    .ThenInclude(r => r!.StoreRoom)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "ASSET_CODE")
                    query = query.Where(a => a.ASSET_CODE != null && a.ASSET_CODE.Contains(keyword));
                else if (searchBy == "MODEL")
                    query = query.Where(a => a.MODEL != null && a.MODEL.Contains(keyword));
                else if (searchBy == "BRAND")
                    query = query.Where(a => a.BRAND != null && a.BRAND.Contains(keyword));
                else if (searchBy == "ROOM_NAME")
                    query = query.Where(a => a.StorageBin != null && a.StorageBin.StoreRoom != null && a.StorageBin.StoreRoom.ROOM_NAME.Contains(keyword));
            }

            if (statusFilter == "Active")
                query = query.Where(a => a.IsDisabled == false);
            else if (statusFilter == "Disabled")
                query = query.Where(a => a.IsDisabled == true);

            var assetInfos = query.OrderBy(a => a.IsDisabled)
                                  .ThenBy(a => a.ASSET_CODE)
                                  .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("AssetIndex", assetInfos);
        }

        // ==========================================
        // 2. 新增 (Create)
        // ==========================================
        /// <summary>
        /// 載入新增資產頁面
        /// </summary>
        /// <returns>新增資產檢視</returns>
        [HttpGet]
        public IActionResult AssetCreate()
        {
            // 準備大類下拉選單 (給 AJAX 連動當起點用)
            var mainCategories = _context.AssetCategories.Where(c => c.IsDisabled == false).ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "MAIN_CAT_ID", "MAIN_CAT_NAME");

            // 準備單位下拉選單
            var units = _context.AssetUnits
                .Where(u => u.IsDisabled == false)
                .Select(c => new
                {
                    ASSET_UNIT_ID = c.ASSET_UNIT_ID,
                    UnitDisplayText = c.ASSET_UNIT + "-" + c.ASSET_UNIT_CODE
                })
                .ToList();
            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
            ViewBag.UnitList = new SelectList(units, "ASSET_UNIT_ID", "UnitDisplayText");

            return View("AssetCreate");
        }

        /// <summary>
        /// 接收表單並儲存新資產，自動產生料號
        /// </summary>
        /// <param name="assetInfo">資產資訊實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssetCreate(AssetInfo assetInfo)
        {
            var currentUser = GetCurrentUser();

            // 排除系統自動產生或無需驗證的欄位
            ModelState.Remove("ASSET_ID");
            ModelState.Remove("ASSET_CODE"); // 編碼是系統自動產生的
            ModelState.Remove("CreatedDate");
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("ModifierId");
            ModelState.Remove("Modifier");
            ModelState.Remove("ModifiedDate");

            if (ModelState.IsValid)
            {
                try
                {
                    bool isDuplicate = _context.AssetInfos.Any(c =>
                        c.MODEL == assetInfo.MODEL &&
                        c.BRAND == assetInfo.BRAND &&
                        c.UNIT_ID == assetInfo.UNIT_ID);

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "新增失敗！已存在相同的資產資訊（型號、廠牌與單位相同）。";
                        return View("AssetCreate", assetInfo);
                    }

                    // 1. 抓出這筆品名的完整家族樹
                    var itemTree = _context.ItemNames
                        .Include(i => i.SubAssetCategory)
                            .ThenInclude(s => s!.AssetCategory)
                        .FirstOrDefault(i => i.IN_ID == assetInfo.IN_ID);

                    if (itemTree == null || itemTree.SubAssetCategory == null || itemTree.SubAssetCategory.AssetCategory == null)
                        throw new Exception("找不到對應的品名分類結構");

                    // 2. 組合字首 (例如: COMP-NB-MAC)
                    string prefix = $"{itemTree.SubAssetCategory!.AssetCategory!.MAIN_CAT_CODE}-{itemTree.SubAssetCategory!.SUB_CAT_CODE}-{itemTree.IN_CODE}";

                    // 3. 去資料庫找今天這個字首最大的流水號
                    var lastAsset = _context.AssetInfos
                        .Where(a => a.ASSET_CODE != null && a.ASSET_CODE.StartsWith(prefix))
                        .OrderByDescending(a => a.ASSET_CODE)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (lastAsset != null && !string.IsNullOrEmpty(lastAsset.ASSET_CODE))
                    {
                        // 取出最後 4 碼並轉成數字加 1
                        string lastNumStr = lastAsset.ASSET_CODE.Substring(lastAsset.ASSET_CODE.Length - 4);
                        if (int.TryParse(lastNumStr, out int lastNum))
                        {
                            nextNumber = lastNum + 1;
                        }
                    }

                    // 4. 賦予最終料號
                    assetInfo.ASSET_CODE = $"{prefix}-{nextNumber.ToString("D4")}";

                    // 補齊其他系統欄位
                    assetInfo.ASSET_ID = Guid.NewGuid();
                    assetInfo.CreatorId = currentUser.UserId;
                    assetInfo.Creator = currentUser.UserName;
                    assetInfo.ModifierId = currentUser.UserId;
                    assetInfo.Modifier = currentUser.UserName;
                    // CreatedDate 與 ModifiedDate 將由 ApplicationDbContext 的 SaveChanges 自動填入

                    _context.AssetInfos.Add(assetInfo);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = $"物料建立成功！自動產生料號為：{assetInfo.ASSET_CODE}";
                    return RedirectToAction(nameof(AssetCreate));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，存檔失敗：" + ex.Message;
                }
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            }

            // 失敗時重新綁定下拉選單
            var mainCategories = _context.AssetCategories.Where(c => c.IsDisabled == false).ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "MAIN_CAT_ID", "MAIN_CAT_NAME");
            var units = _context.AssetUnits
                .Where(u => u.IsDisabled == false)
                .Select(c => new
                {
                    ASSET_UNIT_ID = c.ASSET_UNIT_ID,
                    UnitDisplayText = c.ASSET_UNIT + "-" + c.ASSET_UNIT_CODE
                })
                .ToList();
            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
            ViewBag.UnitList = new SelectList(units, "ASSET_UNIT_ID", "UnitDisplayText");

            return View("AssetCreate", assetInfo);
        }

        // ==========================================
        // 3. 編輯與停用啟用
        // ==========================================
        /// <summary>
        /// 載入資產編輯頁面
        /// </summary>
        /// <param name="id">資產識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult AssetEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var units = _context.AssetUnits.Where(u => u.IsDisabled == false).ToList();
            ViewBag.UnitList = new SelectList(units, "ASSET_UNIT_ID", "ASSET_UNIT");
            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");

            // 於資料庫查詢此筆資料
            var assetInfo = _context.AssetInfos.Find(id);
            if (assetInfo == null) return NotFound();

            Guid? currentRoomId = null;
            if (assetInfo.BIN_ID.HasValue)
            {
                // 反向查出這個儲位是屬於哪一個資材室
                var currentBin = _context.StorageBins.Find(assetInfo.BIN_ID);
                if (currentBin != null)
                {
                    currentRoomId = currentBin.ROOM_ID; // 記下原本的資材室

                    // 先把原本的「儲位清單」撈出來送給畫面
                    var bins = _context.StorageBins
                        .Where(b => b.ROOM_ID == currentRoomId && b.IsDisabled == false)
                        .ToList();
                    ViewBag.BinList = new SelectList(bins, "BIN_ID", "BIN_CODE");
                }
            }
            var rooms = _context.StoreRooms.Where(r => r.IsDisabled == false).ToList();
            ViewBag.RoomList = new SelectList(rooms, "ROOM_ID", "ROOM_NAME", currentRoomId);
            return View("AssetEdit", assetInfo);
        }

        /// <summary>
        /// 接收表單並更新資產主檔
        /// </summary>
        /// <param name="id">資產識別碼</param>
        /// <param name="assetInfo">要更新的資產資訊實體</param>
        /// <returns>原編輯檢視</returns>
        [HttpPost]
        public IActionResult AssetEdit(Guid id, AssetInfo assetInfo)
        {
            var units = _context.AssetUnits.Where(u => u.IsDisabled == false).ToList();
            ViewBag.UnitList = new SelectList(units, "ASSET_UNIT_ID", "ASSET_UNIT");
            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
            var currentUser = GetCurrentUser();

            // 排除不需要驗證的欄位
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
                    // 檢查是否有其他筆資料用了同樣的型號與廠牌
                    bool isDuplicate = _context.AssetInfos.Any(c =>
                        c.ASSET_ID != id &&
                        (c.MODEL == assetInfo.MODEL && c.BRAND == assetInfo.BRAND && c.UNIT_ID == assetInfo.UNIT_ID));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的資產資訊（型號、廠牌與單位相同）。";
                        return View("AssetEdit", assetInfo);
                    }

                    var existingAsset = _context.AssetInfos.Find(id);
                    if (existingAsset != null)
                    {
                        existingAsset.MODEL = assetInfo.MODEL;
                        existingAsset.BIN_ID = assetInfo.BIN_ID;
                        existingAsset.BRAND = assetInfo.BRAND; // 修正 assignment bug (以前寫 existingAsset.BRAND = existingAsset.BRAND)
                        existingAsset.UNIT_ID = assetInfo.UNIT_ID;
                        existingAsset.ModifierId = currentUser.UserId;
                        existingAsset.Modifier = currentUser.UserName;
                        // ModifiedDate 由 ApplicationDbContext 自動更新

                        _context.Update(existingAsset);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資產資料修改成功！";

                        return RedirectToAction(nameof(AssetEdit), new { id });
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
            return View("AssetEdit", assetInfo);
        }

        /// <summary>
        /// 軟停用資產資訊
        /// </summary>
        /// <param name="id">資產識別碼</param>
        /// <returns>重新導向至資產清單</returns>
        [HttpGet]
        public IActionResult AssetDisable(Guid id)
        {
            var assetInfo = _context.AssetInfos.Find(id);

            if (assetInfo == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                assetInfo.IsDisabled = true;
                assetInfo.ModifierId = currentUser.UserId;
                assetInfo.Modifier = currentUser.UserName;

                _context.Update(assetInfo);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"資產 [{assetInfo.ASSET_CODE}] 已成功停用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(AssetIndex));
        }

        /// <summary>
        /// 恢復啟用資產資訊
        /// </summary>
        /// <param name="id">資產識別碼</param>
        /// <returns>重新導向至資產清單</returns>
        [HttpGet]
        public IActionResult AssetEnable(Guid id)
        {
            var assetInfo = _context.AssetInfos.Find(id);
            if (assetInfo == null) return NotFound();

            try
            {
                assetInfo.IsDisabled = false;
                _context.Update(assetInfo);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"資產 [{assetInfo.ASSET_CODE}] 已成功恢復啟用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(AssetIndex));
        }
    }
}
