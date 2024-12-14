namespace RoboticsManager.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int? StatusCode { get; set; }
        public string? StackTrace { get; set; }
        public bool ShowStackTrace => !string.IsNullOrEmpty(StackTrace);

        public ErrorViewModel()
        {
        }

        public ErrorViewModel(string? requestId)
        {
            RequestId = requestId;
        }

        public ErrorViewModel(int statusCode, string? requestId = null)
        {
            StatusCode = statusCode;
            RequestId = requestId;
        }

        public ErrorViewModel(Exception exception, string? requestId = null)
        {
            RequestId = requestId;
#if DEBUG
            StackTrace = exception.ToString();
#endif
        }
    }
}
