using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產資訊實體，定義具體資產的料號、廠牌、型號、預設單位與存放儲位
    /// </summary>
    public class AssetInfo : BaseEntity
    {
        /// <summary>
        /// 資產唯一識別碼
        /// </summary>
        [Key]
        public Guid ASSET_ID { get; set; }

        /// <summary>
        /// 系統自動生成的資材編碼，如：COMP-NB-MAC-0001
        /// </summary>
        [MaxLength(30, ErrorMessage = "資材編碼長度不能超過 30 個字元")]
        public string? ASSET_CODE { get; set; }

        /// <summary>
        /// 關聯的品名唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "請選擇品名")]
        public Guid? IN_ID { get; set; }

        /// <summary>
        /// 關聯的品名導覽屬性
        /// </summary>
        [ForeignKey("IN_ID")]
        public virtual ItemName? ItemName { get; set; }

        /// <summary>
        /// 規格型號，例如：MacBook Pro 16
        /// </summary>
        [MaxLength(50, ErrorMessage = "型號長度不能超過 50 個字元")]
        public string? MODEL { get; set; }

        /// <summary>
        /// 品牌，例如：Apple
        /// </summary>
        [MaxLength(50, ErrorMessage = "廠牌長度不能超過 50 個字元")]
        public string? BRAND { get; set; }

        /// <summary>
        /// 關聯的計量單位唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "請選擇標準單位")]
        public Guid? UNIT_ID { get; set; }

        /// <summary>
        /// 關聯的計量單位導覽屬性
        /// </summary>
        [ForeignKey("UNIT_ID")]
        public virtual AssetUnit? AssetUnit { get; set; }

        /// <summary>
        /// 預設存放儲位唯一識別碼
        /// </summary>
        public Guid? BIN_ID { get; set; }

        /// <summary>
        /// 預設存放儲位導覽屬性
        /// </summary>
        [ForeignKey("BIN_ID")]
        public virtual StorageBin? StorageBin { get; set; }
    }
}
