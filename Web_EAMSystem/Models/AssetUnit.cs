using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    public class AssetUnit
    {
        //GUID 
        [Key]
        public Guid ASSET_UNIT_ID { get; set; }

        //單位名稱
        [Required(ErrorMessage = "請輸入資產單位")]
        [MaxLength(20)]
        public string ASSET_UNIT { get; set; }

        [Required(ErrorMessage = "請輸入資產單位代號")]
        [MaxLength(5)]
        public string ASSET_UNIT_CODE { get; set; }

        //建檔者
        [MaxLength(10)]
        public string Creator { get; set; }
        //建檔者工號
        [MaxLength(10)]
        public string CreatorId { get; set; }

        //建檔日期
        public DateTime CreatedDate { get; set; }

        //修改者
        [MaxLength(10)]
        public string Modifier { get; set; }
        //修改者ID
        [MaxLength(10)]
        public string ModifierId { get; set; }

        // 7. 異動日期 (使用 DateTime? 允許空值 Nullable)
        public DateTime ModifiedDate { get; set; }

        // 8. 狀態(是否停用) - True代表停用，False代表啟用
        public bool IsDisabled { get; set; }
    }
}
