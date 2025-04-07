namespace student_management_api.Models.DTO;

public class EnrollmentHistory
{
    public string StudentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CourseId { get; set; }

    public string ClassId { get; set; }

    public int SemesterId { get; set; }

    private string _action;
    public string Action
    {
        get {
            if (_action == "register")
            {
                return "Đăng ký";
            }
            else if (_action == "cancel")
            {
                return "Hủy đăng ký";
            }
            else
            {
                return "Unknown action";
            }
        }

        set => _action = value;
    }
}
