﻿using student_management_api.Contracts.IRepositories;
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
        if (faculties == null)
        {
            throw new Exception("no faculties found");
        }

        return faculties;
    }
}
