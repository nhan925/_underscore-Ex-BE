using System.Text.Json.Nodes;

namespace student_management_api.Models.DTO;

public class FullIdentityInfo
{
    public string StudentId { get; set; }
    public string Type { get; set; }
    public string Number { get; set; }
    public DateTime? DateOfIssue { get; set; }
    public string PlaceOfIssue { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}
