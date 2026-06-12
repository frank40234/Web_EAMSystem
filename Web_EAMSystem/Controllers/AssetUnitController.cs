using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 資產計量單位控制器，負責單位的建立、查詢、修改與啟用/停用
    /// </summary>
    public class AssetUnitController : BaseController
    {
        /// <summary>
        /// 初始化資產計量單位控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public AssetUnitController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 單位清單列表與查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（ASSET_UNIT_CODE、ASSET_UNIT、Creator）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>單位清單檢視</returns>
        [HttpGet]
        public IActionResult UnitIndex(string searchBy, string keyword, string statusFilter)
        {
            var query = _context.AssetUnits.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "ASSET_UNIT_CODE")
                {
                    query = query.Where(c => c.ASSET_UNIT_CODE.Contains(keyword));
                }
                else if (searchBy == "ASSET_UNIT")
                {
                    query = query.Where(c => c.ASSET_UNIT.Contains(keyword));
                }
                else if (searchBy == "Creator")
                {
                    query = query.Where(c => c.Creator != null && c.Creator.Contains(keyword));
                }
            }
            if (statusFilter == "Active")
            {
                query = query.Where(c => c.IsDisabled == false);
            }
            else if (statusFilter == "Disable" || statusFilter == "Disabled")
            {
                query = query.Where(c => c.IsDisabled == true);
            }

            var unitSort = query.OrderBy(c => c.IsDisabled)
                .ThenByDescending(c => c.CreatedDate)
                .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("UnitIndex", unitSort);
        }

        /// <summary>
        /// 載入新增單位頁面
        /// </summary>
        /// <returns>新增單位檢視</returns>
        [HttpGet]
        public IActionResult UnitCreate()
        {
            return View("UnitCreate");
        }

        /// <summary>
        /// 接收表單並儲存新單位資料
        /// </summary>
        /// <param name="assetUnit">計量單位實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UnitCreate(AssetUnit assetUnit)
        {
            bool isDuplicate = _context.AssetUnits.Any(c =>
                c.ASSET_UNIT == assetUnit.ASSET_UNIT ||
                c.ASSET_UNIT_CODE == assetUnit.ASSET_UNIT_CODE);

            var currentUser = GetCurrentUser();

            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的單位代號或單位名稱。"; // 修正殘留的「大類」字樣
                return View("UnitCreate", assetUnit);
            }

            ModelState.Remove("ASSET_UNIT_ID");
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
                    assetUnit.ASSET_UNIT_ID = Guid.NewGuid();
                    assetUnit.CreatorId = currentUser.UserId;
                    assetUnit.Creator = currentUser.UserName;
                    assetUnit.ModifierId = currentUser.UserId;
                    assetUnit.Modifier = currentUser.UserName;
                    assetUnit.IsDisabled = false;

                    _context.AssetUnits.Add(assetUnit);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "資料單位已成功新增。"; // 修正殘留的「大類」字樣
                    return RedirectToAction(nameof(UnitCreate));
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

            return View(assetUnit);
        }

        /// <summary>
        /// 載入單位編輯頁面
        /// </summary>
        /// <param name="id">單位識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult UnitEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound(); // 修正無效 null 檢查

            var unit = _context.AssetUnits.Find(id);
            if (unit == null) return NotFound();
 
            return View("UnitEdit", unit);
        }

        /// <summary>
        /// 接收表單並更新單位資料
        /// </summary>
        /// <param name="id">單位識別碼</param>
        /// <param name="assetUnit">要更新的單位實體</param>
        /// <returns>重新導向或原編輯檢視</returns>
        [HttpPost]
        public IActionResult UnitEdit(Guid id, AssetUnit assetUnit)
        {
            var currentUser = GetCurrentUser();

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
                    bool isDuplicate = _context.AssetUnits.Any(c =>
                        c.ASSET_UNIT_ID != id &&
                        (c.ASSET_UNIT_CODE == assetUnit.ASSET_UNIT_CODE || c.ASSET_UNIT == assetUnit.ASSET_UNIT));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的單位代號或名稱。";
                        return View("UnitEdit", assetUnit);
                    }

                    var existingUnit = _context.AssetUnits.Find(id);
                    if (existingUnit != null)
                    {
                        existingUnit.ASSET_UNIT = assetUnit.ASSET_UNIT;
                        existingUnit.ASSET_UNIT_CODE = assetUnit.ASSET_UNIT_CODE;
                        existingUnit.ModifierId = currentUser.UserId;
                        existingUnit.Modifier = currentUser.UserName;

                        _context.Update(existingUnit);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";
                        return RedirectToAction(nameof(UnitEdit), new { id });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，修改失敗：" + ex.Message;
                }
            }
            return View("UnitEdit", assetUnit);
        }

        /// <summary>
        /// 停用單位
        /// </summary>
        /// <param name="id">單位識別碼</param>
        /// <returns>重新導向至單位清單</returns>
        [HttpGet]
        public IActionResult UnitDisable(Guid id)
        {
            var unit = _context.AssetUnits.Find(id);

            if (unit == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                unit.IsDisabled = true;
                unit.ModifierId = currentUser.UserId;
                unit.Modifier = currentUser.UserName;

                _context.Update(unit);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"單位 [{unit.ASSET_UNIT}] 已成功停用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(UnitIndex));
        }

        /// <summary>
        /// 啟用單位
        /// </summary>
        /// <param name="id">單位識別碼</param>
        /// <returns>重新導向至單位清單</returns>
        [HttpGet]
        public IActionResult UnitEnable(Guid id)
        {
            var unit = _context.AssetUnits.Find(id);
            if (unit == null) return NotFound();

            try
            {
                unit.IsDisabled = false;
                _context.Update(unit);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"單位 [{unit.ASSET_UNIT}] 已成功恢復啟用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(UnitIndex));
        }
    }
}
