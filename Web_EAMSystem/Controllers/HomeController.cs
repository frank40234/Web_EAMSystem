using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace Web_EAMSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

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
                subQuery = subQuery.Where(s => s.AssetCategory.MAIN_CAT_CODE == mainFilter);
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

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(string mainCode)
        {
            var subCategories = await _context.SubAssetCategories
                .Where(s => s.AssetCategory.MAIN_CAT_CODE == mainCode)
                .OrderBy(s => s.SUB_CAT_CODE)
                .Select(s => new {
                    value = s.SUB_CAT_CODE,
                    text = $"[{s.SUB_CAT_CODE}] {s.SUB_CAT_NAME}"
                })
                .ToListAsync();

            return Json(subCategories);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
