using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web_EAMSystem.Data;
using Web_EAMSystem.Models;


namespace Web_EAMSystem.Controllers
{
    public class StoreRoomController : Controller
    {
        // 宣告一個私有變數來存放「資料庫總管」
        private readonly ApplicationDbContext _context;
        //自動設定好DBContext
        public StoreRoomController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 大類清單
        /// </summary>
        /// <param name="searchBy"></param>
        /// <param name="keyword"></param>
        /// <param name="statusFilter"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StoreRoomIndex(string searchBy, string keyword, string statusFilter)
        {
            
            //建立基礎查詢草稿
            var query = _context.StoreRooms.AsQueryable();

            // 如果使用者有輸入關鍵字，我們就根據選擇的欄位加入過濾條件
            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchBy == "ROOM_NAME")
                {
                    // Contains 在 SQL 裡會被翻譯成 LIKE '%關鍵字%'
                    query = query.Where(c => c.ROOM_NAME.Contains(keyword));
                }
                else if (searchBy == "Creator")
                {
                    query = query.Where(c => c.Creator.Contains(keyword));
                }
            }
            if (statusFilter == "Active")
            {
                query = query.Where(c => c.IsDisabled == false); // 只找使用中
            }
            else if (statusFilter == "Disabled")
            {
                query = query.Where(c => c.IsDisabled == true);  // 只找已停用
            }

            // 3. 新增：多重排序 (先按狀態排，再按日期排)
            // OrderBy(IsDisabled)：false(0/使用中) 會排在 true(1/已停用) 的前面！
            var roomSort = query.OrderBy(c => c.IsDisabled)
                                  .ThenBy(c => c.ROOM_NAME)
                                  .ToList();

            // 4.利用 ViewBag 把剛剛搜尋的條件存起來傳回畫面
            // 這樣使用者搜完之後，下拉選單跟輸入框才不會變回空白
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStatusFilter = statusFilter; // 記錄目前的狀態篩選

            return View("StoreRoomIndex", roomSort);
        }

        /// <summary>
        /// 新增大類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StoreRoomCreate()
        {
            
            return View("StoreRoomCreate");
        }
        // 2. POST: 負責接收使用者填寫的資料，並存入資料庫
        [HttpPost]
        [ValidateAntiForgeryToken] // 資安防護：防止跨站請求偽造 (CSRF) 攻擊
        public IActionResult StoreRoomCreate(StoreRoom storeRoom)
        {

            // 防呆機制：檢查資料庫是否已有重複資料
            // ==========================================
            bool isDuplicate = _context.StoreRooms.Any(c =>
                c.ROOM_NAME == storeRoom.ROOM_NAME );

            var currentUser = GetCurrentUser();


            if (isDuplicate)
            {
                // 如果發現重複，設定錯誤提示
                TempData["ErrorMessage"] = "添加失敗！資料庫中已存在相同的大類代號或大類名稱。";

                // 直接退回新增畫面，並把使用者填到一半的 category 傳回去
                return View("StoreRoomCreate", storeRoom);
            }

            //  關鍵新增：在驗證之前，手動排除那些不需要使用者從畫面上輸入的欄位！
            ModelState.Remove("ROOM_ID");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("ModifierId");
            ModelState.Remove("Modifier");      // 告訴系統：不要檢查 Modifier！
            ModelState.Remove("ModifiedDate");  // 告訴系統：不要檢查 ModifiedDate！

            // 排除完畢後，再進行驗證
            if (ModelState.IsValid)
            {
                // 使用 try-catch 保護資料庫存檔過程
                try
                {
                    storeRoom.ROOM_ID = Guid.NewGuid();
                    storeRoom.CreatorId = currentUser.UserId;
                    storeRoom.Creator = currentUser.UserName;
                    storeRoom.ModifierId = currentUser.UserId;
                    storeRoom.Modifier = currentUser.UserName;
                    storeRoom.CreatedDate = DateTime.Now;
                    storeRoom.Modifier = storeRoom.Creator;         // 讓異動者等於建檔者
                    storeRoom.ModifiedDate = DateTime.Now;         // 讓異動時間等於現在
                    storeRoom.IsDisabled = false;

                    _context.StoreRooms.Add(storeRoom);
                    _context.SaveChanges();

                    // 存檔成功，設定成功訊息
                    TempData["SuccessMessage"] = "資料大類已成功新增。";

                    return RedirectToAction(nameof(StoreRoomCreate));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "系統發生錯誤，存檔失敗：" + ex.Message;
                }
            }
            else
            {

                var errors = string.Join("; ", ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage));

                TempData["ErrorMessage"] = "資料格式有誤：" + errors;
            }

            return View(storeRoom);
        }

        /// <summary>
        /// 大類編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StoreRoomEdit(Guid id)
        {
            
            if (id == null) return NotFound();

            //於資料庫查詢此筆資料
            var storeRoom = _context.StoreRooms.Find(id);
            if (storeRoom == null) return NotFound();

            return View("StoreRoomEdit", storeRoom);
        }
        [HttpPost]
        public IActionResult StoreRoomEdit(Guid id, StoreRoom storeRoom)
        {
            
            var currentUser = GetCurrentUser();
            // 排除不需要驗證的欄位 (因為這些是系統產生的或是舊資料)
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
                    //  檢查是否有「其他筆資料」用了同樣的代號或名稱 (要排除自己)
                    bool isDuplicate = _context.StoreRooms.Any(c =>
                        c.ROOM_ID != id &&
                        (c.ROOM_NAME == storeRoom.ROOM_NAME ));

                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = "修改失敗！資料庫中已存在相同的大類代號或名稱。";
                        return View("StoreRoomEdit", storeRoom);
                    }

                    //  標準更新流程：先從資料庫拿出舊包裹，再把新東西塞進去
                    var existingStoreRoom = _context.StoreRooms.Find(id);
                    if (existingStoreRoom != null)
                    {
                        existingStoreRoom.ROOM_NAME = storeRoom.ROOM_NAME;
                        existingStoreRoom.ModifierId = currentUser.UserId; // 畫面上填寫的異動者，之後改為登入者
                        existingStoreRoom.Modifier = currentUser.UserName; // 畫面上填寫的異動者，之後改為登入者
                        existingStoreRoom.ModifiedDate = DateTime.Now;  // 系統押上最新修改時間

                        _context.Update(existingStoreRoom);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "資料修改成功！";

                        return RedirectToAction(nameof(StoreRoomEdit)); // 修改完，自動跳回列表頁！
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
        /// 停用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StoreRoomDisable(Guid id)
        {
            
            // 1. 去資料庫把這筆資料找出來
            var storeRoom = _context.StoreRooms.Find(id);

            if (storeRoom == null) return NotFound();
            var currentUser = GetCurrentUser();

            try
            {
                // 2. 執行軟刪除：把停用標記設為 true
                storeRoom.IsDisabled = true;
                storeRoom.ModifierId = currentUser.UserId;
                storeRoom.Modifier = currentUser.UserName;

                // 💡 實務細節：停用也算是一種「異動」，所以我們要更新異動時間
                storeRoom.ModifiedDate = DateTime.Now;
                // (因為這裡沒有表單可以讓使用者輸入名字，異動者就維持上一次的人，或是你可以寫死成 "System")

                // 3. 存檔
                _context.Update(storeRoom);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{storeRoom.ROOM_NAME}] 已成功停用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，停用失敗：" + ex.Message;
            }

            // 4. 完成後，跳回列表頁
            return RedirectToAction(nameof(StoreRoomIndex));
        }

        /// <summary>
        /// 啟用大類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult StoreRoomEnable(Guid id)
        {
            
            var storeRoom = _context.StoreRooms.Find(id);
            if (storeRoom == null) return NotFound();

            try
            {
                storeRoom.IsDisabled = false; // 改為啟用
                storeRoom.ModifiedDate = DateTime.Now;

                _context.Update(storeRoom);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"大類 [{storeRoom.ROOM_NAME}] 已成功恢復啟用！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "系統發生錯誤，啟用失敗：" + ex.Message;
            }

            return RedirectToAction(nameof(StoreRoomIndex));
        }

        private (string UserId, string UserName) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            return (userId, userName);
        }
    }
}
