namespace Sammlerplattform.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        //public ErrorViewModel(string requestId)
        //{
        //    RequestId = requestId;
        //}
    }
}
