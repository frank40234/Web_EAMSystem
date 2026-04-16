using System;

namespace Web_EAMSystem.Models
{
    public class InventoryIndexViewModel
    {
        // 資材編號 (來自 AssetInfo)
        public string? ASSET_CODE { get; set; }

        // 庫存數量 (來自 Inventory)
        public int QTY { get; set; }

        // 大類代號與名稱 (來自 AssetCategory)
        public string MAIN_CAT_CODE { get; set; } = string.Empty;
        public string MAIN_CAT_NAME { get; set; } = string.Empty;

        // 類別代號與名稱 (來自 SubAssetCategory)
        public string SUB_CAT_CODE { get; set; } = string.Empty;
        public string SUB_CAT_NAME { get; set; } = string.Empty;

        // 品名 (來自 ItemName)
        public string IN { get; set; } = string.Empty;

        // 型號與廠牌 (來自 AssetInfo)
        public string? MODEL { get; set; }
        public string? BRAND { get; set; }

        // 單位與單位代號 (來自 AssetUnit)
        public string ASSET_UNIT { get; set; } = string.Empty;
        public string ASSET_UNIT_CODE { get; set; } = string.Empty;

        // 資材室與儲位 (來自 StoreRoom 與 StorageBin)
        public string ROOM_NAME { get; set; } = string.Empty;
        public string BIN_CODE { get; set; } = string.Empty;
    }
}
