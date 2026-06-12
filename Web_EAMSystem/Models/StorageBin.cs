using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 儲位實體，隸屬於特定資材室，代表具體存放貨架位置（如 A區_1-1）
    /// </summary>
    public class StorageBin : BaseEntity
    {
        /// <summary>
        /// 儲位唯一識別碼
        /// </summary>
        [Key]
        public Guid BIN_ID { get; set; }

        /// <summary>
        /// 所屬的資材室唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "請選擇所屬資材室")]
        public Guid ROOM_ID { get; set; }

        /// <summary>
        /// 所屬的資材室導覽屬性
        /// </summary>
        [ForeignKey("ROOM_ID")]
        public virtual StoreRoom? StoreRoom { get; set; }

        /// <summary>
        /// 儲位編碼，例如：A區_1-1
        /// </summary>
        [Required(ErrorMessage = "請輸入儲位代號")]
        [MaxLength(20, ErrorMessage = "儲位代號長度不能超過 20 個字元")]
        public string BIN_CODE { get; set; } = string.Empty;
    }
}
