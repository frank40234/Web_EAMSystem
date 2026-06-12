using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 庫存查詢控制器，負責企業內部資產在各儲位之庫存數據查詢、過濾與呈現
    /// </summary>
    public class InventoryController : BaseController
    {
        /// <summary>
        /// 初始化庫存查詢控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public InventoryController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 顯示庫存清單，並依大類、小類及資材室進行篩選
        /// </summary>
        /// <param name="mainFilter">大類代碼過濾條件</param>
        /// <param name="subFilter">次類代碼過濾條件</param>
        /// <param name="roomFilter">資材室名稱過濾條件</param>
        /// <returns>庫存清單檢視</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string? mainFilter, string? subFilter, string? roomFilter)
        {
            // 建立包含完整導覽屬性的查詢草稿
            var query = _context.Inventories
                .Include(i => i.AssetInfo)
                    .ThenInclude(a => a != null ? a.ItemName : null)
                        .ThenInclude(it => it != null ? it.SubAssetCategory : null)
                            .ThenInclude(s => s != null ? s.AssetCategory : null)
                .Include(i => i.AssetInfo)
                    .ThenInclude(a => a != null ? a.AssetUnit : null)
                .Include(i => i.StorageBin)
                    .ThenInclude(b => b != null ? b.StoreRoom : null)
                .AsQueryable();

            // 套用多重篩選條件
            if (!string.IsNullOrEmpty(mainFilter))
            {
                query = query.Where(q => q.AssetInfo != null && 
                                         q.AssetInfo.ItemName != null && 
                                         q.AssetInfo.ItemName.SubAssetCategory != null && 
                                         q.AssetInfo.ItemName.SubAssetCategory.AssetCategory != null && 
                                         q.AssetInfo.ItemName.SubAssetCategory.AssetCategory.MAIN_CAT_CODE == mainFilter);
            }

            if (!string.IsNullOrEmpty(subFilter))
            {
                query = query.Where(q => q.AssetInfo != null && 
                                         q.AssetInfo.ItemName != null && 
                                         q.AssetInfo.ItemName.SubAssetCategory != null && 
                                         q.AssetInfo.ItemName.SubAssetCategory.SUB_CAT_CODE == subFilter);
            }

            if (!string.IsNullOrEmpty(roomFilter))
            {
                query = query.Where(q => q.StorageBin != null && 
                                         q.StorageBin.StoreRoom != null && 
                                         q.StorageBin.StoreRoom.ROOM_NAME == roomFilter);
            }

            // 排序並投影到 ViewModel
            var results = await query
                .OrderBy(q => q.AssetInfo != null ? q.AssetInfo.ASSET_CODE : string.Empty)
                .Select(q => new InventoryIndexViewModel
                {
                    ASSET_CODE = q.AssetInfo != null ? q.AssetInfo.ASSET_CODE : string.Empty,
                    QTY = q.QTY,
                    MAIN_CAT_CODE = (q.AssetInfo != null && q.AssetInfo.ItemName != null && q.AssetInfo.ItemName.SubAssetCategory != null && q.AssetInfo.ItemName.SubAssetCategory.AssetCategory != null) 
                                    ? q.AssetInfo.ItemName.SubAssetCategory.AssetCategory.MAIN_CAT_CODE : string.Empty,
                    MAIN_CAT_NAME = (q.AssetInfo != null && q.AssetInfo.ItemName != null && q.AssetInfo.ItemName.SubAssetCategory != null && q.AssetInfo.ItemName.SubAssetCategory.AssetCategory != null) 
                                    ? q.AssetInfo.ItemName.SubAssetCategory.AssetCategory.MAIN_CAT_NAME : string.Empty,
                    SUB_CAT_CODE = (q.AssetInfo != null && q.AssetInfo.ItemName != null && q.AssetInfo.ItemName.SubAssetCategory != null) 
                                    ? q.AssetInfo.ItemName.SubAssetCategory.SUB_CAT_CODE : string.Empty,
                    SUB_CAT_NAME = (q.AssetInfo != null && q.AssetInfo.ItemName != null && q.AssetInfo.ItemName.SubAssetCategory != null) 
                                    ? q.AssetInfo.ItemName.SubAssetCategory.SUB_CAT_NAME : string.Empty,
                    IN = (q.AssetInfo != null && q.AssetInfo.ItemName != null) ? q.AssetInfo.ItemName.IN_NAME : string.Empty, // 對接重命名後的 IN_NAME
                    MODEL = q.AssetInfo != null ? q.AssetInfo.MODEL : string.Empty,
                    BRAND = q.AssetInfo != null ? q.AssetInfo.BRAND : string.Empty,
                    ASSET_UNIT = (q.AssetInfo != null && q.AssetInfo.AssetUnit != null) ? q.AssetInfo.AssetUnit.ASSET_UNIT : string.Empty,
                    ASSET_UNIT_CODE = (q.AssetInfo != null && q.AssetInfo.AssetUnit != null) ? q.AssetInfo.AssetUnit.ASSET_UNIT_CODE : string.Empty,
                    ROOM_NAME = (q.StorageBin != null && q.StorageBin.StoreRoom != null) ? q.StorageBin.StoreRoom.ROOM_NAME : string.Empty,
                    BIN_CODE = q.StorageBin != null ? q.StorageBin.BIN_CODE : string.Empty
                })
                .ToListAsync();

            // 準備篩選器所需的下拉式選單數據
            ViewBag.MainCategories = new SelectList(await _context.AssetCategories
                .Where(c => !c.IsDisabled)
                .OrderBy(c => c.MAIN_CAT_CODE)
                .Select(c => new { Value = c.MAIN_CAT_CODE, Text = $"[{c.MAIN_CAT_CODE}] {c.MAIN_CAT_NAME}" })
                .ToListAsync(), "Value", "Text", mainFilter);

            var subQuery = _context.SubAssetCategories.Where(s => !s.IsDisabled);
            if (!string.IsNullOrEmpty(mainFilter))
            {
                subQuery = subQuery.Where(s => s.AssetCategory != null && s.AssetCategory.MAIN_CAT_CODE == mainFilter);
            }
            ViewBag.SubCategories = new SelectList(await subQuery
                .OrderBy(s => s.SUB_CAT_CODE)
                .Select(s => new { Value = s.SUB_CAT_CODE, Text = $"[{s.SUB_CAT_CODE}] {s.SUB_CAT_NAME}" })
                .ToListAsync(), "Value", "Text", subFilter);

            ViewBag.StoreRooms = new SelectList(await _context.StoreRooms
                .Where(r => !r.IsDisabled)
                .OrderBy(r => r.ROOM_NAME)
                .ToListAsync(), "ROOM_NAME", "ROOM_NAME", roomFilter);

            return View("Index", results);
        }
    }
}
