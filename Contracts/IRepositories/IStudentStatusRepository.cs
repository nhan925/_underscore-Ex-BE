﻿using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IStudentStatusRepository
{
    Task<List<StudentStatus>> GetAllStudentStatuses();

    Task<int> UpdateStudentStatus(StudentStatus studentStatus);

    Task<int> AddStudentStatus(string name);

    Task<int> UpdateReferenceState(int statusId, bool state);
}
