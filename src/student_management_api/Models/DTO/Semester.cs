namespace student_management_api.Models.DTO;

public class Semester
{
    public int Id { get; set; }

    public int SemesterNum { get; set; }

    public int YearId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
