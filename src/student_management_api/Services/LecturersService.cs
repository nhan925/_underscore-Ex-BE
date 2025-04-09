using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class LecturersService : ILecturersService
{
    private readonly ILecturersRepository _lecturersRepository;

    public LecturersService(ILecturersRepository lecturersRepository)
    {
        _lecturersRepository = lecturersRepository;
    }

    public async Task<List<Lecturer>> GetAllLecturers()
    {
        var lecturers = await _lecturersRepository.GetAllLecturers();
        return lecturers;
    }
}
