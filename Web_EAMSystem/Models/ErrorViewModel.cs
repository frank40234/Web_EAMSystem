namespace Web_EAMSystem.Models
{
    /// <summary>
    /// 系統錯誤檢視模型，用於向使用者顯示錯誤相關欄位與要求識別碼
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// 要求識別碼 (可為空)
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 錯誤詳細訊息，給予預設值以消除 Nullable 警告
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// 是否顯示要求識別碼的判斷條件
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
