using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 所有實體模型的稽核與停用狀態基底類別，用以統一追蹤資料異動
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 建立此資料的使用者姓名
        /// </summary>
        [MaxLength(10)]
        public string? Creator { get; set; }

        /// <summary>
        /// 建立此資料的使用者帳號或工號
        /// </summary>
        [MaxLength(10)]
        public string? CreatorId { get; set; }

        /// <summary>
        /// 資料建立的系統時間
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 最後修改此資料的使用者姓名
        /// </summary>
        [MaxLength(10)]
        public string? Modifier { get; set; }

        /// <summary>
        /// 最後修改此資料的使用者帳號或工號
        /// </summary>
        [MaxLength(10)]
        public string? ModifierId { get; set; }

        /// <summary>
        /// 最後修改的系統時間
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 標記此資料是否已被邏輯停用或軟刪除
        /// </summary>
        public bool IsDisabled { get; set; } = false;
    }
}
