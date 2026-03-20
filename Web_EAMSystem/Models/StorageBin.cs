using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    public class StorageBin
    {
        [Key]
        public Guid BIN_ID { get; set; }

        // 🌟 關聯到資材室的外鍵
        [Required(ErrorMessage = "請選擇所屬資材室")]
        public Guid ROOM_ID { get; set; }

        [ForeignKey("ROOM_ID")]
        public StoreRoom? StoreRoom { get; set; }

        // 儲位代號 (例如: "A-01", "A-02")
        [Required(ErrorMessage = "請輸入儲位代號")]
        [MaxLength(20)]
        public string BIN_CODE { get; set; }

        // --- 以下為系統共用欄位 ---
        [MaxLength(10)]
        public string? Creator { get; set; }
        [MaxLength(10)]
        public string? CreatorId { get; set; }
        public DateTime CreatedDate { get; set; }
        [MaxLength(10)]
        public string? Modifier { get; set; }
        [MaxLength(10)]
        public string? ModifierId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDisabled { get; set; }
    }
}
