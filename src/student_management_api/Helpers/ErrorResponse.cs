namespace student_management_api.Helpers;

public class ErrorResponse<T>
{
    public int Status { get; set; }
    
    public string Message { get; set; }
    
    public T? Details { get; set; }
    
    public ErrorResponse(int status, string message, T? details = default)
    {
        Status = status;
        Message = message;
        Details = details;
    }
}
