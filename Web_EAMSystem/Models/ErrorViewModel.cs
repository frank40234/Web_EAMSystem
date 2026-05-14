namespace Web_EAMSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public string Error { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
