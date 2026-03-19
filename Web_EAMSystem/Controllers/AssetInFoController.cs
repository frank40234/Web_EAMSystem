using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    public class AssetInFoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetInFoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. 列表查詢 (Index)
        // ==========================================
        [HttpGet]
        public IActionResult AssetIndex(string searchBy, string keyword, string statusFilter)
        {
            // 因為畫面需要顯示大類、類別、品名和單位名稱，我們必須把它們全部 Include 進來！
            var query = _context.AssetInfos
                .Include(a => a.AssetUnit)
                .Include(a => a.ItemName)
                    .ThenInclude(i => i.SubAssetCategory)
                        .ThenInclude(s => s.AssetCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "ASSET_CODE")
                    query = query.Where(a => a.ASSET_CODE.Contains(keyword));
                else if (searchBy == "MODEL")
                    query = query.Where(a => a.MODEL.Contains(keyword));
                else if (searchBy == "BRAND")
                    query = query.Where(a => a.BRAND.Contains(keyword));
            }

            if (statusFilter == "Active")
                query = query.Where(a => a.IsDisabled == false);
            else if (statusFilter == "Disabled")
                query = query.Where(a => a.IsDisabled == true);

            var assetInfos = query.OrderBy(a => a.IsDisabled)
                                  .ThenByDescending(a => a.CreatedDate)
                                  .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("AssetIndex", assetInfos);
        }

        // ==========================================
        // 2. 新增 (Create)
        // ==========================================
        [HttpGet]
        public IActionResult AssetCreate()
        {
            // 準備大類下拉選單 (給 AJAX 連動當起點用)
            var mainCategories = _context.AssetCategories.Where(c => c.IsDisabled == false).ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "MAIN_CAT_ID", "MAIN_CAT_NAME");

            // 準備單位下拉選單
            var units = _context.AssetUnits.Where(u => u.IsDisabled == false).ToList();
            ViewBag.UnitList = new SelectList(units, "ASSET_UNIT_ID", "ASSET_UNIT");

            return View("AssetCreate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssetCreate(AssetInfo assetInfo)
        {
            var currentUser = GetCurrentUser();

            // 排除系統自動產生或無需驗證的欄位
            ModelState.Remove("ASSET_ID");
            ModelState.Remove("ASSET_CODE"); // 💡 編碼是我們等一下要自己產生的！
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
                    // 核心邏輯：自動產生料號 (大類代碼-類別代碼-品名代碼-0001)

                    // 1. 抓出這筆品名的完整家族樹
                    var itemTree = _context.ItemNames
                        .Include(i => i.SubAssetCategory)
                            .ThenInclude(s => s.AssetCategory)
                        .FirstOrDefault(i => i.IN_ID == assetInfo.IN_ID);

                    if (itemTree == null) throw new Exception("找不到對應的品名資料");

                    // 2. 組合字首 (例如: COMP-NB-MAC)
                    string prefix = $"{itemTree.SubAssetCategory.AssetCategory.MAIN_CAT_CODE}-{itemTree.SubAssetCategory.SUB_CAT_CODE}-{itemTree.IN_CODE}";

                    // 3. 去資料庫找今天這個字首最大的流水號
                    var lastAsset = _context.AssetInfos
                        .Where(a => a.ASSET_CODE.StartsWith(prefix))
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
                    assetInfo.CreatedDate = DateTime.Now;
                    assetInfo.ModifiedDate = DateTime.Now;
                    assetInfo.IsDisabled = false;

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

            // 若失敗，重新準備下拉選單
            ViewBag.MainCategoryList = new SelectList(_context.AssetCategories.Where(c => c.IsDisabled == false), "MAIN_CAT_ID", "MAIN_CAT_NAME");
            ViewBag.UnitList = new SelectList(_context.AssetUnits.Where(u => u.IsDisabled == false), "ASSET_UNIT_ID", "ASSET_UNIT");
            return View(assetInfo);
        }

        // ==========================================
        // 3. AJAX 給前端連動下拉選單用的 API
        // ==========================================
        [HttpGet]
        public IActionResult GetSubCategories(Guid mainCatId)
        {
            var data = _context.SubAssetCategories
                .Where(s => s.MAIN_CAT_ID == mainCatId && s.IsDisabled == false)
                .Select(s => new { value = s.SUB_CAT_ID, text = s.SUB_CAT_CODE + " - " + s.SUB_CAT_NAME })
                .ToList();
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetItemNames(Guid subCatId)
        {
            var data = _context.ItemNames
                .Where(i => i.SUB_CAT_ID == subCatId && i.IsDisabled == false)
                .Select(i => new { value = i.IN_ID, text = i.IN_CODE + " - " + i.IN })
                .ToList();
            return Json(data);
        }

        // ==========================================
        // 4. 共用方法 (停用、啟用、取得使用者)
        // ==========================================
        // 編輯 (Edit), 停用 (Disable), 啟用 (Enable) 邏輯與 AssetCategory 完全相同，這裡先省略細節以保持程式碼簡潔。
        // 你可以直接將之前的寫法複製過來，把 AssetCategory 改成 AssetInfo 即可！

        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }
    }
}
