namespace student_management_api.Models.Configuration;

public class Configuration<T>
{
    public int Id { get; set; }
    
    public string? Type { get; set; }

    public T? Value { get; set; }

    public bool IsActive { get; set; }
}
