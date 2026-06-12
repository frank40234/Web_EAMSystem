using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 首頁控制器，處理資產庫存儀表板以及系統首頁資料呈現
    /// </summary>
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// 初始化首頁控制器
        /// </summary>
        /// <param name="logger">日誌服務</param>
        /// <param name="context">資料庫上下文</param>
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) : base(context)
        {
            _logger = logger;
        }

        /// <summary>
        /// 顯示資產庫存首頁與過濾條件
        /// </summary>
        /// <param name="mainFilter">大類代碼過濾條件</param>
        /// <param name="subFilter">次類代碼過濾條件</param>
        /// <param name="roomFilter">資材室名稱過濾條件</param>
        /// <returns>庫存儀表板檢視</returns>
        public async Task<IActionResult> Index(string? mainFilter, string? subFilter, string? roomFilter)
        {
            // 1. 準備基礎查詢
            var query = from inv in _context.Inventories
                        join asset in _context.AssetInfos on inv.ASSET_ID equals asset.ASSET_ID
                        join bin in _context.StorageBins on inv.BIN_ID equals bin.BIN_ID
                        join room in _context.StoreRooms on bin.ROOM_ID equals room.ROOM_ID
                        join item in _context.ItemNames on asset.IN_ID equals item.IN_ID
                        join unit in _context.AssetUnits on asset.UNIT_ID equals unit.ASSET_UNIT_ID
                        join sub in _context.SubAssetCategories on item.SUB_CAT_ID equals sub.SUB_CAT_ID
                        join main in _context.AssetCategories on sub.MAIN_CAT_ID equals main.MAIN_CAT_ID
                        select new { inv, asset, item, sub, main, unit, bin, room };

            // 2. 篩選邏輯
            if (!string.IsNullOrEmpty(mainFilter))
                query = query.Where(q => q.main.MAIN_CAT_CODE == mainFilter);

            if (!string.IsNullOrEmpty(subFilter))
                query = query.Where(q => q.sub.SUB_CAT_CODE == subFilter);

            if (!string.IsNullOrEmpty(roomFilter))
                query = query.Where(q => q.room.ROOM_NAME == roomFilter);

            // 3. 排序與執行
            var results = await query.OrderBy(q => q.asset.ASSET_CODE).ToListAsync();

            // 4. 準備選單資料 (ViewBag 名稱更新)
            ViewBag.MainCategoryList = new SelectList(await _context.AssetCategories
                .OrderBy(c => c.MAIN_CAT_CODE)
                .Select(c => new { Value = c.MAIN_CAT_CODE, Text = $"[{c.MAIN_CAT_CODE}] {c.MAIN_CAT_NAME}" })
                .ToListAsync(), "Value", "Text", mainFilter);
            
            var subQuery = _context.SubAssetCategories.AsQueryable();
            if (!string.IsNullOrEmpty(mainFilter))
            {
                subQuery = subQuery.Where(s => s.AssetCategory != null && s.AssetCategory.MAIN_CAT_CODE == mainFilter);
            }
            ViewBag.SubCategoryList = new SelectList(await subQuery
                .OrderBy(s => s.SUB_CAT_CODE)
                .Select(s => new { Value = s.SUB_CAT_CODE, Text = $"[{s.SUB_CAT_CODE}] {s.SUB_CAT_NAME}" })
                .ToListAsync(), "Value", "Text", subFilter);
            
            ViewBag.StoreRoomList = new SelectList(await _context.StoreRooms.OrderBy(r => r.ROOM_NAME).ToListAsync(), "ROOM_NAME", "ROOM_NAME", roomFilter);

            ViewBag.CurrentMain = mainFilter;
            ViewBag.CurrentSub = subFilter;
            ViewBag.CurrentRoom = roomFilter;

            return View(results.Cast<dynamic>());
        }

        /// <summary>
        /// 提供 AJAX 連動，依大類代碼取得次類清單
        /// </summary>
        /// <param name="mainCode">大類代碼</param>
        /// <returns>次類 JSON 清單</returns>
        [HttpGet]
        public async Task<JsonResult> GetSubCategories(string mainCode)
        {
            var subCategories = await _context.SubAssetCategories
                .Where(s => s.AssetCategory != null && s.AssetCategory.MAIN_CAT_CODE == mainCode)
                .OrderBy(s => s.SUB_CAT_CODE)
                .Select(s => new {
                    value = s.SUB_CAT_CODE,
                    text = $"[{s.SUB_CAT_CODE}] {s.SUB_CAT_NAME}"
                })
                .ToListAsync();

            return Json(subCategories);
        }

        /// <summary>
        /// 系統隱私條款頁面
        /// </summary>
        /// <returns>隱私檢視</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// 錯誤處理頁面
        /// </summary>
        /// <returns>錯誤提示檢視</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
