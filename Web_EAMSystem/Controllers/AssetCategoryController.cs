using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 資產大類控制器，負責資產大類的建立、查詢、修改與啟用/停用
    /// </summary>
    public class AssetCategoryController : BaseController
    {
        /// <summary>
        /// 初始化資產大類控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public AssetCategoryController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 大類清單列表與模糊查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（MAIN_CAT_NAME、MAIN_CAT_CODE、Creator）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>大類清單檢視</returns>
        [HttpGet]
        public IActionResult CategoryIndex(string searchBy, string keyword, string statusFilter)
        {
            var query = _context.AssetCategories.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "MAIN_CAT_NAME")
                {
                    query = query.Where(c => c.MAIN_CAT_NAME.Contains(keyword));
                }
                else if (searchBy == "MAIN_CAT_CODE")
                {
                    query = query.Where(c => c.MAIN_CAT_CODE.Contains(keyword));
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

            return View("CategoryIndex", categories);
        }

        /// <summary>
        /// 載入新增大類頁面
        /// </summary>
        /// <returns>新增大類檢視</returns>
        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View("CategoryCreate");
        }

        /// <summary>
        /// 接收表單並儲存新大類
        /// </summary>
        /// <param name="category">大類實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CategoryCreate(AssetCategory category)
        {
            bool isDuplicate = _context.AssetCategories.Any(c =>
                c.MAIN_CAT_CODE == category.MAIN_CAT_CODE ||
                c.MAIN_CAT_NAME == category.MAIN_CAT_NAME);

            var currentUser = GetCurrentUser();

            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的大類代號或大類名稱。";
                return View("CategoryCreate", category);
            }

            ModelState.Remove("MAIN_CAT_ID");
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
                    category.MAIN_CAT_ID = Guid.NewGuid();
                    category.CreatorId = currentUser.UserId;
                    category.Creator = currentUser.UserName;
                    category.ModifierId = currentUser.UserId;
                    category.Modifier = currentUser.UserName;
                    category.IsDisabled = false;

                    _context.AssetCategories.Add(category);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "資料大類已成功新增。";
                    return RedirectToAction(nameof(CategoryCreate));
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

            return View(category);
        }

        /// <summary>
        /// 載入大類編輯頁面
        /// </summary>
        /// <param name="id">大類識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult CategoryEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound(); // 修正無效 null 檢查

            var category = _context.AssetCategories.Find(id);
            if (category == null) return NotFound();
            
            return View("CategoryEdit", category);
        }

        /// <summary>
        /// 接收表單並更新大類資料
        /// </summary>
        /// <param name="id">大類識別碼</param>
        /// <param name="category">要更新的大類實體</param>
        /// <returns>重新導向或原編輯檢視</returns>
        [HttpPost]
        public IActionResult CategoryEdit(Guid id, AssetCategory category)
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
                    bool isDuplicate = _context.AssetCategories.Any(c =>
                        c.MAIN_CAT_ID != id &&
                        (c.MAIN_CAT_CODE == category.MAIN_CAT_CODE || c.MAIN_CAT_NAME == category.MAIN_CAT_NAME));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的大類代號或名稱。";
                        return View("CategoryEdit", category);
                    }

                    var existingCategory = _context.AssetCategories.Find(id);
                    if (existingCategory != null)
                    {
                        existingCategory.MAIN_CAT_NAME = category.MAIN_CAT_NAME;
                        existingCategory.MAIN_CAT_CODE = category.MAIN_CAT_CODE;
                        existingCategory.ModifierId = currentUser.UserId;
                        existingCategory.Modifier = currentUser.UserName;

                        _context.Update(existingCategory);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";
                        return RedirectToAction(nameof(CategoryEdit), new { id });
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

            return View("CategoryEdit", category);
        }

        /// <summary>
        /// 停用主要大類
        /// </summary>
        /// <param name="id">主要大類識別碼</param>
        /// <returns>重新導向至大類清單</returns>
        [HttpGet]
        public IActionResult CategoryDisable(Guid id)
        {
            var category = _context.AssetCategories.Find(id);

            if (category == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                category.IsDisabled = true;
                category.ModifierId = currentUser.UserId;
                category.Modifier = currentUser.UserName;

                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{category.MAIN_CAT_NAME}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(CategoryIndex));
        }
        
        /// <summary>
        /// 啟用主要大類
        /// </summary>
        /// <param name="id">主要大類識別碼</param>
        /// <returns>重新導向至大類清單</returns>
        [HttpGet]
        public IActionResult CategoryEnable(Guid id)
        {
            var category = _context.AssetCategories.Find(id);
            if (category == null) return NotFound();

            try
            {
                category.IsDisabled = false;
                _context.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{category.MAIN_CAT_NAME}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(CategoryIndex));
        }
    }
}
