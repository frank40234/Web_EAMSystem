using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產大類實體，定義資產最高層級類別（如 IT設備、辦公設備等）
    /// </summary>
    public class AssetCategory : BaseEntity
    {
        /// <summary>
        /// 大類唯一識別碼
        /// </summary>
        [Key]
        public Guid MAIN_CAT_ID { get; set; }

        /// <summary>
        /// 大類名稱，例如：IT設備
        /// </summary>
        [Required(ErrorMessage = "請輸入大類名稱")]
        [MaxLength(20, ErrorMessage = "大類名稱長度不能超過 20 個字元")]
        public string MAIN_CAT_NAME { get; set; } = string.Empty;

        /// <summary>
        /// 大類代號（通常為2碼），例如：IT
        /// </summary>
        [Required(ErrorMessage = "請輸入大類代號")]
        [MaxLength(2, ErrorMessage = "大類代號長度不能超過 2 個字元")]
        public string MAIN_CAT_CODE { get; set; } = string.Empty;
    }
}
