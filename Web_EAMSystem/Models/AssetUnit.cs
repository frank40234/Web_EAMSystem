using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產單位實體，定義系統共用的計量單位（如 台、個、箱等）
    /// </summary>
    public class AssetUnit : BaseEntity
    {
        /// <summary>
        /// 單位唯一識別碼
        /// </summary>
        [Key]
        public Guid ASSET_UNIT_ID { get; set; }

        /// <summary>
        /// 單位名稱，例如：台
        /// </summary>
        [Required(ErrorMessage = "請輸入資產單位")]
        [MaxLength(20, ErrorMessage = "資產單位長度不能超過 20 個字元")]
        public string ASSET_UNIT { get; set; } = string.Empty;

        /// <summary>
        /// 單位代碼，例如：PC
        /// </summary>
        [Required(ErrorMessage = "請輸入資產單位代號")]
        [MaxLength(5, ErrorMessage = "單位代號長度不能超過 5 個字元")]
        public string ASSET_UNIT_CODE { get; set; } = string.Empty;
    }
}
