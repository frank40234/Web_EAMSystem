using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資產品名實體，隸屬於資產次類，代表具體的產品類別（如 筆記型電腦、伺服器等）
    /// </summary>
    public class ItemName : BaseEntity
    {
        /// <summary>
        /// 品名唯一識別碼
        /// </summary>
        [Key]
        public Guid IN_ID { get; set; }

        /// <summary>
        /// 所屬的次類唯一識別碼
        /// </summary>
        [Required(ErrorMessage = "請選擇所屬次類")]
        public Guid? SUB_CAT_ID { get; set; }

        /// <summary>
        /// 所屬的次類導覽屬性
        /// </summary>
        [ForeignKey("SUB_CAT_ID")]
        public virtual SubAssetCategory? SubAssetCategory { get; set; }

        /// <summary>
        /// 品名名稱，例如：筆記型電腦
        /// </summary>
        [Required(ErrorMessage = "請輸入品名")]
        [MaxLength(20, ErrorMessage = "品名長度不能超過 20 個字元")]
        public string IN_NAME { get; set; } = string.Empty;

        /// <summary>
        /// 品名代號（通常為3碼），例如：MAC
        /// </summary>
        [Required(ErrorMessage = "請輸入品名代號")]
        [MaxLength(3, ErrorMessage = "品名代號長度不能超過 3 個字元")]
        public string IN_CODE { get; set; } = string.Empty;
    }
}
