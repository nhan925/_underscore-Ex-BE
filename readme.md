# [BE] Student Management

## Cấu trúc source code
### Các component
```
src/
│
├── student_management_api_tests/               # Project test
│
└── student_management_api/                     # Main project
    │
    ├── Controllers/                            # API endpoints
    │   ├── AuthController.cs
    │   ├── ConfigurationController.cs
    │   ├── CountryPhoneCodeController.cs
    │   ├── CourseClassController.cs
    │   ├── CourseController.cs
    │   ├── CourseEnrollmentController.cs
    │   ├── FacultyController.cs
    │   ├── LecturersController.cs
    │   ├── StudentController.cs
    │   ├── StudentStatusController.cs
    │   ├── StudyProgramController.cs
    │   └── YearAndSemesterController.cs
    |	
    ├── Services/                               # Business logic layer
    │   ├── FacultyService.cs
    │   ├── ConfigurationService.cs
    │   ├── CountryPhoneCodeService.cs
    │   ├── CourseClassService.cs
    │   ├── CourseEnrollmentService.cs
    │   ├── CourseService.cs
    │   ├── JwtService.cs
    │   ├── LecturersService.cs
    │   ├── StudentService.cs
    │   ├── StudentStatusService.cs
    │   ├── StudyProgramService.cs
    │   └── YearAndSemesterService.cs
    │
    ├── Repositories/                           # Data access layer
    │   ├── FacultyRepository.cs
    │   ├── ConfigurationRepository.cs
    │   ├── CourseClassRepository.cs
    │   ├── CourseEnrollmentRepository.cs
    │   ├── CourseRepository.cs
    │   ├── LecturersRepository.cs
    │   ├── StudentRepository.cs
    │   ├── StudentStatusRepository.cs
    │   ├── StudyProgramRepository.cs
    │   ├── UserRepository.cs
    │   └── YearAndSemesterRepository.cs
    │
    ├── Models/                                 # Data structures
    │   ├── Authentication/
    │   │   └── AuthRequest.cs
    │   │
    │   ├── Configuration/                      
    │   │   ├── Configuration.cs 
    │   │   └── CountryPhoneCode.cs 
    │   │
    │   ├── Course/
    │   │   └── SimplifiedCourseWithGrade.cs
    │   │
    │   ├── CourseClass/                      
    │   │   ├── GetCourseClassResult.cs 
    │   │   ├── GetStudentsInClassRequest.cs 
    │   │   └── StudentInClass.cs 
    │   │
    │   ├── CourseEnrollment/                      
    │   │   ├── CourseEnrollmentRequest.cs 
    │   │   └── UpdateStudentGradeRequest.cs 
    │   │
    │   ├── DTO/                      
    │   │   ├── Address.cs 
    │   │   ├── Course.cs 
    │   │   ├── CourseClass.cs 
    │   │   ├── EnrollmentHistory.cs 
    │   │   ├── Faculty.cs 
    │   │   ├── FullAddress.cs 
    │   │   ├── FullIdentityInfo.cs 
    │   │   ├── IdentityInfo.cs 
    │   │   ├── Lecturer.cs 
    │   │   ├── PagedResult.cs 
    │   │   ├── Semester.cs 
    │   │   ├── Student.cs 
    │   │   ├── StudentStatus.cs 
    │   │   ├── StudyProgram.cs 
    │   │   ├── User.cs 
    │   │   └── Year.cs
    │   │
    │   └── Student/
    │       ├── AddStudentRequest.cs  
    │       ├── SimplifiedStudent.cs 
    │       ├── StudentFilter.cs 
    │       ├── Transcript.cs 
    │       └── UpdateStudentRequest.cs 
    │
    ├── Contracts/                              # Interfaces
    │   ├── IRepositories/
    │   │   ├── IFacultyRepository.cs
    │   │   ├── IConfigurationRepository.cs
    │   │   ├── ICourseClassRepository.cs
    │   │   ├── ICourseEnrollmentRepository.cs
    │   │   ├── ICourseRepository.cs
    │   │   ├── ILecturersRepository.cs
    │   │   ├── IStudentRepository.cs
    │   │   ├── IStudentStatusRepository.cs
    │   │   ├── IStudyProgramRepository.cs
    │   │   ├── IUserRepository.cs
    │   │   └── IYearAndSemesterRepository.cs
    │   │
    │   └── IServices/
    │       ├── IFacultyService.cs
    │       ├── IConfigurationService.cs
    │       ├── ICountryPhoneCodeService.cs
    │       ├── ICourseClassService.cs
    │       ├── ICourseEnrollmentService.cs
    │       ├── ICourseService.cs
    │       ├── IJwtService.cs
    │       ├── ILeturersService.cs
    │       ├── IStudentService.cs
    │       ├── IStudentStatusService.cs
    │       ├── IStudyProgramService.cs
    │       └── IYearAndSemesterService.cs
    │
    ├── Helpers/                                # Helper classes
    │   ├── JsonbTypeHandler.cs                 # Custom Dapper type handler for JSON
    │   ├── ErrorResponse.cs                    # Error response model
    │   └── LanguageHeaderOperationFilter.cs    # Filter for Swagger to handle language headers
    │
    ├── Middlewares/                            # Custom middleware components
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── LoggingEnrichmentMiddleware.cs
    │
    ├── Exceptions/                             # Customed exceptions
    │   ├── ForbiddenException.cs
    │   ├── EnvironmentVariableNotFoundException.cs
    │   ├── OperationFailedException.cs
    │   └── NotFoundException.cs
    │
    ├── Localization/                           # Localization services
    │   ├── ExternalTranslationService.cs       # Local LLM translation service
    │   ├── GeminiTranslationService.cs         # Gemini translation service
    │   └── IExternalTranslationService.cs      # Interface for translation services
    │
    ├── Resources/                              # Localization resources (messages)
    │   ├── Messages.resx                       # Default English messages
    │   └── Messages.vi.resx                    # Vietnamese messages
    │
    ├── Templates/                              # Student transcript templates
    │   ├── transcript_template-en.html 
    │   └── transcript_template-vi.html
    │
    ├── .env                                    # Environment variables (not in repo)
    ├── .env.example                            # Example environment variables template
    ├── create_db.sql                           # Script to create database schema
    └── Program.cs   
```
### Flow hoạt động - Kiến trúc
<div align=center>
	<img src="Documents/call_flow.svg" alt="Call Flow" width="300">
</div>


## Hướng dẫn cài đặt & chạy chương trình
- Sử dụng Visual Studio 2022
- Cài gói `ASP.NET and web development`
- Thêm file `.env` vào thư mục gốc của project theo mẫu (xem file `.env.example`)
- Cần chạy source Backend để có thể chạy được Frontend
- Chạy source bằng cách nhấn `F5` hoặc `Ctrl + F5`

## Hướng dẫn chạy Tests
- Mở `student_management_api_tests` trong Visual Studio
- Chọn `Test` -> `Run All Tests` hoặc nhấn `Ctrl + R, A`

## Lưu ý
- Để test thử APIs có thể sử dụng Swagger (được tích hợp sẵn) bằng cách vào `https://localhost:7285`
- Cần thêm file `.env` vào thư mục gốc của project để chạy được chương trình

## Hình ảnh các chức năng
- Xem bên source Front-end: [student-management-frontend](https://github.com/nhan925/_underscore-Ex-FE.git)