namespace WebApp1.Models
{
    public class CustomErrorResponse
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public CustomErrorResponse(string message, string stackTrace)
        {
            Message = message;
            StackTrace = stackTrace;
        }
    }
}
