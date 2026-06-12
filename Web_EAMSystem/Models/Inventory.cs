using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產庫存表，記錄特定資產在特定儲位上的庫存數量
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// 庫存紀錄唯一識別碼
        /// </summary>
        [Key]
        public Guid INVENTORY_ID { get; set; }

        /// <summary>
        /// 關聯的資產資訊唯一識別碼
        /// </summary>
        public Guid ASSET_ID { get; set; }

        /// <summary>
        /// 關聯的資產資訊導覽屬性
        /// </summary>
        [ForeignKey("ASSET_ID")]
        public virtual AssetInfo? AssetInfo { get; set; }

        /// <summary>
        /// 關聯的儲位唯一識別碼
        /// </summary>
        public Guid BIN_ID { get; set; }

        /// <summary>
        /// 關聯的儲位導覽屬性
        /// </summary>
        [ForeignKey("BIN_ID")]
        public virtual StorageBin? StorageBin { get; set; }

        /// <summary>
        /// 庫存數量，必須大於或等於零
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量不能為負數")]
        public int QTY { get; set; }

        /// <summary>
        /// 最後盤點或異動日期
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
