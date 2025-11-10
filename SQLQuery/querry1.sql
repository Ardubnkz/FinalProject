CREATE DATABASE hotel_booking_db;
USE hotel_booking_db;
CREATE TABLE users(
user_id INT AUTO_INCREMENT PRIMARY KEY,
name VARCHAR(100) NOT NULL,
email VARCHAR(100) UNIQUE NOT NULL,
password_hash VARCHAR(225) not null);
CREATE TABLE rooms(
room_id INT AUTO_INCREMENT PRIMARY KEY,
room_number varchar(50) NOT NULL,
room_type VARCHAR(50) NOT NULL, 
price DECIMAL(10,2) NOT NULL,
bed_type VARCHAR(50),
description TEXT);
CREATE TABLE bookings( booking_id INT AUTO_INCREMENT PRIMARY KEY,
user_id INT,
room_id INT,
check_in_date DATE,
check_out_date DATE,
FOREIGN KEY (user_id) REFERENCES users(user_id),
FOREIGN KEY (room_id) REFERENCES rooms(room_id));
ALTER TABLE users ADD COLUMN role VARCHAR(20) DEFAULT 'user';
UPDATE users SET role='admin' WHERE email='admin@hotel.com';