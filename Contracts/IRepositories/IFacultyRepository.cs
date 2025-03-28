﻿using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IFacultyRepository
{
    Task<List<Faculty>> GetAllFaculties();

    Task<int> UpdateFaculty(Faculty faculty);

    Task<int> AddFaculty(string name);
}
