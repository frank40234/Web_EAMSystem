using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Controllers
{
    public class StorageBinController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StorageBinController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. 儲位清單 (Index)
        // ==========================================
        [HttpGet]
        public IActionResult StorageBinIndex(Guid? roomFilter, string keyword, string statusFilter)
        {
            // 🌟 記得 Include 資材室，這樣畫面上才能顯示所屬的資材室名稱
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

            //  新增：準備「資材室」下拉選單給搜尋列使用
            // 搜尋條件通常會把所有資材室都撈出來，即使是停用的，這樣才能查到歷史資料
            var rooms = _context.StoreRooms.ToList();
            // 注意第四個參數 roomFilter，這會讓畫面重新載入時「記住」使用者剛剛選了哪個選項！
            ViewBag.RoomList = new SelectList(rooms, "ROOM_ID", "ROOM_NAME", roomFilter);

            ViewBag.CurrentRoomFilter = roomFilter;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter;

            return View("StorageBinIndex", bins);
        }

        // ==========================================
        // 2. 新增儲位 (Create - 載入畫面)
        // ==========================================
        [HttpGet]
        public IActionResult StorageBinCreate()
        {
            // 準備「資材室」下拉選單 (只抓取未停用的)
            var rooms = _context.StoreRooms.Where(r => r.IsDisabled == false).ToList();
            ViewBag.RoomList = new SelectList(rooms, "ROOM_ID", "ROOM_NAME");

            return View("StorageBinCreate");
        }

        // ==========================================
        // 3. 新增儲位 (Create - 接收並處理資料)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StorageBinCreate(StorageBin storageBin)
        {
            var currentUser = GetCurrentUser();

            // 排除系統共用欄位，避免 ModelState 驗證失敗
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
                    // 🌟 核心魔法：自動組合 BIN_CODE (資材室名稱_庫位號碼)

                    // 1. 去資料庫把使用者選的「資材室」找出來
                    var selectedRoom = _context.StoreRooms.Find(storageBin.ROOM_ID);
                    if (selectedRoom == null)
                    {
                        throw new Exception("找不到對應的資材室資料！");
                    }

                    // 2. 將畫面上輸入的號碼 (例如: 1-1)，加上資材室名稱 (例如: A)
                    // 組合結果：A_1-1
                    string finalBinCode = $"{selectedRoom.ROOM_NAME}_{storageBin.BIN_CODE}";

                    // 3. 防呆機制：檢查這個組合出來的 BIN_CODE 是否已經存在？
                    bool isDuplicate = _context.StorageBins.Any(b => b.BIN_CODE == finalBinCode);
                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = $"新增失敗！儲位代號 [{finalBinCode}] 已經存在。";

                        // 失敗時要重新綁定下拉選單，並退回原畫面
                        ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
                        return View(storageBin);
                    }

                    // 4. 確認沒重複，把組合好的新代號正式塞進去
                    storageBin.BIN_CODE = finalBinCode;

                    // 5. 補齊系統共用欄位並存檔
                    storageBin.BIN_ID = Guid.NewGuid();
                    storageBin.CreatorId = currentUser.UserId;
                    storageBin.Creator = currentUser.UserName;
                    storageBin.ModifierId = currentUser.UserId;
                    storageBin.Modifier = currentUser.UserName;
                    storageBin.CreatedDate = DateTime.Now;
                    storageBin.ModifiedDate = DateTime.Now;
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

            // 發生錯誤退回畫面時，別忘了重新準備下拉選單
            ViewBag.RoomList = new SelectList(_context.StoreRooms.Where(r => r.IsDisabled == false), "ROOM_ID", "ROOM_NAME");
            return View(storageBin);
        }

        /// <summary>
        /// 停用儲位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StorageBinDisable(Guid id)
        {

            // 1. 去資料庫把這筆資料找出來
            var storageBin = _context.StorageBins.Find(id);

            if (storageBin == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                // 2. 執行軟刪除：把停用標記設為 true
                storageBin.IsDisabled = true;
                storageBin.ModifierId = currentUser.UserId;
                storageBin.Modifier = currentUser.UserName;

                // 💡 實務細節：停用也算是一種「異動」，所以我們要更新異動時間
                storageBin.ModifiedDate = DateTime.Now;
                // (因為這裡沒有表單可以讓使用者輸入名字，異動者就維持上一次的人，或是你可以寫死成 "System")

                // 3. 存檔
                _context.Update(storageBin);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{storageBin.BIN_CODE}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            // 4. 完成後，跳回列表頁
            return RedirectToAction(nameof(StorageBinIndex));
        }

        /// <summary>
        /// 啟用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StorageBinEnable(Guid id)
        {

            var storageBin = _context.StorageBins.Find(id);
            if (storageBin == null) return NotFound();

            try
            {
                storageBin.IsDisabled = false; // 改為啟用
                storageBin.ModifiedDate = DateTime.Now;

                _context.Update(storageBin);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{storageBin.BIN_CODE}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StorageBinIndex));
        }

        // ==========================================
        // 取得目前登入者 (共用方法)
        // ==========================================
        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }
    }
}