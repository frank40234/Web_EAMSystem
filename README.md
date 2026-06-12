# Web_EAMSystem (企業資產管理系統)

本系統為一個基於 **ASP.NET Core 8.0 MVC**、**Entity Framework Core** 與 **MS SQL Server** 構建的企業資產管理系統 (Enterprise Asset Management System)。

---

## 🛠️ 專案技術棧
* **後端核心**：ASP.NET Core 8.0 MVC (C#)
* **ORM 框架**：Entity Framework Core (Code-First Migrations)
* **資料庫**：Microsoft SQL Server
* **前端框架**：Razor Views (.cshtml), Bootstrap 5, jQuery
* **驗證與授權**：Cookie Authentication (基於 Cookie 的身分驗證)

---

## 📂 系統架構與資料模型
系統主要圍繞資產的定義分類與空間倉儲位置進行設計：
1. **大類 (AssetCategory)**：IT設備、事務設備等最高分類。
2. **次類 (SubAssetCategory)**：隸屬大類，如電腦設備、投影設備等。
3. **品名 (ItemName)**：隸屬次類，如筆記型電腦、雷射印表機等。
4. **標準單位 (AssetUnit)**：計量單位，如台、個、套。
5. **資產主檔 (AssetInfo)**：具體資產的型號、廠牌與預設存放儲位。新增時系統將自動生成料號，編碼格式為：`{大類代碼}-{次類代碼}-{品名代碼}-{0001~9999}`。
6. **資材室 (StoreRoom)**：實體存放庫房或大區域，如 A區、B區。
7. **儲位 (StorageBin)**：隸屬資材室，如 A區_1-1。
8. **庫存 (Inventory)**：記錄特定資產在特定儲位上的庫存數量，並記錄最後盤動時間。

---

## 🚀 專案重構與 Bugs 修復摘要 (2026-06-12)
專案於今日完成全面重構與重大 Bugs 修復，**成功將編譯警告由 54 個降至 0 個**，主要優化內容如下：

### 1. 核心 Bugs 修正
* **廠牌更新 Bug**：修復 `AssetInFoController` 在編輯 POST 時因 assignment 寫錯導致 `existingAsset.BRAND` 無法更新的問題。
* **資產防重失效 Bug**：修復 `AssetInFoController` 編輯 POST 時恆真式 (`c.UNIT_ID == c.UNIT_ID`) 導致重複資產判斷失效的問題。
* **品名重複 View 找不到 Bug**：修復 `ItemNameController` 在品名重複時導向不存在的 View `"CategoryEdit"` 導致拋出 runtime 異常的問題。
* **無效 Null 檢查警告**：修復多個 Controller 中 `Guid id` 被以 `id == null` 進行檢查的編譯警告，統一修正為 `id == Guid.Empty`。

### 2. 安全性隱患修補
* **JWT 金鑰硬編碼**：修復 `AuthController.cs` 的 `SSOLogin` 中硬編碼 JWT 驗證密鑰的問題，改由 `IConfiguration` 於 `appsettings.json` 動態載入。

### 3. 全新功能補齊
* **庫存清單查詢**：新建 [InventoryController.cs](file:///C:/02Project/GitProject/Web_EAMSystem/Web_EAMSystem/Controllers/InventoryController.cs)，並投影關聯數據至 `InventoryIndexViewModel`，成功對接並啟用先前缺失的庫存篩選與清單查詢功能。

### 4. DRY 架構優化與重複清理
* **BaseEntity 基底類別**：建立 [BaseEntity.cs](file:///C:/02Project/GitProject/Web_EAMSystem/Web_EAMSystem/Models/BaseEntity.cs) 統一管理稽核屬性與狀態（建檔/修改者、時間、停用狀態）。
* **自動稽核時間**：覆寫 `ApplicationDbContext` 中的 `SaveChanges`/`SaveChangesAsync`，實體新增或修改時會自動寫入時間，不需在控制器手動指派。
* **BaseController 基底類別**：建立 [BaseController.cs](file:///C:/02Project/GitProject/Web_EAMSystem/Web_EAMSystem/Controllers/BaseController.cs) 統一管理使用者 Claims 讀取（`GetCurrentUser()`），簡化控制器注入與邏輯。

---

## 🏃 如何啟動與運行專案

### 1. 組態配置
請確認 `Web_EAMSystem/appsettings.json` 或 `appsettings.Development.json` 中配置了正確的資料庫連線字串與 JWT 驗證金鑰：
```json
{
  "ConnectionStrings": {
    "EAM_DBConnection": "Server=YOUR_DB_SERVER;Database=Web_EAMSystem;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
  },
  "JwtSettings": {
    "loginTokenKey": "YOUR_SUPER_SECRET_JWT_KEY_MIN_256_BITS"
  }
}
```

### 2. 資料庫遷移與更新
請於 `Web_EAMSystem` 目錄下執行以下命令以還原並建立資料庫：
```bash
dotnet ef database update
```

### 3. 運行專案
```bash
dotnet run
```
專案啟動後，可使用瀏覽器存取控制台輸出的 URL。如需於開發階段進行登入測試，可直接開啟 `/Auth/GenerateTestToken` 頁面產生測試 JWT Token 並一鍵模擬單點登入。
