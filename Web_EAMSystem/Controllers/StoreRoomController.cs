using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    /// <summary>
    /// 資材室區域控制器，負責資材室的建立、查詢、修改與啟用/停用
    /// </summary>
    public class StoreRoomController : BaseController
    {
        /// <summary>
        /// 初始化資材室區域控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public StoreRoomController(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 資材室清單列表與查詢
        /// </summary>
        /// <param name="searchBy">查詢欄位類型（ROOM_NAME、Creator）</param>
        /// <param name="keyword">查詢關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>資材室清單檢視</returns>
        [HttpGet]
        public IActionResult StoreRoomIndex(string searchBy, string keyword, string statusFilter)
        {
            var query = _context.StoreRooms.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "ROOM_NAME")
                {
                    query = query.Where(c => c.ROOM_NAME.Contains(keyword));
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

            var roomSort = query.OrderBy(c => c.IsDisabled)
                                .ThenBy(c => c.ROOM_NAME)
                                .ToList();

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("StoreRoomIndex", roomSort);
        }

        /// <summary>
        /// 載入新增資材室頁面
        /// </summary>
        /// <returns>新增資材室檢視</returns>
        [HttpGet]
        public IActionResult StoreRoomCreate()
        {
            return View("StoreRoomCreate");
        }

        /// <summary>
        /// 接收表單並儲存新資材室資料
        /// </summary>
        /// <param name="storeRoom">資材室實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StoreRoomCreate(StoreRoom storeRoom)
        {
            bool isDuplicate = _context.StoreRooms.Any(c =>
                c.ROOM_NAME == storeRoom.ROOM_NAME);

            var currentUser = GetCurrentUser();

            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的資材室名稱。"; // 修正殘留的「大類」字樣
                return View("StoreRoomCreate", storeRoom);
            }

            ModelState.Remove("ROOM_ID");
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
                    storeRoom.ROOM_ID = Guid.NewGuid();
                    storeRoom.CreatorId = currentUser.UserId;
                    storeRoom.Creator = currentUser.UserName;
                    storeRoom.ModifierId = currentUser.UserId;
                    storeRoom.Modifier = currentUser.UserName;
                    storeRoom.IsDisabled = false;

                    _context.StoreRooms.Add(storeRoom);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "資料資材室已成功新增。"; // 修正殘留的「大類」字樣
                    return RedirectToAction(nameof(StoreRoomCreate));
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

            return View(storeRoom);
        }

        /// <summary>
        /// 載入資材室編輯頁面
        /// </summary>
        /// <param name="id">資材室識別碼</param>
        /// <returns>編輯檢視</returns>
        [HttpGet]
        public IActionResult StoreRoomEdit(Guid id)
        {
            if (id == Guid.Empty) return NotFound(); // 修正無效 null 檢查

            var storeRoom = _context.StoreRooms.Find(id);
            if (storeRoom == null) return NotFound();

            return View("StoreRoomEdit", storeRoom);
        }

        /// <summary>
        /// 接收表單並更新資材室資料
        /// </summary>
        /// <param name="id">資材室識別碼</param>
        /// <param name="storeRoom">要更新的資材室實體</param>
        /// <returns>重新導向或原編輯檢視</returns>
        [HttpPost]
        public IActionResult StoreRoomEdit(Guid id, StoreRoom storeRoom)
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
                    bool isDuplicate = _context.StoreRooms.Any(c =>
                        c.ROOM_ID != id &&
                        (c.ROOM_NAME == storeRoom.ROOM_NAME));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的資材室名稱。"; // 修正舊有錯誤訊息
                        return View("StoreRoomEdit", storeRoom);
                    }

                    var existingStoreRoom = _context.StoreRooms.Find(id);
                    if (existingStoreRoom != null)
                    {
                        existingStoreRoom.ROOM_NAME = storeRoom.ROOM_NAME;
                        existingStoreRoom.ModifierId = currentUser.UserId;
                        existingStoreRoom.Modifier = currentUser.UserName;

                        _context.Update(existingStoreRoom);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";
                        return RedirectToAction(nameof(StoreRoomEdit), new { id });
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

            return View("StoreRoomEdit", storeRoom);
        }

        /// <summary>
        /// 停用資材室區域
        /// </summary>
        /// <param name="id">資材室識別碼</param>
        /// <returns>重新導向至資材室清單</returns>
        [HttpGet]
        public IActionResult StoreRoomDisable(Guid id)
        {
            var storeRoom = _context.StoreRooms.Find(id);

            if (storeRoom == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                storeRoom.IsDisabled = true;
                storeRoom.ModifierId = currentUser.UserId;
                storeRoom.Modifier = currentUser.UserName;

                _context.Update(storeRoom);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"資材室 [{storeRoom.ROOM_NAME}] 已成功停用！"; // 修正大類字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StoreRoomIndex));
        }

        /// <summary>
        /// 啟用資材室區域
        /// </summary>
        /// <param name="id">資材室識別碼</param>
        /// <returns>重新導向至資材室清單</returns>
        [HttpGet]
        public IActionResult StoreRoomEnable(Guid id)
        {
            var storeRoom = _context.StoreRooms.Find(id);
            if (storeRoom == null) return NotFound();

            try
            {
                storeRoom.IsDisabled = false;
                _context.Update(storeRoom);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"資材室 [{storeRoom.ROOM_NAME}] 已成功恢復啟用！"; // 修正大類字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StoreRoomIndex));
        }
    }
}
