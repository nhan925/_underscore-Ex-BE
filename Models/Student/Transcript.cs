using student_management_api.Models.Course;
using student_management_api.Models.DTO;

namespace student_management_api.Models.Student;

public class Transcript
{
    public List<SimpliedCourseWithGrade> Courses { get; set; }

    public int TotalCredits { get; set; }

    public float GPA { get; set; }
}
