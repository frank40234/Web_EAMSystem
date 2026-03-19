using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    public class SubAssetCategory
    {
        //GUID 
        [Key]
        public Guid SUB_CAT_ID { get; set; }

        //大類名稱
        [Required(ErrorMessage = "請輸入類別代號")]
        [MaxLength(2)]
        public string MAIN_CAT_CODE { get; set; }

        //類別名稱
        [Required(ErrorMessage = "請輸入類別名稱")]
        [MaxLength(20)]
        public string SUB_CAT_NAME { get; set; }

        //類別代號
        [Required(ErrorMessage = "請輸入類別代號")]
        [MaxLength(2)]
        public string SUB_CAT_CODE { get; set; }

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
