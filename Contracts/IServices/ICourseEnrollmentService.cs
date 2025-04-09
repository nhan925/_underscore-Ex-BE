using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface ICourseEnrollmentService
{
    Task RegisterClass(CourseEnrollmentRequest request);

    Task UnregisterClass(CourseEnrollmentRequest request);

    Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId);


    /// <summary>
    /// Generates a PDF transcript for a student based on a provided HTML template.
    /// </summary>
    /// <remarks>
    /// The HTML template must contain the following placeholders which will be replaced with actual data:
    /// <list type="bullet">
    ///   <item><description><c>{{school_name}}</c> - The name of the school/university</description></item>
    ///   <item><description><c>{{student_name}}</c> - The full name of the student</description></item>
    ///   <item><description><c>{{student_id}}</c> - The student's ID number</description></item>
    ///   <item><description><c>{{intake_year}}</c> - The year the student was admitted</description></item>
    ///   <item><description><c>{{dob}}</c> - The student's date of birth (formatted)</description></item>
    ///   <item><description><c>{{course_rows}}</c> - HTML table rows containing course information</description></item>
    ///   <item><description><c>{{total_credits}}</c> - The total number of credits earned</description></item>
    ///   <item><description><c>{{gpa}}</c> - The student's grade point average</description></item>
    /// </list>
    /// 
    /// The course rows will be generated in the following HTML format:
    /// <code>
    /// &lt;tr&gt;&lt;td&gt;Course ID&lt;/td&gt;&lt;td&gt;Course Name&lt;/td&gt;&lt;td&gt;Credits&lt;/td&gt;&lt;td&gt;Grade&lt;/td&gt;&lt;/tr&gt;
    /// </code>
    /// </remarks>
    Task<Stream> GetTranscriptOfStudentById(string studentId, string htmlTemplate);
}
