using DocumentFormat.OpenXml.Wordprocessing;

namespace student_management_api.Models.CourseClass;

public class StudentInClass
{
    public string? Id { get; set; }

    public string? FullName { get; set; }

    public float? Grade { get; set; }

    private string? _status;
    public string? Status
    {
        get
        {
            if (_status == "enrolled")
            {
                return "Đã đăng ký";
            }
            else if (_status == "failed")
            {
                return "Rớt";
            }
            else if (_status == "passed")
            {
                return "Đã qua môn";
            }
            else
            {
                return _status;
            }
        }

        set => _status = value;
    }
}
