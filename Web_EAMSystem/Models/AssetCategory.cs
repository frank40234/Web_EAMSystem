using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_EAMSystem.Models
{
    public class AssetCategory
    {
        //GUID 
        [Key]
        public Guid MAIN_CAT_ID { get; set; }

        //大類名稱
        [Required(ErrorMessage ="請輸入大類名稱")]
        [MaxLength(50)]
        public string MAIN_CAT_NAME { get; set; }

        //大類代號
        [Required(ErrorMessage = "請輸入大類代號")]
        [MaxLength(50)]
        public string MAIN_CAT_CODE { get; set; }

        //建檔者
        [Required(ErrorMessage = "請輸入姓名")]
        [MaxLength(50)]
        public string Creator { get; set; }
        //建檔者工號
        [MaxLength(25)]
        public string CreatorId { get; set; }

        //建檔日期
        public DateTime CreatedDate { get; set; }

        //修改者
        [MaxLength(50)]
        public string Modifier { get; set; }
        //修改者ID
        [MaxLength(25)]
        public string ModifierId { get; set; }

        // 7. 異動日期 (使用 DateTime? 允許空值 Nullable)
        public DateTime ModifiedDate { get; set; }

        // 8. 狀態(是否停用) - True代表停用，False代表啟用
        public bool IsDisabled { get; set; }

    }
}
