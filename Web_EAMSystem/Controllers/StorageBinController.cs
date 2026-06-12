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
    /// 儲位管理控制器，負責具體儲位（例如 A區_1-1）的建立、查詢與啟用/停用
    /// </summary>
    public class StorageBinController : BaseController
    {
        /// <summary>
        /// 初始化儲位管理控制器
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public StorageBinController(ApplicationDbContext context) : base(context)
        {
        }

        // ==========================================
        // 1. 儲位清單 (Index)
        // ==========================================
        /// <summary>
        /// 儲位清單列表與模糊查詢
        /// </summary>
        /// <param name="roomFilter">資材室唯一識別碼過濾</param>
        /// <param name="keyword">儲位代碼關鍵字</param>
        /// <param name="statusFilter">停用狀態篩選（Active、Disabled）</param>
        /// <returns>儲位清單檢視</returns>
        [HttpGet]
        public IActionResult StorageBinIndex(Guid? roomFilter, string keyword, string statusFilter)
        {
            var query = _context.StorageBins.Include(b => b.StoreRoom).AsQueryable();

            if (roomFilter.HasValue && roomFilter.Value != Guid.Empty)
            {
                query = query.Where(b => b.ROOM_ID == roomFilter.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(b => b.BIN_CODE.Contains(keyword));
            }

            if (statusFilter == "Active")
                query = query.Where(b => b.IsDisabled == false);
            else if (statusFilter == "Disabled")
                query = query.Where(b => b.IsDisabled == true);

            var bins = query.OrderBy(b => b.IsDisabled)
                            .ThenByDescending(b => b.CreatedDate)
                            .ToList();

            var rooms = _context.StoreRooms.ToList();
            ViewBag.RoomList = new SelectList(rooms, "ROOM_ID", "ROOM_NAME", roomFilter);

            ViewBag.CurrentRoomFilter = roomFilter;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("StorageBinIndex", bins);
        }

        // ==========================================
        // 2. 新增儲位 (Create - 載入畫面)
        // ==========================================
        /// <summary>
        /// 載入新增儲位頁面
        /// </summary>
        /// <returns>新增儲位檢視</returns>
        [HttpGet]
        public IActionResult StorageBinCreate()
        {
            var rooms = _context.StoreRooms.Where(r => r.IsDisabled == false).ToList();
            ViewBag.RoomList = new SelectList(rooms, "ROOM_ID", "ROOM_NAME");

            return View("StorageBinCreate");
        }

        // ==========================================
        // 3. 新增儲位 (Create - 接收並處理資料)
        // ==========================================
        /// <summary>
        /// 接收表單並儲存新儲位，儲位代號將自動與所屬資材室名稱組合
        /// </summary>
        /// <param name="storageBin">儲位實體</param>
        /// <returns>重新導向或原檢視</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StorageBinCreate(StorageBin storageBin)
        {
            var currentUser = GetCurrentUser();

            ModelState.Remove("BIN_ID");
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
                    // 1. 去資料庫把使用者選的「資材室」找出來
                    var selectedRoom = _context.StoreRooms.Find(storageBin.ROOM_ID);
                    if (selectedRoom == null)
                    {
                        throw new Exception("找不到對應的資材室資料！");
                    }

                    // 2. 將畫面上輸入的號碼 (例如: 1-1)，加上資材室名稱 (例如: A) -> A_1-1
                    string finalBinCode = $"{selectedRoom.ROOM_NAME}_{storageBin.BIN_CODE}";

                    // 3. 防呆機制：檢查這個組合出來的 BIN_CODE 是否已經存在？
                    bool isDuplicate = _context.StorageBins.Any(b => b.BIN_CODE == finalBinCode);
                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = $"新增失敗！儲位代號 [{finalBinCode}] 已經存在。";
                        ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
                        return View(storageBin);
                    }

                    storageBin.BIN_CODE = finalBinCode;
                    storageBin.BIN_ID = Guid.NewGuid();
                    storageBin.CreatorId = currentUser.UserId;
                    storageBin.Creator = currentUser.UserName;
                    storageBin.ModifierId = currentUser.UserId;
                    storageBin.Modifier = currentUser.UserName;
                    storageBin.IsDisabled = false;

                    _context.StorageBins.Add(storageBin);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = $"儲位建檔成功！完整代號為：{storageBin.BIN_CODE}";
                    return RedirectToAction(nameof(StorageBinCreate));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，存檔失敗：" + (ex.InnerException?.Message ?? ex.Message);
                }
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            }

            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
            return View(storageBin);
        }

        /// <summary>
        /// 停用儲位
        /// </summary>
        /// <param name="id">儲位識別碼</param>
        /// <returns>重新導向至儲位清單</returns>
        [HttpGet]
        public IActionResult StorageBinDisable(Guid id)
        {
            var storageBin = _context.StorageBins.Find(id);

            if (storageBin == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                storageBin.IsDisabled = true;
                storageBin.ModifierId = currentUser.UserId;
                storageBin.Modifier = currentUser.UserName;

                _context.Update(storageBin);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"儲位 [{storageBin.BIN_CODE}] 已成功停用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StorageBinIndex));
        }

        /// <summary>
        /// 啟用儲位
        /// </summary>
        /// <param name="id">儲位識別碼</param>
        /// <returns>重新導向至儲位清單</returns>
        [HttpGet]
        public IActionResult StorageBinEnable(Guid id)
        {
            var storageBin = _context.StorageBins.Find(id);
            if (storageBin == null) return NotFound();

            try
            {
                storageBin.IsDisabled = false;
                _context.Update(storageBin);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"儲位 [{storageBin.BIN_CODE}] 已成功恢復啟用！"; // 修正殘留的「大類」字樣
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StorageBinIndex));
        }
    }
}