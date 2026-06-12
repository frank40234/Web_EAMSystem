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
    /// 資產次類（子類別）控制器，負責資產次類的建立、查詢、修改與啟用/停用
    /// </summary>
    public class SubAssetCategoryController : BaseController
    {
        /// <summary>
        /// 初始化資產次類控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public SubAssetCategoryController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 次類清單列表與模糊查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（SUB_CAT_NAME、SUB_CAT_CODE、Creator）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>次類清單檢視</returns>
        [HttpGet]
        public IActionResult SubCategoryIndex(string searchBy, string keyword, string statusFilter)
        {
            var query = _context.SubAssetCategories.Include(c => c.AssetCategory).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "SUB_CAT_NAME")
                {
                    query = query.Where(c => c.SUB_CAT_NAME.Contains(keyword));
                }
                else if (searchBy == "SUB_CAT_CODE")
                {
                    query = query.Where(c => c.SUB_CAT_CODE.Contains(keyword));
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
            else if (statusFilter == "Disabled")
            {
                query = query.Where(c => c.IsDisabled == true);
            }

            var categories = query.OrderBy(c => c.IsDisabled)
                                  .ThenByDescending(c => c.CreatedDate)
                                  .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;
            return View("SubCategoryIndex", categories);
        }

        /// <summary>
        /// 載入新增次類頁面
        /// </summary>
        /// <returns>新增次類檢視</returns>
        [HttpGet]
        public IActionResult SubCategoryCreate()
        {
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            return View("SubCategoryCreate");
        }

        /// <summary>
        /// 接收表單並儲存新次類
        /// </summary>
        /// <param name="subCategory">次類實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubCategoryCreate(SubAssetCategory subCategory)
        {
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            var currentUser = GetCurrentUser();

            bool isDuplicate = _context.SubAssetCategories.Any(c =>
                c.MAIN_CAT_ID == subCategory.MAIN_CAT_ID &&
                (c.SUB_CAT_CODE == subCategory.SUB_CAT_CODE || c.SUB_CAT_NAME == subCategory.SUB_CAT_NAME));

            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的次類代號或次類名稱。";
                return View(subCategory);
            }

            ModelState.Remove("SUB_CAT_ID");
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
                    subCategory.SUB_CAT_ID = Guid.NewGuid();
                    subCategory.CreatorId = currentUser.UserId;
                    subCategory.Creator = currentUser.UserName;
                    subCategory.ModifierId = currentUser.UserId;
                    subCategory.Modifier = currentUser.UserName;
                    subCategory.IsDisabled = false;

                    _context.SubAssetCategories.Add(subCategory);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "資料類別已成功新增。"; // 修正殘留的「大類」字樣
                    return RedirectToAction(nameof(SubCategoryCreate));
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

            return View(subCategory);
        }

        /// <summary>
        /// 載入次類編輯頁面
        /// </summary>
        /// <param name="id">次類識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult SubCategoryEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound(); // 修正無效 null 檢查

            var category = _context.SubAssetCategories.Find(id);
            if (category == null) return NotFound();

            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

            return View("SubCategoryEdit", category);
        }

        /// <summary>
        /// 接收表單並更新次類資料
        /// </summary>
        /// <param name="id">次類識別碼</param>
        /// <param name="category">要更新的次類實體</param>
        /// <returns>重新導向或原編輯檢視</returns>
        [HttpPost]
        public IActionResult SubCategoryEdit(Guid id, SubAssetCategory category)
        {
            var Categories = _context.AssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    MAIN_CAT_ID = c.MAIN_CAT_ID,
                    DisplayText = c.MAIN_CAT_CODE + " - " + c.MAIN_CAT_NAME
                })
                .ToList();

            ViewBag.CategoryList = new SelectList(Categories, "MAIN_CAT_ID", "DisplayText");

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
                    bool isDuplicate = _context.SubAssetCategories.Any(c =>
                        c.SUB_CAT_ID != id && c.MAIN_CAT_ID == category.MAIN_CAT_ID &&
                        (c.SUB_CAT_CODE == category.SUB_CAT_CODE || c.SUB_CAT_NAME == category.SUB_CAT_NAME));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的類別資料。";
                        return View("SubCategoryEdit", category);
                    }

                    var existingCategory = _context.SubAssetCategories.Find(id);
                    if (existingCategory != null)
                    {
                        existingCategory.MAIN_CAT_ID = category.MAIN_CAT_ID;
                        existingCategory.SUB_CAT_NAME = category.SUB_CAT_NAME;
                        existingCategory.SUB_CAT_CODE = category.SUB_CAT_CODE;
                        existingCategory.ModifierId = currentUser.UserId;
                        existingCategory.Modifier = currentUser.UserName;

                        _context.Update(existingCategory);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(SubCategoryEdit), new { id }); 
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
        /// 停用次類別
        /// </summary>
        /// <param name="id">次類識別碼</param>
        /// <returns>重新導向至次類清單</returns>
        [HttpGet]
        public IActionResult SubCategoryDisable(Guid id)
        {
            var category = _context.SubAssetCategories.Find(id);

            if (category == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                category.IsDisabled = true;
                category.ModifierId = currentUser.UserId;
                category.Modifier = currentUser.UserName;

                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"類別 [{category.SUB_CAT_NAME}] 已成功停用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(SubCategoryIndex));
        }

        /// <summary>
        /// 啟用次類別
        /// </summary>
        /// <param name="id">次類識別碼</param>
        /// <returns>重新導向至次類清單</returns>
        [HttpGet]
        public IActionResult SubCategoryEnable(Guid id)
        {
            var category = _context.SubAssetCategories.Find(id);
            if (category == null) return NotFound();

            try
            {
                category.IsDisabled = false;
                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"類別 [{category.SUB_CAT_NAME}] 已成功恢復啟用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(SubCategoryIndex));
        }
    }
}
