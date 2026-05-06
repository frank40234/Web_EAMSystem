# Web_EAMSystem 系統架構與功能分析 (Memory Node)

## 1. 專案概覽 (Project Overview)
本專案為一個基於 **C# ASP.NET Core MVC** 框架並搭配 **MS SQL Server** 的企業資產管理系統（Web_EAMSystem）。系統主要用於管理公司內部的資產分類、資產項目、庫存狀況以及資材室與儲位等相關資料。專案採用標準的 MVC 結構，結合 Entity Framework Core (EF Core) 進行資料庫操作。

## 2. 系統架構 (System Architecture)
- **前端 (Front-End)**: Razor Views (`.cshtml`), Bootstrap, jQuery, 以及 jQuery Validation。
- **後端 (Back-End)**: ASP.NET Core MVC, C#。
- **資料存取層 (Data Access)**: Entity Framework Core (`ApplicationDbContext.cs`)，以 Code-First 的方式建立與更新資料庫（包含 Migrations）。
- **資料庫 (Database)**: Microsoft SQL Server。
- **身分驗證 (Authentication)**: 採用基於 Cookie 的身分驗證 (Cookie Authentication)，具備權限檢查 (FallbackPolicy RequireAuthenticatedUser)。

## 3. 資料模型與功能關聯 (Data Models & Entity Relationships)

系統的核心設計以資產的分類層級和儲存位置為主。

### 3.1 資產分類與定義
資產的定義採用層級式架構（大類 -> 次類 -> 品名 -> 具體資產）。
* **AssetCategory (大類)**: 定義資產的最高層級類別 (例如：IT設備)，具備代號 (`MAIN_CAT_CODE`) 與名稱。
* **SubAssetCategory (次類)**: 隸屬於大類 (透過 `MAIN_CAT_ID` 關聯) (例如：電腦設備)，具備代號 (`SUB_CAT_CODE`) 與名稱。
* **ItemName (品名)**: 隸屬於次類 (透過 `SUB_CAT_ID` 關聯) (例如：筆記型電腦)，具備代號 (`IN_CODE`) 與名稱。
* **AssetUnit (單位)**: 系統共用的單位表 (例如：台、個、套)，具備單位代號與名稱。
* **AssetInfo (資產資訊)**: 資產主檔，定義具體的設備屬性。關聯到 `ItemName`、`AssetUnit` 以及預設存放的 `StorageBin` (`BIN_ID`)，並包含物料編碼 (`ASSET_CODE` 自動產生)、型號 (`MODEL`)、廠牌 (`BRAND`) 等資訊。

### 3.2 倉儲與儲位管理
* **StoreRoom (資材室)**: 定義實體的存放房間或大區域 (例如：A區)，具備代號或名稱。
* **StorageBin (儲位)**: 隸屬於資材室 (透過 `ROOM_ID` 關聯)，代表具體的存放位置 (例如：A-01)。

### 3.3 庫存管理
* **Inventory (庫存)**: 將具體的資產項目存放在對應的儲位上。透過 `ASSET_ID` 關聯資產，並透過 `BIN_ID` 關聯儲位，紀錄數量 (`QTY`) 以及最後異動時間。

### 3.4 系統共用屬性 (Audit Fields)
大部分的主要實體 (Models) 皆具備以下稽核與狀態欄位，以追蹤資料異動：
* `Creator`, `CreatorId`, `CreatedDate`: 建立者與建立時間。
* `Modifier`, `ModifierId`, `ModifiedDate`: 修改者與修改時間。
* `IsDisabled`: 邏輯刪除或停用狀態標記 (True代表停用，False代表啟用)。

## 4. 控制器功能分析 (Controllers)
各 Controller 分別對應上述 Model 的 CRUD (Create, Read, Update, Delete) 操作，負責接收前端請求、調用資料庫上下文 (DbContext) 並返回對應的 View 頁面：
1. **HomeController**: 負責首頁與系統進入點，及錯誤頁面處理。
2. **AuthController**: 處理使用者登入、登出、存取拒絕等身分驗證邏輯。
3. **AssetCategoryController**: 資產大類的管理與維護。
4. **SubAssetCategoryController**: 資產次類的管理與維護。
5. **ItemNameController**: 資產品名的管理與維護。
6. **AssetUnitController**: 資產單位的管理與維護。
7. **AssetInFoController**: 資產基本資訊與編碼產生的管理。
8. **StoreRoomController**: 資材室的管理。
9. **StorageBinController**: 儲位與資材室關聯的管理。
10. **InventoryController** (經由 `Inventory` 視圖結構推論): 顯示或管理資產的庫存狀況。

## 5. 開發與教學規範 (Development & Mentoring Guidelines)
根據專案 `GEMINI.md` 配置的規範：
* **教學定位**: 扮演資深 Web 開發導師，協助初學者構建此系統。
* **教學流程**:
  1. 解釋理論與目的。
  2. 提供 C# 語法、HTML/CSS 或 SQL 的相關規則與最佳實踐。
  3. 提供程式碼範例並明確說明放置位置 (不可直接代為修改檔案)。
  4. 每次對話專注處理一個具體功能，避免資訊過載。
* **重點關注**: 逐步指導，包含環境配置、資料庫連線 (EF Core)、認證 (Auth) 以及後續的 CRUD 開發，並會要求使用者運行驗證並回報錯誤以進行除錯指導。