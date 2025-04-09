using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IStudyProgramRepository
{
    Task<List<StudyProgram>> GetAllPrograms();

    Task<int> UpdateProgram(StudyProgram program);

    Task<int> AddProgram(string name);
}
