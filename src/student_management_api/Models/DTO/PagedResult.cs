namespace student_management_api.Models.DTO;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalCount { get; set; }
}
