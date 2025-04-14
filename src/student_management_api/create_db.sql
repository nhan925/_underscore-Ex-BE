
------------------------ v1.0 -------------------------

-- Enable the UUID extension (only needed once per database)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create the users table with UUID as primary key
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(), -- Unique user ID
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create the faculties table
CREATE TABLE faculties (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

-- Create the student_statuses table
CREATE TABLE student_statuses (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

-- Create the students table
CREATE TABLE students (
    id VARCHAR(10) PRIMARY KEY,  -- Mã số sinh viên
    full_name TEXT NOT NULL,  -- Họ tên
    date_of_birth DATE NOT NULL,  -- Ngày tháng năm sinh
    gender VARCHAR(10) CHECK (gender IN ('Nam', 'Nữ', 'Khác')),  -- Giới tính
    faculty_id INT NOT NULL,  -- Khoa (Foreign Key)
    intake_year INT NOT NULL,  -- Khóa (Year of Enrollment)
    program TEXT NOT NULL,  -- Chương trình
    address TEXT,  -- Địa chỉ
    email TEXT UNIQUE NOT NULL,  -- Email
    phone_number VARCHAR(15) UNIQUE,  -- Số điện thoại liên hệ
    status_id INT NOT NULL,  -- Tình trạng sinh viên (Foreign Key)
    FOREIGN KEY (faculty_id) REFERENCES faculties(id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (status_id) REFERENCES student_statuses(id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO users (id, username, password)
VALUES (uuid_generate_v4(), 'admin', crypt('admin', gen_salt('bf')));

-- Insert faculties
INSERT INTO faculties (name) VALUES 
('Khoa Luật'),
('Khoa Tiếng Anh thương mại'),
('Khoa Tiếng Nhật'),
('Khoa Tiếng Pháp');

-- Insert student statuses
INSERT INTO student_statuses (name) VALUES 
('Đang học'),
('Đã tốt nghiệp'),
('Đã thôi học'),
('Tạm dừng học');

-- Insert 20 students with different intake years
--INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program, address, email, phone_number, status_id) VALUES 
--('20010001', 'Nguyễn Văn Anh', '2002-05-10', 'Nam', 1, 2020, 'Luật Dân sự', '123 Trần Hưng Đạo, Hà Nội', 'nguyenvana@student.com', '0987654321', 1),
--('20020001', 'Trần Thị Bông', '2001-09-15', 'Nữ', 2, 2020, 'Tiếng Anh Thương mại', '456 Nguyễn Huệ, TP. HCM', 'tranthib@student.com', '0971234567', 1),
--('21030001', 'Lê Văn Cẩn', '2003-02-20', 'Nam', 3, 2021, 'Tiếng Nhật', '789 Lê Lợi, Đà Nẵng', 'levanc@student.com', '0969876543', 1),
--('21040001', 'Phạm Minh Dũng', '2000-07-25', 'Nam', 4, 2021, 'Tiếng Pháp', '321 Bạch Đằng, Hải Phòng', 'phamminhd@student.com', '0951112233', 2),
--('22010001', 'Hoàng Thị Én', '1999-12-05', 'Nữ', 1, 2022, 'Luật Hình sự', '654 Lê Thánh Tông, Cần Thơ', 'hoangthie@student.com', '0942223344', 2),
--('22020001', 'Đặng Quốc Phương', '2002-03-18', 'Nam', 2, 2022, 'Tiếng Anh Kinh doanh', '987 Hai Bà Trưng, Hà Nội', 'dangquocf@student.com', '0933334455', 1),
--('22030001', 'Lý Thu Giang', '2001-08-22', 'Nữ', 3, 2022, 'Tiếng Nhật Thực hành', '135 Võ Văn Kiệt, TP. HCM', 'lythug@student.com', '0924445566', 3),
--('22040001', 'Bùi Minh Huy', '2000-06-30', 'Nam', 4, 2022, 'Tiếng Pháp Chuyên sâu', '246 Nguyễn Trãi, Đà Nẵng', 'buiminhh@student.com', '0915556677', 3),
--('23010001', 'Đỗ Hồng Lĩnh', '2003-11-12', 'Nữ', 1, 2023, 'Luật Quốc tế', '369 Hoàng Văn Thụ, Hải Phòng', 'dohongi@student.com', '0906667788', 1),
--('23020001', 'Vũ Thanh Hoa', '2002-04-08', 'Nữ', 2, 2023, 'Tiếng Anh Hành chính', '741 Nguyễn Văn Linh, Cần Thơ', 'vuthanhj@student.com', '0897778899', 1),
--('23030001', 'Ngô Phúc Kiến', '2001-10-27', 'Nam', 3, 2023, 'Tiếng Nhật Doanh nghiệp', '852 Trường Chinh, Hà Nội', 'ngophuck@student.com', '0888889900', 4),
--('23040001', 'Trịnh Minh Lân', '2000-05-15', 'Nam', 4, 2023, 'Tiếng Pháp Ứng dụng', '963 Điện Biên Phủ, TP. HCM', 'trinhminhl@student.com', '0879990011', 2),
--('22010002', 'Cao Thanh Mai', '2003-07-03', 'Nữ', 1, 2022, 'Luật Thương mại', '159 Lạc Long Quân, Đà Nẵng', 'caothanhm@student.com', '0860001122', 1),
--('22020002', 'Đinh Trọng Nam', '2002-01-17', 'Nam', 2, 2022, 'Tiếng Anh Luật', '357 Nguyễn Văn Cừ, Hải Phòng', 'dinhtrongn@student.com', '0851112233', 3),
--('22030002', 'Hà Quốc Tuấn', '2001-09-21', 'Nam', 3, 2022, 'Tiếng Nhật Phiên dịch', '468 Lê Quang Định, Cần Thơ', 'haquoco@student.com', '0842223344', 1),
--('22040016', 'Phùng Minh Phương', '2000-12-10', 'Nữ', 4, 2022, 'Tiếng Pháp Văn hóa', '579 Nam Kỳ Khởi Nghĩa, Hà Nội', 'phungminhp@student.com', '0833334455', 2),
--('23010017', 'Tạ Hoàng Quang', '2003-06-25', 'Nam', 1, 2023, 'Luật Hiến pháp', '680 Tôn Đức Thắng, TP. HCM', 'tahoangq@student.com', '0824445566', 1),
--('23020018', 'Nguyễn Nhật Linh', '2002-08-19', 'Nam', 2, 2023, 'Tiếng Anh Truyền thông', '791 Phan Chu Trinh, Đà Nẵng', 'nguyennhatr@student.com', '0815556677', 1),
--('23030019', 'Đỗ Huy Sáng', '2001-11-30', 'Nữ', 3, 2023, 'Tiếng Nhật Dịch thuật', '802 Nguyễn Công Trứ, Hải Phòng', 'dohuys@student.com', '0806667788', 4),
--('23040020', 'Lương Khánh Trân', '2000-04-02', 'Nữ', 4, 2023, 'Tiếng Pháp Kinh tế', '913 Quang Trung, Cần Thơ', 'luongkhanht@student.com', '0797778899', 2);

------------------------ v2.0 -------------------------

CREATE TABLE programs (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

INSERT INTO programs (name) VALUES
    ('Chính quy'),
    ('CLC'),
    ('Liên thông'),
    ('Tiên tiến');

CREATE TABLE addresses (
    student_id VARCHAR(10) NOT NULL,
    other TEXT NOT NULL,
    village TEXT NOT NULL,
    district TEXT NOT NULL,
    city TEXT NOT NULL,
    country TEXT NOT NULL,
    type TEXT NOT NULL CHECK (type IN ('thuong_tru', 'tam_tru', 'nhan_thu')),
    PRIMARY KEY (student_id, type),
    FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE identity_info (
    student_id VARCHAR(10) PRIMARY KEY,
    number VARCHAR(12) NOT NULL UNIQUE,
    place_of_issue TEXT NOT NULL,
    date_of_issue DATE NOT NULL,
    expiry_date DATE NOT NULL,
    additional_info JSONB,
    type TEXT NOT NULL CHECK (type IN ('cmnd', 'cccd', 'passport')),
    FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE CASCADE ON UPDATE CASCADE,
    CHECK (date_of_issue <= expiry_date)
);


ALTER TABLE students
DROP COLUMN IF EXISTS program;

ALTER TABLE students
DROP COLUMN IF EXISTS address;

ALTER TABLE students
ADD program_id INT NOT NULL;
ALTER TABLE students
ADD FOREIGN KEY (program_id) REFERENCES programs(id) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE students
ADD nationality TEXT NOT NULL; 

INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality) VALUES 
('20010001', 'Nguyễn Văn An', '2002-05-10', 'Nam', 1, 2020, 1, 'nguyenvana@student.com', '0987654321', 1, 'Việt Nam'),
('20020001', 'Trần Thị Bông', '2001-09-15', 'Nữ', 2, 2020, 2, 'tranthib@student.com', '0971234567', 1, 'Việt Nam'),
('21030001', 'Lê Văn Cẩn', '2003-02-20', 'Nam', 3, 2021, 1, 'levanc@student.com', '0969876543', 1, 'Việt Nam'),
('21040001', 'Phạm Minh Dũng', '2000-07-25', 'Nam', 4, 2021, 3, 'phamminhd@student.com', '0951112233', 2, 'Việt Nam'),
('22010001', 'Hoàng Thị Én', '1999-12-05', 'Nữ', 1, 2022, 1, 'hoangthie@student.com', '0942223344', 2, 'Việt Nam'),
('22020001', 'Đặng Quốc Phương', '2002-03-18', 'Nam', 2, 2022, 2, 'dangquocf@student.com', '0933334455', 1, 'Việt Nam'),
('22030001', 'Lý Thu Giang', '2001-08-22', 'Nữ', 3, 2022, 3, 'lythug@student.com', '0924445566', 3, 'Việt Nam'),
('22040001', 'Bùi Minh Huy', '2000-06-30', 'Nam', 4, 2022, 4, 'buiminhh@student.com', '0915556677', 3, 'Việt Nam'),
('23010001', 'Đỗ Hồng Lĩnh', '2003-11-12', 'Nữ', 1, 2023, 1, 'dohongi@student.com', '0906667788', 1, 'Việt Nam'),
('23020001', 'Vũ Thanh Hoa', '2002-04-08', 'Nữ', 2, 2023, 2, 'vuthanhj@student.com', '0897778899', 1, 'Việt Nam'),
('23030001', 'Ngô Phúc Kiến', '2001-10-27', 'Nam', 3, 2023, 3, 'ngophuck@student.com', '0888889900', 4, 'Việt Nam'),
('23040001', 'Trịnh Minh Lân', '2000-05-15', 'Nam', 4, 2023, 4, 'trinhminhl@student.com', '0879990011', 2, 'Việt Nam'),
('22010002', 'Cao Thanh Mai', '2003-07-03', 'Nữ', 1, 2022, 1, 'caothanhm@student.com', '0860001122', 1, 'Việt Nam'),
('22020002', 'Đinh Trọng Nam', '2002-01-17', 'Nam', 2, 2022, 2, 'dinhtrongn@student.com', '0851112233', 3, 'Việt Nam'),
('22030002', 'Hà Quốc Tuấn', '2001-09-21', 'Nam', 3, 2022, 3, 'haquoco@student.com', '0842223344', 1, 'Việt Nam');

INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type) VALUES 
('20010001', '012345678901', 'Công an Hà Nội', '2018-06-15', '2028-06-15', '{"has_chip": "yes"}', 'cccd'),
('20020001', '987654321098', 'Công an TP.HCM', '2019-09-10', '2029-09-10', '{"has_chip": "no"}', 'cccd'),
('21030001', 'A12345678', 'Cục Quản lý Xuất nhập cảnh', '2020-02-01', '2030-02-01', '{"country_of_issue": "Việt Nam", "note": "Dùng cho du học"}', 'passport'),
('21040001', 'B87654321', 'Cục Quản lý Xuất nhập cảnh', '2017-05-20', '2027-05-20', '{"country_of_issue": "Việt Nam"}', 'passport'),
('22010001', '112233445566', 'Công an Đà Nẵng', '2018-08-30', '2028-08-30', '{"has_chip": "yes"}', 'cccd'),
('22020001', '2233445566', 'Công an Hà Nội', '2019-03-18', '2029-03-18', null, 'cmnd'),
('22030001', '334455667788', 'Công an TP.HCM', '2020-08-22', '2030-08-22', '{"has_chip": "yes"}', 'cccd'),
('22040001', '445566778899', 'Công an Đà Nẵng', '2017-06-30', '2027-06-30', '{"has_chip": "no"}', 'cccd'),
('23010001', '556677889900', 'Công an Hà Nội', '2023-11-12', '2033-11-12', '{"has_chip": "yes"}', 'cccd'),
('23020001', '667788990011', 'Công an TP.HCM', '2022-04-08', '2032-04-08', '{"has_chip": "no"}', 'cccd'),
('23030001', '778899001122', 'Cục Quản lý Xuất nhập cảnh', '2021-10-27', '2031-10-27', '{"country_of_issue": "Việt Nam"}', 'passport'),
('23040001', '889900112233', 'Cục Quản lý Xuất nhập cảnh', '2020-05-15', '2030-05-15', '{"country_of_issue": "Việt Nam"}', 'passport'),
('22010002', '990011223344', 'Công an Đà Nẵng', '2022-07-03', '2032-07-03', '{"has_chip": "yes"}', 'cccd'),
('22020002', '001122334455', 'Công an Hà Nội', '2021-01-17', '2031-01-17', '{"has_chip": "no"}', 'cccd'),
('22030002', '112233445567', 'Công an TP.HCM', '2020-09-21', '2030-09-21', '{"has_chip": "yes"}', 'cccd');

INSERT INTO addresses (student_id, other, village, district, city, country, type) VALUES 
('20010001', '123 Trần Hưng Đạo', 'Phường 1', 'Quận 1', 'Hà Nội', 'Việt Nam', 'thuong_tru'),
('20010001', '456 Lê Lợi', 'Phường 2', 'Quận 2', 'Hà Nội', 'Việt Nam', 'tam_tru'),
('20010001', '789 Nguyễn Huệ', 'Phường 3', 'Quận 3', 'Hà Nội', 'Việt Nam', 'nhan_thu'),

('20020001', '456 Nguyễn Huệ', 'Phường 4', 'Quận 4', 'TP. HCM', 'Việt Nam', 'thuong_tru'),
('20020001', '789 Lê Lợi', 'Phường 5', 'Quận 5', 'TP. HCM', 'Việt Nam', 'tam_tru'),

('21030001', '789 Lê Lợi', 'Phường 6', 'Quận 6', 'Đà Nẵng', 'Việt Nam', 'thuong_tru'),
('21030001', '123 Trần Hưng Đạo', 'Phường 7', 'Quận 7', 'Đà Nẵng', 'Việt Nam', 'nhan_thu'),

('21040001', '321 Bạch Đằng', 'Phường 8', 'Quận 8', 'Hải Phòng', 'Việt Nam', 'thuong_tru'),

('22010001', '654 Lê Thánh Tông', 'Phường 9', 'Quận 9', 'Cần Thơ', 'Việt Nam', 'thuong_tru'),
('22010001', '987 Hai Bà Trưng', 'Phường 10', 'Quận 10', 'Cần Thơ', 'Việt Nam', 'tam_tru'),

('22020001', '987 Hai Bà Trưng', 'Phường 11', 'Quận 11', 'Hà Nội', 'Việt Nam', 'thuong_tru'),

('22030001', '135 Võ Văn Kiệt', 'Phường 12', 'Quận 12', 'TP. HCM', 'Việt Nam', 'thuong_tru'),
('22030001', '246 Nguyễn Trãi', 'Phường 13', 'Quận 13', 'TP. HCM', 'Việt Nam', 'nhan_thu'),

('22040001', '246 Nguyễn Trãi', 'Phường 14', 'Quận 14', 'Đà Nẵng', 'Việt Nam', 'thuong_tru'),

('23010001', '369 Hoàng Văn Thụ', 'Phường 15', 'Quận 15', 'Hải Phòng', 'Việt Nam', 'thuong_tru'),

('23020001', '741 Nguyễn Văn Linh', 'Phường 16', 'Quận 16', 'Cần Thơ', 'Việt Nam', 'thuong_tru'),

('23030001', '852 Trường Chinh', 'Phường 17', 'Quận 17', 'Hà Nội', 'Việt Nam', 'thuong_tru'),

('23040001', '963 Điện Biên Phủ', 'Phường 18', 'Quận 18', 'TP. HCM', 'Việt Nam', 'thuong_tru'),

('22010002', '159 Lạc Long Quân', 'Phường 19', 'Quận 19', 'Đà Nẵng', 'Việt Nam', 'thuong_tru'),

('22020002', '357 Nguyễn Văn Cừ', 'Phường 20', 'Quận 20', 'Hải Phòng', 'Việt Nam', 'thuong_tru'),

('22030002', '468 Lê Quang Định', 'Phường 21', 'Quận 21', 'Cần Thơ', 'Việt Nam', 'thuong_tru');

------------------------ v3.0 -------------------------
CREATE TABLE configurations (
    id SERIAL PRIMARY KEY,
    type TEXT UNIQUE NOT NULL,
    value JSONB NOT NULL
);

ALTER TABLE student_statuses ADD COLUMN is_referenced BOOLEAN DEFAULT FALSE;

INSERT INTO configurations (type, value) VALUES 
('student_status_rules', '{
    "1": [2, 3, 4],
    "2": [1],
    "3": [],
    "4": [1]
}'),
('email_domains', '["student.com"]'),
('phone_countries', '["VN"]');

UPDATE student_statuses SET is_referenced = TRUE WHERE id IN (1, 2, 3, 4);

ALTER TABLE students ADD COLUMN created_at TIMESTAMP DEFAULT NOW();

ALTER TABLE configurations ADD COLUMN is_active BOOLEAN DEFAULT TRUE NOT NULL;

------------------------ v4.0 -------------------------
CREATE TABLE IF NOT EXISTS years (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS semesters (
    id SERIAL PRIMARY KEY,
    semester_num INT NOT NULL,
    year_id INT NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    
    FOREIGN KEY(year_id) 
    REFERENCES years(id)
    ON DELETE CASCADE,

    UNIQUE (semester_num, year_id)
);

CREATE TABLE IF NOT EXISTS lecturers (
    id VARCHAR(10) PRIMARY KEY,
    full_name TEXT NOT NULL,
    date_of_birth DATE NOT NULL,
    gender VARCHAR(10) NOT NULL,
    email TEXT NOT NULL UNIQUE,
    phone_number VARCHAR(15) NOT NULL,
    faculty_id INT NOT NULL,
    
    FOREIGN KEY(faculty_id) 
    REFERENCES faculties(id)
    ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS courses (
    id VARCHAR(10) PRIMARY KEY,
    name TEXT NOT NULL,
    credits INT NOT NULL CHECK (credits >= 2),
    faculty_id INT NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    is_active BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY(faculty_id) 
    REFERENCES faculties(id)
    ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS course_prerequisites (
    course_id VARCHAR(10),
    prerequisite_id VARCHAR(10),
    
    PRIMARY KEY (course_id, prerequisite_id),
    
    FOREIGN KEY (course_id)
    REFERENCES courses(id)
    ON DELETE CASCADE,
    
    FOREIGN KEY (prerequisite_id)
    REFERENCES courses(id)
    ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS classes (
    id VARCHAR(10),
    course_id VARCHAR(10) NOT NULL,
    semester_id INT NOT NULL,
    lecturer_id VARCHAR(10) NOT NULL,
    max_students INT NOT NULL CHECK (max_students > 0),
    schedule TEXT,
    room TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    is_active BOOLEAN DEFAULT TRUE,
    
    PRIMARY KEY (id, course_id, semester_id),
    
    FOREIGN KEY(course_id) 
    REFERENCES courses(id)
    ON DELETE RESTRICT,
    
    FOREIGN KEY(semester_id) 
    REFERENCES semesters(id)
    ON DELETE RESTRICT,
    
    FOREIGN KEY(lecturer_id) 
    REFERENCES lecturers(id)
    ON DELETE RESTRICT,

    UNIQUE(semester_id, schedule, room)
);

CREATE TABLE IF NOT EXISTS course_enrollments (
    student_id VARCHAR(10),
    course_id VARCHAR(10),
    class_id VARCHAR(10) NOT NULL,
    semester_id INT NOT NULL,
    grade FLOAT,
    status TEXT CHECK(status in ('failed', 'passed', 'enrolled')),
    
    PRIMARY KEY (student_id, course_id),
    
    FOREIGN KEY(student_id) 
    REFERENCES students(id)
    ON DELETE RESTRICT,
    
    FOREIGN KEY(course_id) 
    REFERENCES courses(id)
    ON DELETE RESTRICT,
    
    FOREIGN KEY(class_id, course_id, semester_id) 
    REFERENCES classes(id, course_id, semester_id)
    ON DELETE RESTRICT,
    
    FOREIGN KEY(semester_id) 
    REFERENCES semesters(id)
    ON DELETE RESTRICT,
    
    CHECK (grade IS NULL OR (grade BETWEEN 0 AND 10))
);

CREATE TABLE IF NOT EXISTS enrollment_history (
    student_id VARCHAR(10),
    created_at TIMESTAMP DEFAULT NOW(),
    course_id VARCHAR(10) NOT NULL,
    class_id VARCHAR(10) NOT NULL,
    semester_id INT NOT NULL,
    action TEXT NOT NULL CHECK(action in ('register', 'cancel')),
    
    PRIMARY KEY (student_id, created_at),
    
      FOREIGN KEY(student_id) 
      REFERENCES students(id)
      ON DELETE RESTRICT,
      
      FOREIGN KEY(class_id, course_id, semester_id) 
      REFERENCES classes(id, course_id, semester_id)
      ON DELETE RESTRICT
);

-- create triggers

CREATE OR REPLACE FUNCTION set_status_based_on_grade()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.grade IS NULL THEN
        NEW.status := 'enrolled';
    ELSIF NEW.grade < 5 THEN
        NEW.status := 'failed';
    ELSE
        NEW.status := 'passed';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_set_status_based_on_grade
BEFORE INSERT OR UPDATE ON course_enrollments
FOR EACH ROW
EXECUTE FUNCTION set_status_based_on_grade();

-- create sample data for new tables in v4.0

INSERT INTO years (name) VALUES 
('2020 - 2021'),
('2021 - 2022'),
('2022 - 2023'),
('2023 - 2024'),
('2024 - 2025');

INSERT INTO semesters (semester_num, year_id, start_date, end_date) VALUES 
(1, 1, '2020-09-01', '2021-01-15'),
(2, 1, '2021-02-01', '2021-06-15'),
(1, 2, '2021-09-01', '2022-01-15'),
(2, 2, '2022-02-01', '2022-06-15'),
(1, 3, '2022-09-01', '2023-01-15'),
(2, 3, '2023-02-01', '2023-06-15'),
(1, 4, '2023-09-01', '2024-01-15'),
(2, 4, '2024-02-01', '2024-06-15'),
(1, 5, '2024-09-01', '2025-01-15'),
(2, 5, '2025-02-01', '2025-06-15');

-- Sample data for lecturers
INSERT INTO lecturers (id, full_name, date_of_birth, gender, email, phone_number, faculty_id) VALUES 
('LEC001', 'Phạm Văn Minh', '1975-03-15', 'Nam', 'phamvanminh@faculty.edu.vn', '0912345678', 1), -- Khoa Luật
('LEC002', 'Nguyễn Thị Hương', '1980-06-22', 'Nữ', 'nguyenthihuong@faculty.edu.vn', '0923456789', 1), -- Khoa Luật
('LEC003', 'Trần Đức Thắng', '1978-11-05', 'Nam', 'tranducthang@faculty.edu.vn', '0934567890', 2), -- Khoa Tiếng Anh thương mại
('LEC004', 'Lê Thị Mai', '1982-09-18', 'Nữ', 'lethimai@faculty.edu.vn', '0945678901', 2), -- Khoa Tiếng Anh thương mại
('LEC005', 'Hoàng Minh Tuấn', '1976-07-30', 'Nam', 'hoangminhtuan@faculty.edu.vn', '0956789012', 3), -- Khoa Tiếng Nhật
('LEC006', 'Đỗ Thanh Hà', '1983-04-12', 'Nữ', 'dothanhha@faculty.edu.vn', '0967890123', 3), -- Khoa Tiếng Nhật
('LEC007', 'Vũ Quang Nam', '1979-01-25', 'Nam', 'vuquangnam@faculty.edu.vn', '0978901234', 4), -- Khoa Tiếng Pháp
('LEC008', 'Ngô Thị Lan', '1981-08-07', 'Nữ', 'ngothilan@faculty.edu.vn', '0989012345', 4); -- Khoa Tiếng Pháp

-- Sample data for courses
INSERT INTO courses (id, name, credits, faculty_id, description, created_at, is_active) VALUES 
-- Khoa Luật courses
('LAW101', 'Luật Dân sự cơ bản', 3, 1, 'Giới thiệu về các nguyên tắc cơ bản của luật dân sự Việt Nam', '2025-04-07 05:20:47', TRUE),
('LAW102', 'Luật Hình sự đại cương', 3, 1, 'Khái quát về luật hình sự và các tội phạm cơ bản', '2025-04-07 05:20:47', TRUE),
('LAW201', 'Pháp luật thương mại', 4, 1, 'Luật áp dụng trong hoạt động thương mại và kinh doanh', '2025-04-07 05:20:47', TRUE),
('LAW301', 'Luật Quốc tế', 4, 1, 'Nghiên cứu về luật pháp quốc tế và các hiệp ước', '2025-04-07 05:20:47', TRUE),

-- Khoa Tiếng Anh thương mại courses
('ENG101', 'Tiếng Anh thương mại cơ bản', 3, 2, 'Giới thiệu về từ vựng và ngữ pháp tiếng Anh trong môi trường kinh doanh', '2025-04-07 05:20:47', TRUE),
('ENG102', 'Kỹ năng giao tiếp tiếng Anh', 3, 2, 'Phát triển kỹ năng giao tiếp tiếng Anh trong môi trường làm việc', '2025-04-07 05:20:47', TRUE),
('ENG201', 'Tiếng Anh thương mại nâng cao', 4, 2, 'Nâng cao kỹ năng tiếng Anh chuyên ngành thương mại', '2025-04-07 05:20:47', TRUE),
('ENG301', 'Tiếng Anh đàm phán kinh doanh', 4, 2, 'Kỹ năng đàm phán kinh doanh bằng tiếng Anh', '2025-04-07 05:20:47', TRUE),

-- Khoa Tiếng Nhật courses
('JPN101', 'Tiếng Nhật cơ bản 1', 3, 3, 'Nhập môn tiếng Nhật với các kỹ năng nghe, nói, đọc, viết cơ bản', '2025-04-07 05:20:47', TRUE),
('JPN102', 'Tiếng Nhật cơ bản 2', 3, 3, 'Tiếp tục phát triển kỹ năng tiếng Nhật cơ bản', '2025-04-07 05:20:47', TRUE),
('JPN201', 'Tiếng Nhật trung cấp', 4, 3, 'Phát triển kỹ năng tiếng Nhật ở mức độ trung cấp', '2025-04-07 05:20:47', TRUE),
('JPN301', 'Tiếng Nhật thương mại', 4, 3, 'Tiếng Nhật chuyên ngành thương mại và kinh doanh', '2025-04-07 05:20:47', TRUE),

-- Khoa Tiếng Pháp courses
('FRE101', 'Tiếng Pháp cơ bản 1', 3, 4, 'Nhập môn tiếng Pháp với các kỹ năng nghe, nói, đọc, viết cơ bản', '2025-04-07 05:20:47', TRUE),
('FRE102', 'Tiếng Pháp cơ bản 2', 3, 4, 'Tiếp tục phát triển kỹ năng tiếng Pháp cơ bản', '2025-04-07 05:20:47', TRUE),
('FRE201', 'Tiếng Pháp trung cấp', 4, 4, 'Phát triển kỹ năng tiếng Pháp ở mức độ trung cấp', '2025-04-07 05:20:47', TRUE),
('FRE301', 'Tiếng Pháp thương mại', 4, 4, 'Tiếng Pháp chuyên ngành thương mại và kinh doanh', '2025-04-07 05:20:47', TRUE);

-- Sample data for course_prerequisites
INSERT INTO course_prerequisites (course_id, prerequisite_id) VALUES 
-- Law prerequisites
('LAW201', 'LAW101'), -- Pháp luật thương mại requires Luật Dân sự cơ bản
('LAW301', 'LAW201'), -- Luật Quốc tế requires Pháp luật thương mại

-- English prerequisites
('ENG102', 'ENG101'), -- Kỹ năng giao tiếp tiếng Anh requires Tiếng Anh thương mại cơ bản
('ENG201', 'ENG102'), -- Tiếng Anh thương mại nâng cao requires Kỹ năng giao tiếp tiếng Anh
('ENG301', 'ENG201'), -- Tiếng Anh đàm phán kinh doanh requires Tiếng Anh thương mại nâng cao

-- Japanese prerequisites
('JPN102', 'JPN101'), -- Tiếng Nhật cơ bản 2 requires Tiếng Nhật cơ bản 1
('JPN201', 'JPN102'), -- Tiếng Nhật trung cấp requires Tiếng Nhật cơ bản 2
('JPN301', 'JPN201'), -- Tiếng Nhật thương mại requires Tiếng Nhật trung cấp

-- French prerequisites
('FRE102', 'FRE101'), -- Tiếng Pháp cơ bản 2 requires Tiếng Pháp cơ bản 1
('FRE201', 'FRE102'), -- Tiếng Pháp trung cấp requires Tiếng Pháp cơ bản 2
('FRE301', 'FRE201'); -- Tiếng Pháp thương mại requires Tiếng Pháp trung cấp

-- Sample data for classes
INSERT INTO classes (id, course_id, semester_id, lecturer_id, max_students, schedule, room, created_at, is_active) VALUES 
-- Classes for 2023-2024 academic year, semester 1 (semester_id = 7)
('C001', 'LAW101', 7, 'LEC001', 40, 'Thứ 2, 7:30-9:30', 'A101', '2025-04-07 05:20:47', TRUE),
('C002', 'LAW102', 7, 'LEC002', 35, 'Thứ 3, 13:30-15:30', 'A102', '2025-04-07 05:20:47', TRUE),
('C003', 'ENG101', 7, 'LEC003', 30, 'Thứ 4, 9:30-11:30', 'B201', '2025-04-07 05:20:47', TRUE),
('C004', 'ENG102', 7, 'LEC004', 30, 'Thứ 5, 7:30-9:30', 'B202', '2025-04-07 05:20:47', TRUE),
('C005', 'JPN101', 7, 'LEC005', 25, 'Thứ 6, 13:30-15:30', 'C301', '2025-04-07 05:20:47', TRUE),
('C006', 'FRE101', 7, 'LEC007', 25, 'Thứ 7, 9:30-11:30', 'C302', '2025-04-07 05:20:47', TRUE),

-- Classes for 2023-2024 academic year, semester 2 (semester_id = 8)
('C007', 'LAW201', 8, 'LEC001', 35, 'Thứ 2, 9:30-11:30', 'A103', '2025-04-07 05:20:47', TRUE),
('C008', 'ENG201', 8, 'LEC003', 25, 'Thứ 3, 7:30-9:30', 'B203', '2025-04-07 05:20:47', TRUE),
('C009', 'JPN102', 8, 'LEC006', 20, 'Thứ 4, 13:30-15:30', 'C303', '2025-04-07 05:20:47', TRUE),
('C010', 'FRE102', 8, 'LEC008', 20, 'Thứ 5, 9:30-11:30', 'C304', '2025-04-07 05:20:47', TRUE),

-- Classes for 2024-2025 academic year, semester 1 (semester_id = 9)
('C011', 'LAW301', 9, 'LEC002', 30, 'Thứ 6, 7:30-9:30', 'A104', '2025-04-07 05:20:47', TRUE),
('C012', 'ENG301', 9, 'LEC004', 25, 'Thứ 7, 13:30-15:30', 'B204', '2025-04-07 05:20:47', TRUE),
('C013', 'JPN201', 9, 'LEC005', 20, 'Thứ 2, 15:30-17:30', 'C305', '2025-04-07 05:20:47', TRUE),
('C014', 'FRE201', 9, 'LEC007', 20, 'Thứ 3, 15:30-17:30', 'C306', '2025-04-07 05:20:47', TRUE),

-- Classes for 2024-2025 academic year, semester 2 (semester_id = 10)
('C015', 'JPN301', 10, 'LEC006', 15, 'Thứ 4, 15:30-17:30', 'C307', '2025-04-07 05:20:47', TRUE),
('C016', 'FRE301', 10, 'LEC008', 15, 'Thứ 5, 15:30-17:30', 'C308', '2025-04-07 05:20:47', TRUE);

-- Sample data for course_enrollments
INSERT INTO course_enrollments (student_id, course_id, class_id, semester_id, grade) VALUES 
-- Student 20010001 (Nguyễn Văn An) enrollments - Law student
('20010001', 'LAW101', 'C001', 7, 3.4),
('20010001', 'LAW102', 'C002', 7, 7.8),
('20010001', 'LAW201', 'C007', 8, 8.2),
('20010001', 'LAW301', 'C011', 9, NULL),

-- Student 20020001 (Trần Thị Bông) enrollments - English student
('20020001', 'ENG101', 'C003', 7, 9.0),
('20020001', 'ENG102', 'C004', 7, 8.7),
('20020001', 'ENG201', 'C008', 8, 4.9),
('20020001', 'ENG301', 'C012', 9, NULL),

-- Student 21030001 (Lê Văn Cẩn) enrollments - Japanese student
('21030001', 'JPN101', 'C005', 7, 7.5),
('21030001', 'JPN102', 'C009', 8, 8.0),
('21030001', 'JPN201', 'C013', 9, NULL),

-- Student 21040001 (Phạm Minh Dũng) enrollments - French student
('21040001', 'FRE101', 'C006', 7, 6.5),
('21040001', 'FRE102', 'C010', 8, 7.2),

-- Student 22010001 (Hoàng Thị Én) enrollments - Law student
('22010001', 'LAW101', 'C001', 7, 9.5),
('22010001', 'LAW102', 'C002', 7, 9.2),
('22010001', 'LAW201', 'C007', 8, NULL);

-- Sample data for enrollment_history
INSERT INTO enrollment_history (student_id, created_at, course_id, class_id, semester_id, action) VALUES 
-- Student 20010001 (Nguyễn Văn An) history
('20010001', '2023-08-15 09:30:00', 'LAW101', 'C001', 7, 'register'),
('20010001', '2023-08-15 09:45:00', 'LAW102', 'C002', 7, 'register'),
('20010001', '2024-01-10 14:20:00', 'LAW201', 'C007', 8, 'register'),
('20010001', '2024-08-20 10:15:00', 'LAW301', 'C011', 9, 'register'),

-- Student 20020001 (Trần Thị Bông) history
('20020001', '2023-08-16 11:20:00', 'ENG101', 'C003', 7, 'register'),
('20020001', '2023-08-16 11:30:00', 'ENG102', 'C004', 7, 'register'),
('20020001', '2024-01-12 15:45:00', 'ENG201', 'C008', 8, 'register'),
('20020001', '2024-08-18 09:30:00', 'ENG301', 'C012', 9, 'register'),

-- Student 21030001 (Lê Văn Cẩn) history
('21030001', '2023-08-17 10:10:00', 'JPN101', 'C005', 7, 'register'),
('21030001', '2023-08-19 14:25:00', 'LAW101', 'C001', 7, 'register'), -- Registered for Law class
('21030001', '2023-08-25 16:30:00', 'LAW101', 'C001', 7, 'cancel'), -- Cancelled Law class
('21030001', '2024-01-15 13:40:00', 'JPN102', 'C009', 8, 'register'),
('21030001', '2024-08-19 11:05:00', 'JPN201', 'C013', 9, 'register'),

-- Student 21040001 (Phạm Minh Dũng) history
('21040001', '2023-08-18 13:15:00', 'FRE101', 'C006', 7, 'register'),
('21040001', '2024-01-16 10:50:00', 'FRE102', 'C010', 8, 'register'),
('21040001', '2024-06-20 09:00:00', 'FRE102', 'C010', 8, 'register'); -- Registering again for the failed course