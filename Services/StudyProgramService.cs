using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class StudyProgramService: IStudyProgramService
{
    private readonly IStudyProgramRepository _programRepository;
    public StudyProgramService(IStudyProgramRepository programRepository)
    {
        _programRepository = programRepository;
    }

    public async Task<List<StudyProgram>> GetAllPrograms()
    {
        var programs = await _programRepository.GetAllPrograms();
        if (programs == null)
        {
            throw new Exception("no program found");
        }

        return programs;
    }

    public async Task<int> UpdateProgram(StudyProgram program)
    {
        return await _programRepository.UpdateProgram(program);
    }

    public async Task<int> AddProgram(string name)
    {
        return await _programRepository.AddProgram(name);
    }
}
