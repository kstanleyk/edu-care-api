namespace EduCare.Application.Helpers;

public abstract class BaseResponse
{
    protected BaseResponse()
    {
        Success = true;
        ValidationErrors = [];
        Message = string.Empty;
    }

    protected BaseResponse(string? message)
    {
        Success = true;
        Message = message ?? string.Empty;
        ValidationErrors = [];
    }

    protected BaseResponse(string? message, bool success)
    {
        Success = success;
        Message = message ?? string.Empty;
        ValidationErrors = [];
    }

    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> ValidationErrors { get; set; }
}