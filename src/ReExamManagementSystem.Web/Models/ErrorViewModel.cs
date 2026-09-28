namespace ReExamManagementSystem.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; }
    public string Message { get; set; } = "An unexpected error occurred.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
