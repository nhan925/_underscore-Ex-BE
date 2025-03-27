# [BE] Student Management

## Cấu trúc source code
### Các component
```
student_management_api/
│
├── Controllers/                  # API endpoints
│   ├── AuthController.cs
│   ├── ConfigurationController.cs
│   ├── CountryPhoneCodeController.cs
│   ├── FacultyController.cs
│   ├── StudentController.cs
│   ├── StudentStatusController.cs
│   └── StudyProgramController.cs
|	
├── Services/                     # Business logic layer
│   ├── FacultyService.cs
│   ├── ConfigurationService.cs
│   ├── CountryPhoneCodeService.cs
│   ├── JwtService.cs
│   ├── StudentService.cs
│   ├── StudentStatusService.cs
│   └── StudyProgramService.cs
│
├── Repositories/                 # Data access layer
│   ├── FacultyRepository.cs
│   ├── ConfigurationRepository.cs
│   ├── StudentRepository.cs
│   ├── StudentStatusRepository.cs
│   ├── StudyProgramRepository.cs
│   └── UserRepository.cs
│
├── Models/                       # Data structures
│   ├── Authentication/
│   │   └── AuthRequest.cs
│   │
│   ├── Configuration/                      
│   │   ├── Configuration.cs 
│   │   ├── CountryPhoneCode.cs 
│   │
│   ├── DTO/                      
│   │   ├── Address.cs 
│   │   ├── Faculty.cs 
│   │   ├── FullAddress.cs 
│   │   ├── FullIdentityInfo.cs 
│   │   ├── IdentityInfo.cs 
│   │   ├── PagedResult.cs 
│   │   ├── Student.cs 
│   │   ├── StudentStatus.cs 
│   │   ├── StudyProgram.cs 
│   │   └── User.cs
│   │
│   └── Student/
│       ├── AddStudentRequest.cs  
│       ├── SimplifiedStudent.cs 
│       ├── StudentFilter.cs 
│       └── UpdateStudentRequest.cs 
│
├── Contracts/                    # Interfaces
│   ├── IRepositories/
│   │   ├── IFacultyRepository.cs
│   │   ├── IConfigurationRepository.cs
│   │   ├── IStudentRepository.cs
│   │   ├── IStudentStatusRepository.cs
│   │   ├── IStudyProgramRepository.cs
│   │   └── IUserRepository.cs
│   │
│   └── IServices/
│       ├── IFacultyService.cs
│       ├── IConfigurationService.cs
│       ├── ICountryPhoneCodeService.cs
│       ├── IJwtService.cs
│       ├── IStudentService.cs
│       ├── IStudentStatusService.cs
│       └── IStudyProgramService.cs
│
├── Helpers/                      # Helper classes
│   └── JsonbTypeHandler.cs       # Custom Dapper type handler for JSON
│
├── Middlewares/                  # Custom middleware components
│   ├── ExceptionHandlingMiddleware.cs
│   └── LoggingEnrichmentMiddleware.cs
│
├── .env                          # Environment variables (not in repo)
├── .env.example                  # Example environment variables template
├── create_db.sql                 # Script to create database schema
└── Program.cs   
```
### Flow hoạt động
<div align=center>
	<img src="Documents/call_flow.svg" alt="Call Flow" width="300">
</div>


## Hướng dẫn cài đặt & chạy chương trình
- Sử dụng Visual Studio 2022
- Cài gói `ASP.NET and web development`
- Thêm file `.env` vào thư mục gốc của project theo mẫu (xem file `.env.example`)
- Cần chạy source Backend để có thể chạy được Frontend
- Chạy source bằng cách nhấn `F5` hoặc `Ctrl + F5`

## Lưu ý
- Để test thử APIs có thể sử dụng Swagger (được tích hợp sẵn) bằng cách vào `https://localhost:7285`
- Cần thêm file `.env` vào thư mục gốc của project để chạy được chương trình

## Hình ảnh các chức năng
- Xem bên source Front-end: [student-management-frontend](https://github.com/nhan925/_underscore-Ex-FE.git)