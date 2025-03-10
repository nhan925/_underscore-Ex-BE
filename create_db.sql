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
    FOREIGN KEY (faculty_id) REFERENCES faculties(id) ON DELETE CASCADE,
    FOREIGN KEY (status_id) REFERENCES student_statuses(id) ON DELETE CASCADE
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
INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program, address, email, phone_number, status_id) VALUES 
('20010001', 'Nguyễn Văn Anh', '2002-05-10', 'Nam', 1, 2020, 'Luật Dân sự', '123 Trần Hưng Đạo, Hà Nội', 'nguyenvana@student.com', '0987654321', 1),
('20020002', 'Trần Thị Bông', '2001-09-15', 'Nữ', 2, 2020, 'Tiếng Anh Thương mại', '456 Nguyễn Huệ, TP. HCM', 'tranthib@student.com', '0971234567', 1),
('21030003', 'Lê Văn Cẩn', '2003-02-20', 'Nam', 3, 2021, 'Tiếng Nhật', '789 Lê Lợi, Đà Nẵng', 'levanc@student.com', '0969876543', 1),
('21040004', 'Phạm Minh Dũng', '2000-07-25', 'Nam', 4, 2021, 'Tiếng Pháp', '321 Bạch Đằng, Hải Phòng', 'phamminhd@student.com', '0951112233', 2),
('22010005', 'Hoàng Thị Én', '1999-12-05', 'Nữ', 1, 2022, 'Luật Hình sự', '654 Lê Thánh Tông, Cần Thơ', 'hoangthie@student.com', '0942223344', 2),
('22020006', 'Đặng Quốc Phương', '2002-03-18', 'Nam', 2, 2022, 'Tiếng Anh Kinh doanh', '987 Hai Bà Trưng, Hà Nội', 'dangquocf@student.com', '0933334455', 1),
('22030007', 'Lý Thu Giang', '2001-08-22', 'Nữ', 3, 2022, 'Tiếng Nhật Thực hành', '135 Võ Văn Kiệt, TP. HCM', 'lythug@student.com', '0924445566', 3),
('22040008', 'Bùi Minh Huy', '2000-06-30', 'Nam', 4, 2022, 'Tiếng Pháp Chuyên sâu', '246 Nguyễn Trãi, Đà Nẵng', 'buiminhh@student.com', '0915556677', 3),
('23010009', 'Đỗ Hồng Lĩnh', '2003-11-12', 'Nữ', 1, 2023, 'Luật Quốc tế', '369 Hoàng Văn Thụ, Hải Phòng', 'dohongi@student.com', '0906667788', 1),
('23020010', 'Vũ Thanh Hoa', '2002-04-08', 'Nữ', 2, 2023, 'Tiếng Anh Hành chính', '741 Nguyễn Văn Linh, Cần Thơ', 'vuthanhj@student.com', '0897778899', 1),
('23030011', 'Ngô Phúc Kiến', '2001-10-27', 'Nam', 3, 2023, 'Tiếng Nhật Doanh nghiệp', '852 Trường Chinh, Hà Nội', 'ngophuck@student.com', '0888889900', 4),
('23040012', 'Trịnh Minh Lân', '2000-05-15', 'Nam', 4, 2023, 'Tiếng Pháp Ứng dụng', '963 Điện Biên Phủ, TP. HCM', 'trinhminhl@student.com', '0879990011', 2),
('22010013', 'Cao Thanh Mai', '2003-07-03', 'Nữ', 1, 2022, 'Luật Thương mại', '159 Lạc Long Quân, Đà Nẵng', 'caothanhm@student.com', '0860001122', 1),
('22020014', 'Đinh Trọng Nam', '2002-01-17', 'Nam', 2, 2022, 'Tiếng Anh Luật', '357 Nguyễn Văn Cừ, Hải Phòng', 'dinhtrongn@student.com', '0851112233', 3),
('22030015', 'Hà Quốc Tuấn', '2001-09-21', 'Nam', 3, 2022, 'Tiếng Nhật Phiên dịch', '468 Lê Quang Định, Cần Thơ', 'haquoco@student.com', '0842223344', 1),
('22040016', 'Phùng Minh Phương', '2000-12-10', 'Nữ', 4, 2022, 'Tiếng Pháp Văn hóa', '579 Nam Kỳ Khởi Nghĩa, Hà Nội', 'phungminhp@student.com', '0833334455', 2),
('23010017', 'Tạ Hoàng Quang', '2003-06-25', 'Nam', 1, 2023, 'Luật Hiến pháp', '680 Tôn Đức Thắng, TP. HCM', 'tahoangq@student.com', '0824445566', 1),
('23020018', 'Nguyễn Nhật Linh', '2002-08-19', 'Nam', 2, 2023, 'Tiếng Anh Truyền thông', '791 Phan Chu Trinh, Đà Nẵng', 'nguyennhatr@student.com', '0815556677', 1),
('23030019', 'Đỗ Huy Sáng', '2001-11-30', 'Nữ', 3, 2023, 'Tiếng Nhật Dịch thuật', '802 Nguyễn Công Trứ, Hải Phòng', 'dohuys@student.com', '0806667788', 4),
('23040020', 'Lương Khánh Trân', '2000-04-02', 'Nữ', 4, 2023, 'Tiếng Pháp Kinh tế', '913 Quang Trung, Cần Thơ', 'luongkhanht@student.com', '0797778899', 2);
