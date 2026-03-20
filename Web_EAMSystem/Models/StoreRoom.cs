using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    public class StoreRoom
    {
        [Key]
        public Guid ROOM_ID { get; set; }

        // 資材室代號或名稱 (例如: "A", "B")
        [Required(ErrorMessage = "請輸入資材室")]
        [MaxLength(5)]
        public string ROOM_NAME { get; set; }

        // --- 以下為系統共用欄位 ---
        [MaxLength(10)]
        public string Creator { get; set; }
        [MaxLength(10)]
        public string CreatorId { get; set; }
        public DateTime CreatedDate { get; set; }
        [MaxLength(10)]
        public string Modifier { get; set; }
        [MaxLength(10)]
        public string ModifierId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDisabled { get; set; }
    }
}
