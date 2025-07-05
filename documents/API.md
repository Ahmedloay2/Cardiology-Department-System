# Cardiology Department API Documentation

## Overview
API for Cardiology Department System - Version v1

**Base URL**: `/api`  
**Authentication**: Bearer Token (JWT)

---

## 🔐 Authentication

All endpoints require JWT Bearer token authentication except login and registration endpoints.

**Header Format:**
```
Authorization: Bearer <your_jwt_token>
```

---

## 👥 User Roles & Endpoints

### 🔴 Admin Endpoints
Administrative functions for system management

### 🔵 Doctor Endpoints  
Medical staff operations and profile management

### 🟢 Patient Endpoints
Patient registration, profile management, and health records

### 🟡 Message Endpoints
Communication system between doctors and patients

### 🗓️ Appointment Endpoints
Appointment booking, management, and tracking system

### 📋 Report Endpoints
Medical report creation, updates, and retrieval system

---

## 📋 Endpoint Reference

### 🔴 Admin Operations

#### **POST** `/api/Admin/Login`
**Purpose**: Admin authentication  
**Content-Type**: `application/json`

**📥 Input:**
```json
{
  "email": "admin@hospital.com",
  "password": "SecurePass123!"
}
```

**📤 Output:** `200 OK` - Authentication successful

---

#### **GET** `/api/Admin/Profile`
**Purpose**: Retrieve admin profile information

**📥 Input Parameters:**
- `ID` (query, integer) - Admin ID

**📤 Output:** `200 OK` - Admin profile data

---

#### **PUT** `/api/Admin/Profile`
**Purpose**: Update admin profile  
**Content-Type**: `multipart/form-data`

**📥 Input:**
```
FName: string (2-50 chars, pattern: ^.{2,50}$)
LName: string (2-50 chars, pattern: ^.{2,50}$)
BirthDate: datetime
Email: string (email format, pattern: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$)
PhoneNumbers: array of strings
Address: string (max 500 chars, pattern: ^.{0,500}$)
PhotoData: binary file
Gender: string
```

**📤 Output:** `200 OK` - Profile updated successfully

---

#### **POST** `/api/Admin/CreateAdmin`
**Purpose**: Create new admin account  
**Content-Type**: `multipart/form-data`

**📥 Input (Required Fields):**
```
✅ FName: string (2-50 chars)
✅ LName: string (2-50 chars)  
✅ BirthDate: datetime
✅ Password: string (8-100 chars, complex pattern)
✅ Email: string (email format)
✅ PhoneNumbers: array of strings
✅ Gender: string

Optional:
Address: string (max 500 chars)
Photo: binary file
```

**🔒 Password Requirements:**
- 8-100 characters
- At least one lowercase letter
- At least one uppercase letter  
- At least one digit
- At least one special character

**📤 Output:** `200 OK` - Admin created successfully

---

#### **POST** `/api/Admin/Logout`
**Purpose**: Admin logout

**📤 Output:** `200 OK` - Logout successful

---

#### **DELETE** `/api/Admin/Delete`
**Purpose**: Delete admin account

**📤 Output:** `200 OK` - Account deleted

---

#### **GET** `/api/Admin/AdminProfilesList`
**Purpose**: Get list of admin profiles with search functionality

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of admin profiles

---

#### **GET** `/api/Admin/AdminsList`
**Purpose**: Get simplified list of admins

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number  
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of admins

---

### 🔵 Doctor Operations

#### **POST** `/api/Doctor/CreateDoctor`
**Purpose**: Create new doctor account  
**Content-Type**: `multipart/form-data`

**📥 Input (Required Fields):**
```
✅ FName: string (2-50 chars)
✅ LName: string (2-50 chars)
✅ BirthDate: datetime
✅ Password: string (8-100 chars, complex pattern)
✅ Email: string (email format, max 100 chars)
✅ Position: string (max 50 chars)
✅ Gender: string
✅ PhoneNumbers: array of strings
✅ Salary: float (6000-50000)

Optional:
YearsOfExperience: integer (1-50)
Address: string (max 500 chars)
Photo: binary file
```

**💰 Salary Range:** 6,000 - 50,000  
**👨‍⚕️ Experience:** 1-50 years

**📤 Output:** `200 OK` - Doctor created successfully

---

#### **POST** `/api/Doctor/Login`
**Purpose**: Doctor authentication  
**Content-Type**: `application/json`

**📥 Input:**
```json
{
  "email": "doctor@hospital.com",
  "password": "SecurePass123!"
}
```

**📤 Output:** `200 OK` - Authentication successful

---

#### **GET** `/api/Doctor/Profile`
**Purpose**: Retrieve doctor profile

**📥 Input Parameters:**
- `ID` (query, integer) - Doctor ID

**📤 Output:** `200 OK` - Doctor profile data

---

#### **PUT** `/api/Doctor/UpdateProfile`
**Purpose**: Update doctor profile  
**Content-Type**: `multipart/form-data`

**📥 Input:**
```
FName: string (2-50 chars)
LName: string (2-50 chars)
BirthDate: datetime
Email: string (email format, max 100 chars)
Position: string (max 50 chars)
YearsOfExperience: integer (1-50)
Gender: string
Address: string (max 500 chars)
PhoneNumbers: array of strings
PhotoData: binary file
Salary: float (6000-50000)
```

**📤 Output:** `200 OK` - Profile updated successfully

---

#### **POST** `/api/Doctor/Logout`
**Purpose**: Doctor logout

**📤 Output:** `200 OK` - Logout successful

---

#### **DELETE** `/api/Doctor/Delete`
**Purpose**: Delete doctor account

**📤 Output:** `200 OK` - Account deleted

---

#### **GET** `/api/Doctor/DoctorProfilesList`
**Purpose**: Get list of doctor profiles with search functionality

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of doctor profiles

---

#### **GET** `/api/Doctor/DoctorsList`
**Purpose**: Get simplified list of doctors

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number  
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of doctors

---

### 🟢 Patient Operations

#### **POST** `/api/Patient/Register`
**Purpose**: Patient registration  
**Content-Type**: `multipart/form-data`

**📥 Input (Required Fields):**
```
✅ FName: string (2-50 chars)
✅ LName: string (2-50 chars)
✅ BirthDate: datetime
✅ Gender: string
✅ Password: string (8-100 chars, complex pattern)
✅ Email: string (email format, max 100 chars)
✅ Address: string
✅ EmergencyContactName: string (max 100 chars)
✅ EmergencyContactPhone: string (Egyptian phone format)
✅ PhoneNumbers: array of strings
✅ ParentName: string
✅ Link: string

Optional Medical Information:
SpouseName: string
BloodType: string
Allergies: string
ChronicConditions: string
PreviousSurgeries: string
CurrentMedications: string

Optional Insurance Information:
PolicyNumber: string
InsuranceProvider: string
PolicyValidDate: datetime

Optional:
LandLine: string (tel format)
Photo: binary file
```

**📱 Phone Format:** Egyptian mobile numbers (`^(?:\+20|0)?1[0125]\d{8}$`)

**📤 Output:** `200 OK` - Patient registered successfully

---

#### **POST** `/api/Patient/Login`
**Purpose**: Patient authentication  
**Content-Type**: `application/json`

**📥 Input:**
```json
{
  "email": "patient@email.com",
  "password": "SecurePass123!"
}
```

**📤 Output:** `200 OK` - Authentication successful

---

#### **GET** `/api/Patient/Profile`
**Purpose**: Retrieve patient profile

**📥 Input Parameters:**
- `ID` (query, integer) - Patient ID

**📤 Output:** `200 OK` - Patient profile data

---

#### **PUT** `/api/Patient/UpdateProfile`
**Purpose**: Update patient profile  
**Content-Type**: `multipart/form-data`

**📥 Input:**
```
Personal Information:
FName: string (2-50 chars, pattern: ^.{2,50}$)
LName: string (2-50 chars, pattern: ^.{2,50}$)
BirthDate: datetime
Email: string (1-100 chars, email format)
Gender: string (max 20 chars)
Address: string (max 500 chars)
PhoneNumbers: array of strings
LandLine: string (max 20 chars)

Family Information:
ParentName: string (max 100 chars)
SpouseName: string (max 100 chars)

Emergency Contact:
EmergencyContactName: string (2-50 chars)
EmergencyContactPhone: string (Egyptian phone format)

Medical Information:
BloodType: string (pattern: ^(A|B|AB|O)[+-]$)
Allergies: string (max 255 chars)
ChronicConditions: string (max 255 chars)
PreviousSurgeries: string (max 255 chars)
CurrentMedications: string (max 255 chars)

Insurance Information:
PolicyNumber: string (max 50 chars)
InsuranceProvider: string (max 100 chars)
PolicyValidDate: datetime

Other:
Link: string (max 255 chars)
PhotoData: binary file
```

**🩸 Blood Type Format:** A+, A-, B+, B-, AB+, AB-, O+, O-

**📤 Output:** `200 OK` - Profile updated successfully

---

#### **POST** `/api/Patient/Logout`
**Purpose**: Patient logout

**📤 Output:** `200 OK` - Logout successful

---

#### **DELETE** `/api/Patient/Delete`
**Purpose**: Delete patient account

**📤 Output:** `200 OK` - Account deleted

---

#### **GET** `/api/Patient/PatientProfilesList`
**Purpose**: Get list of patient profiles with search functionality

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of patient profiles

---

#### **GET** `/api/Patient/PatientsList`
**Purpose**: Get simplified list of patients

**📥 Input Parameters:**
- `name` (query, string) - Search by name
- `pagenumber` (query, integer, default: 1) - Page number  
- `exactmatch` (query, boolean, default: false) - Exact name match

**📤 Output:** `200 OK` - List of patients

---

### 🟡 Message Operations

#### **GET** `/api/Message/GetMessages`
**Purpose**: Retrieve message conversation between patient and doctor

**📥 Input Parameters:**
- `patientid` (query, integer) - Patient ID
- `doctorid` (query, integer) - Doctor ID

**📤 Output:** `200 OK` - Message conversation history

---

#### **POST** `/api/Message/SendMessage`
**Purpose**: Send message to another user  
**Content-Type**: `application/json`

**📥 Input Parameters:**
- `reciverID` (query, integer) - Recipient ID

**📥 Request Body:**
```json
"Your message content here"
```

**📤 Output:** `200 OK` - Message sent successfully

---

#### **DELETE** `/api/Message/Delete`
**Purpose**: Delete a specific message

**📥 Input Parameters:**
- `messageId` (query, integer) - Message ID to delete

**📤 Output:** `200 OK` - Message deleted successfully

---

### 🗓️ Appointment Operations

#### **POST** `/api/Appointment/BookAppointment`
**Purpose**: Create a new appointment between patient and doctor  
**Content-Type**: `application/json`  
**🔒 Authorization**: Patient role required

**📥 Input:**
```json
{
  "AppDate": "2025-07-15T10:30:00",
  "DoctorID": 123
}
```

**📤 Responses:**
- `200 OK` - Appointment created successfully
- `400 Bad Request` - Invalid input data or time slot unavailable

**📝 Notes:**
- Patient ID is automatically extracted from JWT token
- Appointment date must be in the future
- Time slot must be available for the specified doctor

---

#### **GET** `/api/Appointment/GetAppointments`
**Purpose**: Retrieve all confirmed appointments for a specific day

**📥 Input Parameters:**
- `RequestedDate` (query, datetime, required) - Date to retrieve appointments for
- `ID` (query, integer, optional) - Doctor/Patient ID (auto-extracted for authenticated users)
- `IsPatient` (query, boolean, optional) - true for patient appointments, false for doctor appointments

**📤 Responses:**
- `200 OK` - List of appointments for the specified date
- `400 Bad Request` - Invalid parameters or error occurred

**📝 Notes:**
- For authenticated doctors/patients, ID is auto-extracted from JWT token
- For admin users, both ID and IsPatient parameters are required
- Returns empty list if no appointments found

---

#### **GET** `/api/Appointment/GetAppointment`
**Purpose**: Retrieve a specific appointment at exact date and time

**📥 Input Parameters:**
- `RequestedDate` (query, datetime, required) - Exact appointment date and time
- `ID` (query, integer, optional) - Doctor/Patient ID (auto-extracted for authenticated users)
- `IsPatient` (query, boolean, optional) - true for patient appointment, false for doctor appointment

**📤 Responses:**
- `200 OK` - Appointment details at specified time
- `400 Bad Request` - No appointment found or invalid parameters

**📝 Notes:**
- Requires exact date and time match
- For authenticated users, ID is auto-extracted from JWT token

---

#### **POST** `/api/Appointment/RescheduleAppointment`
**Purpose**: Reschedule an existing appointment to a new date and time  
**Content-Type**: `application/json`  
**🔒 Authorization**: Patient role required

**📥 Input:**
```json
{
  "AppDate": "2025-07-15T10:30:00",
  "NewDate": "2025-07-20T14:00:00",
  "DoctorID": 123
}
```

**📤 Responses:**
- `200 OK` - Appointment rescheduled successfully
- `400 Bad Request` - Invalid input, original appointment not found, or new time unavailable

**📝 Notes:**
- Only the original patient can reschedule their appointment
- Original appointment is marked as "Postponed"
- New appointment date must be in the future and available

---

#### **POST** `/api/Appointment/CancelAppointment`
**Purpose**: Cancel an existing appointment  
**Content-Type**: `application/json`  
**🔒 Authorization**: Patient role required

**📥 Input:**
```json
{
  "AppDate": "2025-07-15T10:30:00",
  "DoctorID": 123
}
```

**📤 Responses:**
- `200 OK` - Appointment cancelled successfully
- `400 Bad Request` - Invalid input, appointment not found, or unauthorized access

**📝 Notes:**
- Only the original patient can cancel their appointment
- Appointment status is changed to "Cancelled"

---

#### **POST** `/api/Appointment/MarkAppointment`
**Purpose**: Mark appointment as completed or missed by the doctor  
**🔒 Authorization**: Doctor role required

**📥 Input Parameters:**
- `AppointmentId` (query, integer, required) - Appointment ID to mark
- `IsCompleted` (query, boolean, required) - true for completed, false for missed

**📤 Responses:**
- `200 OK` - Appointment marked successfully
- `400 Bad Request` - Invalid input, appointment not found, or unauthorized access

**📝 Notes:**
- Only the assigned doctor can mark their appointments
- Can only mark appointments after their scheduled time
- Status changes to "Completed" or "Missed" based on IsCompleted parameter

---

### 📋 Report Operations

#### **POST** `/api/Report/CreateReport`
**Purpose**: Create a new medical report for a patient  
**Content-Type**: `application/json`  
**🔒 Authorization**: Doctor role required

**📥 Input:**
```json
{
  "PatientID": 456,
  "AppointmentID": 123,
  "Diagnosis": "Hypertension",
  "Treatment": "Prescribed ACE inhibitors",
  "Notes": "Patient shows improvement",
  "RecommendedFollowUp": "2025-08-15T10:00:00"
}
```

**📤 Responses:**
- `200 OK` - Report created successfully
- `400 Bad Request` - Invalid input data or model validation failed

**📝 Notes:**
- Doctor ID is automatically extracted from JWT token
- All required fields must be provided in the ReportDto
- Report is linked to a specific appointment and patient

---

#### **POST** `/api/Report/UpdateReport`
**Purpose**: Update an existing medical report  
**Content-Type**: `application/json`  
**🔒 Authorization**: Doctor role required

**📥 Input:**
```json
{
  "ReportID": 789,
  "PatientID": 456,
  "AppointmentID": 123,
  "Diagnosis": "Hypertension - Controlled",
  "Treatment": "Continue current medication",
  "Notes": "Patient shows significant improvement",
  "RecommendedFollowUp": "2025-09-15T10:00:00"
}
```

**📤 Responses:**
- `200 OK` - Report updated successfully
- `200 OK` - "There is no thing to update" (if no changes detected)
- `400 Bad Request` - Invalid input data or model validation failed

**📝 Notes:**
- Doctor ID is automatically extracted from JWT token
- Only the doctor who created the report can update it
- Returns success message even if no changes were made

---

#### **GET** `/api/Report/GetReport`
**Purpose**: Retrieve a medical report by appointment ID  
**🔒 Authorization**: Required (All authenticated users)

**📥 Input Parameters:**
- `appointmentID` (query, integer, required) - Appointment ID associated with the report (must be > 0)

**📤 Responses:**
- `200 OK` - Report data retrieved successfully
- `400 Bad Request` - Invalid appointment ID or report not found

**📝 Notes:**
- Appointment ID must be a positive integer
- Returns complete report details including diagnosis, treatment, and notes
- Accessible to all authenticated users (Admin, Doctor, Patient)

---

## 🔒 Security Requirements

### Password Complexity
All passwords must meet the following criteria:
- **Length:** 8-100 characters
- **Lowercase:** At least one lowercase letter (a-z)
- **Uppercase:** At least one uppercase letter (A-Z)  
- **Digit:** At least one number (0-9)
- **Special Character:** At least one non-alphanumeric character

**Pattern:** `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$`

### Email Validation
- **Format:** Standard email format
- **Pattern:** `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$`
- **Length:** Maximum 100 characters

### Phone Number Format (Egyptian)
- **Pattern:** `^(?:\+20|0)?1[0125]\d{8}$`
- **Examples:** 
  - `+201012345678`
  - `01012345678`
  - `1012345678`

---

## 📊 Data Constraints

### Character Limits
- **Names (First/Last):** 2-50 characters
- **Address:** Maximum 500 characters  
- **Position:** Maximum 50 characters
- **Medical Fields:** Maximum 255 characters
- **Insurance Provider:** Maximum 100 characters
- **Policy Number:** Maximum 50 characters

### Numeric Constraints
- **Doctor Salary:** 6,000 - 50,000
- **Years of Experience:** 1-50 years
- **Blood Type:** A+, A-, B+, B-, AB+, AB-, O+, O-
- **Appointment ID:** Must be positive integer (> 0)

### Appointment Constraints
- **Future Dates Only:** All appointment dates must be in the future
- **Time Slot Availability:** Each doctor can only have one appointment per time slot
- **Status Values:** Confirmed, Cancelled, Postponed, Completed, Missed

### Report Constraints
- **Doctor Authorization:** Only doctors can create and update reports
- **Appointment Association:** Each report must be linked to a valid appointment
- **Data Integrity:** All report fields must follow ReportDto model validation

---

## 🎨 Legend

- 🔴 **Admin Functions** - System administration
- 🔵 **Doctor Functions** - Medical staff operations  
- 🟢 **Patient Functions** - Patient services
- 🟡 **Message Functions** - Communication system
- 🗓️ **Appointment Functions** - Appointment management system
- 📋 **Report Functions** - Medical report management system
- 📥 **Input** - Request data/parameters
- 📤 **Output** - Response data
- ✅ **Required Field** - Must be provided
- 🔒 **Security Feature** - Authentication/validation
- 💰 **Financial Data** - Salary information
- 👨‍⚕️ **Professional Data** - Medical credentials
- 📱 **Contact Data** - Phone/communication info
- 🩸 **Medical Data** - Health information

---

## 🚀 Quick Start

1. **Register/Login** as Admin, Doctor, or Patient
2. **Obtain JWT token** from login response
3. **Include token** in Authorization header for protected endpoints
4. **Use appropriate endpoints** based on your role
5. **Follow data validation rules** for successful requests

---

## ⚠️ Important Notes

- All endpoints return `200 OK` on success
- JWT tokens are required for authenticated endpoints
- File uploads use `multipart/form-data`
- Login requests use `application/json`
- Phone numbers must follow Egyptian format for patients
- Passwords must meet complexity requirements
- Email addresses must be unique across the system
- Appointment dates must be in the future
- Only assigned users can modify their own appointments
- Doctors can only mark appointments after the scheduled time
- Medical reports can only be created and updated by doctors
- Reports are permanently linked to specific appointments and patients