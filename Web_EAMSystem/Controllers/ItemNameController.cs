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
    /// 資產品名控制器，負責資產品名主檔的建立、查詢、修改與啟用/停用
    /// </summary>
    public class ItemNameController : BaseController
    {
        /// <summary>
        /// 初始化資產品名控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public ItemNameController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 品名清單列表與模糊查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（IN_NAME、IN_CODE、Creator、SUB_CAT_CODE）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>品名清單檢視</returns>
        [HttpGet]
        public IActionResult ItemNameIndex(string searchBy, string keyword, string statusFilter)
        {
            //建立基礎查詢草稿
            var query = _context.ItemNames.Include(c => c.SubAssetCategory).AsQueryable();

            // 如果使用者有輸入關鍵字，我們就根據選擇的欄位加入過濾條件
            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "IN" || searchBy == "IN_NAME")
                {
                    query = query.Where(c => c.IN_NAME.Contains(keyword));
                }
                else if (searchBy == "IN_CODE")
                {
                    query = query.Where(c => c.IN_CODE.Contains(keyword));
                }
                else if (searchBy == "Creator")
                {
                    query = query.Where(c => c.Creator != null && c.Creator.Contains(keyword));
                }
                else if (searchBy == "SUB_CAT_CODE")
                {
                    query = query.Where(c => c.SubAssetCategory != null && c.SubAssetCategory.SUB_CAT_CODE.Contains(keyword));
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

            var itemNameSort = query.OrderBy(c => c.IsDisabled)
                                    .ThenBy(c => c.SubAssetCategory != null ? c.SubAssetCategory.SUB_CAT_CODE : string.Empty)
                                    .ThenBy(c => c.IN_NAME)
                                    .ThenBy(c => c.IN_CODE)
                                    .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("ItemNameIndex", itemNameSort);
        }

        /// <summary>
        /// 載入新增品名頁面
        /// </summary>
        /// <returns>新增品名檢視</returns>
        [HttpGet]
        public IActionResult ItemNameCreate()
        {
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

            return View("ItemNameCreate");
        }

        /// <summary>
        /// 接收表單並儲存新品名
        /// </summary>
        /// <param name="itemName">品名實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ItemNameCreate(ItemName itemName)
        {
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

            var currentUser = GetCurrentUser();

            // 防呆機制：檢查資料庫是否已有重複資料
            bool isDuplicate = _context.ItemNames.Any(c =>
                c.SUB_CAT_ID == itemName.SUB_CAT_ID &&
                (c.IN_CODE == itemName.IN_CODE || c.IN_NAME == itemName.IN_NAME));

            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的品名代號或名稱。";
                return View(itemName);
            }

            ModelState.Remove("IN_ID");
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
                    itemName.IN_ID = Guid.NewGuid();
                    itemName.CreatorId = currentUser.UserId;
                    itemName.Creator = currentUser.UserName;
                    itemName.ModifierId = currentUser.UserId;
                    itemName.Modifier = currentUser.UserName;
                    itemName.IsDisabled = false;

                    _context.ItemNames.Add(itemName);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "資料品名已成功新增。"; // 修正殘留的「大類」字樣
                    return RedirectToAction(nameof(ItemNameCreate));
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

            return View(itemName);
        }

        /// <summary>
        /// 載入品名編輯頁面
        /// </summary>
        /// <param name="id">品名識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult ItemNameEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound(); // 修正無效 null 檢查為 Guid.Empty

            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

            // 於資料庫查詢此筆資料
            var itemName = _context.ItemNames.Find(id);
            if (itemName == null) return NotFound();

            return View("ItemNameEdit", itemName);
        }

        /// <summary>
        /// 接收表單並更新品名資料
        /// </summary>
        /// <param name="id">品名識別碼</param>
        /// <param name="itemName">要更新的品名實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        public IActionResult ItemNameEdit(Guid id, ItemName itemName)
        {
            var subCategories = _context.SubAssetCategories
                .Where(c => c.IsDisabled == false)
                .Select(c => new
                {
                    SUB_CAT_ID = c.SUB_CAT_ID,
                    DisplayText = c.SUB_CAT_CODE + " - " + c.SUB_CAT_NAME
                })
                .ToList();

            ViewBag.SubCategoryList = new SelectList(subCategories, "SUB_CAT_ID", "DisplayText");

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
                    // 檢查是否有「其他筆資料」用了同樣的代號或名稱 (要排除自己)
                    bool isDuplicate = _context.ItemNames.Any(c =>
                        c.IN_ID != id &&
                        c.SUB_CAT_ID == itemName.SUB_CAT_ID &&
                        (c.IN_CODE == itemName.IN_CODE || c.IN_NAME == itemName.IN_NAME));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的品名代號或名稱。";
                        return View("ItemNameEdit", itemName); // 修正轉向 CategoryEdit 的 Bug
                    }

                    var existingItemName = _context.ItemNames.Find(id);
                    if (existingItemName != null)
                    {
                        existingItemName.SUB_CAT_ID = itemName.SUB_CAT_ID;
                        existingItemName.IN_NAME = itemName.IN_NAME;
                        existingItemName.IN_CODE = itemName.IN_CODE;
                        existingItemName.ModifierId = currentUser.UserId;
                        existingItemName.Modifier = currentUser.UserName;

                        _context.Update(existingItemName);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(ItemNameEdit), new { id });
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
        /// 停用品名
        /// </summary>
        /// <param name="id">品名識別碼</param>
        /// <returns>重新導向至品名清單</returns>
        [HttpGet]
        public IActionResult ItemNameDisable(Guid id)
        {
            var itemName = _context.ItemNames.Find(id);

            if (itemName == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                itemName.IsDisabled = true;
                itemName.ModifierId = currentUser.UserId;
                itemName.Modifier = currentUser.UserName;

                _context.Update(itemName);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"品名 [{itemName.IN_NAME}] 已成功停用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(ItemNameIndex));
        }

        /// <summary>
        /// 啟用品名
        /// </summary>
        /// <param name="id">品名識別碼</param>
        /// <returns>重新導向至品名清單</returns>
        [HttpGet]
        public IActionResult ItemNameEnable(Guid id)
        {
            var itemName = _context.ItemNames.Find(id);
            if (itemName == null) return NotFound();

            try
            {
                itemName.IsDisabled = false;
                _context.Update(itemName);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"品名 [{itemName.IN_NAME}] 已成功恢復啟用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(ItemNameIndex));
        }
    }
}
