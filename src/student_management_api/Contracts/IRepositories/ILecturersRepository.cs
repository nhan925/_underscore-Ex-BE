using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface ILecturersRepository
{
    Task<List<Lecturer>> GetAllLecturers();
}
