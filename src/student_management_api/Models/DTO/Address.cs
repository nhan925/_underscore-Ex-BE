namespace student_management_api.Models.DTO;

public class Address
{
    public string? Type { get; set; }
    
    public string? Other { get; set; }
    
    public string? Village { get; set; }
    
    public string? District { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }

    public Address() { }

    public Address(FullAddress fullAddress)
    {
        Type = fullAddress.Type;
        Other = fullAddress.Other;
        Village = fullAddress.Village;
        District = fullAddress.District;
        City = fullAddress.City;
        Country = fullAddress.Country;
    }
}
