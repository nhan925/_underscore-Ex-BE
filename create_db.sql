
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