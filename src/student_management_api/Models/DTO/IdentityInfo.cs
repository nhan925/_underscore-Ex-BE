using System.Text.Json.Nodes;

namespace student_management_api.Models.DTO;

public class IdentityInfo
{
    public string? Type { get; set; }
    
    public string? Number { get; set; }
    
    public DateTime? DateOfIssue { get; set; }
    
    public string? PlaceOfIssue { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    public Dictionary<string, string>? AdditionalInfo { get; set; }

    public IdentityInfo() { }

    public IdentityInfo(FullIdentityInfo info)
    {
        Type = info.Type;
        Number = info.Number;
        DateOfIssue = info.DateOfIssue;
        PlaceOfIssue = info.PlaceOfIssue;
        ExpiryDate = info.ExpiryDate;
        AdditionalInfo = info.AdditionalInfo;
    }
}
