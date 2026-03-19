using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    public class AssetInfo
    {
        [Key]
        public Guid ASSET_ID{  get; set; }

        [MaxLength(14)]
        public string ASSET_CODE { get; set; }

        //大類代號
        [Required(ErrorMessage = "請輸入大類代號")]
        [MaxLength(2)]
        public string MAIN_CAT_CODE { get; set; }

        //類別代號
        [Required(ErrorMessage = "請輸入類別代號")]
        [MaxLength(2)]
        public string SUB_CAT_CODE { get; set; }

        //品名代號
        [Required(ErrorMessage = "請輸入品名代號")]
        [MaxLength(3)]
        public string IN_CODE { get; set; }

        //品名
        [Required(ErrorMessage = "請輸入品名")]
        [MaxLength(20)]
        public string IN { get; set; }

        //品名代號
        [Required(ErrorMessage = "請輸入型號")]
        [MaxLength(50)]
        public string MODEL { get; set; }

        //規格
        [Required(ErrorMessage = "請輸入規格")]
        [MaxLength(50)]
        public string SPEC { get; set; }

        //品名代號
        [Required(ErrorMessage = "請輸入廠牌")]
        [MaxLength(50)]
        public string BRAND { get; set; }

        [Required(ErrorMessage = "請輸入單位")]
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
