using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產次類實體，隸屬於資產大類（如 電腦設備、事務設備等）
    /// </summary>
    public class SubAssetCategory : BaseEntity
    {
        /// <summary>
        /// 次類唯一識別碼
        /// </summary>
        [Key]
        public Guid SUB_CAT_ID { get; set; }

        /// <summary>
        /// 所屬的大類唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "請選擇所屬大類")]
        public Guid? MAIN_CAT_ID { get; set; }

        /// <summary>
        /// 所屬的大類導覽屬性
        /// </summary>
        [ForeignKey("MAIN_CAT_ID")]
        public virtual AssetCategory? AssetCategory { get; set; }

        /// <summary>
        /// 次類名稱，例如：電腦設備
        /// </summary>
        [Required(ErrorMessage = "請輸入類別名稱")]
        [MaxLength(20, ErrorMessage = "類別名稱長度不能超過 20 個字元")]
        public string SUB_CAT_NAME { get; set; } = string.Empty;

        /// <summary>
        /// 次類代號（通常為2碼），例如：PC
        /// </summary>
        [Required(ErrorMessage = "請輸入類別代號")]
        [MaxLength(2, ErrorMessage = "類別代號長度不能超過 2 個字元")]
        public string SUB_CAT_CODE { get; set; } = string.Empty;
    }
}
