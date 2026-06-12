using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 資材室區域實體，例如 A區、B區 等
    /// </summary>
    public class StoreRoom : BaseEntity
    {
        /// <summary>
        /// 資材室唯一識別碼
        /// </summary>
        [Key]
        public Guid ROOM_ID { get; set; }

        /// <summary>
        /// 資材室名稱，例如：A區
        /// </summary>
        [Required(ErrorMessage = "請輸入資材室")]
        [MaxLength(5, ErrorMessage = "資材室名稱長度不能超過 5 個字元")]
        public string ROOM_NAME { get; set; } = string.Empty;
    }
}
