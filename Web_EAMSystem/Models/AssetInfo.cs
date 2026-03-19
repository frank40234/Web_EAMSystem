using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    public class AssetInfo
    {
        // 1. GUID (主鍵)
        [Key]
        public Guid ASSET_ID { get; set; }

        // 2. 物料編碼 (自動產生，如：COMP-NB-MAC-0001)
        [MaxLength(30)]
        public string? ASSET_CODE { get; set; } // 允許空值，因為存檔前由系統自動填入

        // 3. 品名關聯 (透過品名，就能抓到類別與大類)
        [Required(ErrorMessage = "請選擇品名")]
        public Guid? IN_ID { get; set; }

        [ForeignKey("IN_ID")]
        public ItemName? ItemName { get; set; }

        // 4. 型號
        [MaxLength(50)]
        public string? MODEL { get; set; }

        // 5. 廠牌
        [MaxLength(50)]
        public string? BRAND { get; set; }

        // 6. 標準單位 (關聯到 AssetUnit)
        [Required(ErrorMessage = "請選擇標準單位")]
        public Guid? UNIT_ID { get; set; }

        [ForeignKey("UNIT_ID")]
        public AssetUnit? AssetUnit { get; set; }

        // --- 以下為系統共用的稽核與狀態欄位 ---

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

        // 7. 狀態(是否停用)
        public bool IsDisabled { get; set; }
    }
}
