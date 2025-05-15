using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _facultyRepository;

    public FacultyService(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<List<Faculty>> GetAllFaculties()
    {
        var faculties = await _facultyRepository.GetAllFaculties();
        return faculties ?? new();
    }

    public async Task<int> UpdateFaculty(Faculty faculty)
    {
        return await _facultyRepository.UpdateFaculty(faculty);
    }

    public async Task<int> AddFaculty(string name)
    {
        return await _facultyRepository.AddFaculty(name);
    }
}
