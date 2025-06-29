# HR System Data Model Reference

This document lists all major tables (entities) in the HR system, their main properties, and typical use cases.

---

## Announcement

- **Purpose:** Shares important news or updates with everyone in the company.
- **Main Properties:**
  - Announcement_ID, Title, Content, DatePosted, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Communication, engagement

## Application

- **Purpose:** Stores job applications submitted by candidates who have applied for jobs.
- **Main Properties:**
  - Application_ID, CandidateName, CandidateEmail, AppliedDate, Status, JobPosting_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Recruitment, candidate tracking

## Attendance

- **Purpose:** Records when employees come to work and leave each day.
- **Main Properties:**
  - Attendance_ID, Date, Status, TimeIn, TimeOut, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Time tracking, payroll calculation, compliance

## Award

- **Purpose:** Records awards or recognitions given to employees for their achievements.
- **Main Properties:**
  - Award_ID, Title, Description, DateAwarded, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Recognition, engagement

## Benefit

- **Purpose:** Manages benefits (like health insurance) that employees receive from the company.
- **Main Properties:**
  - Benefit_ID, BenefitType, Provider, PolicyNumber, EnrollmentDate, EndDate, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Benefits enrollment, provider integration, compliance

## DEIResource

- **Purpose:** Provides resources and information to support diversity, equity, and inclusion in the workplace.
- **Main Properties:**
  - DEIResource_ID, Title, Description, ResourceType, Link, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - DEI initiatives, employee support

## Department

- **Purpose:** Groups employees into teams or sections within the company, like HR, IT, or Sales.
- **Main Properties:**
  - Department_ID, Department_Name, Department_Location, ManagerID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Department assignment, reporting, analytics

## Employee

- **Purpose:** Stores information about each employee in the company, such as their name, contact details, job, and department.
- **Main Properties:**
  - Employee_ID, Name, Address, Email, PhoneNumber, Position, HireDate, Department_ID, JobRole_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Employee directory, onboarding, payroll, benefits, attendance, performance management, analytics

## Employee_Project

- **Purpose:** Connects employees to the projects they are working on, including their role on each project.
- **Main Properties:**
  - Employee_Project_ID, Employee_ID, Project_ID, Assignment_Date, Role_on_Project, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Project team management, reporting

## Employee_Training

- **Purpose:** Tracks which employees have attended or completed trainings.
- **Main Properties:**
  - Employee_Training_ID, Employee_ID, Training_ID, Completion_Date, Score, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Training completion tracking, analytics

## Feedback

- **Purpose:** Stores comments or suggestions given to employees to help them improve or recognize their efforts.
- **Main Properties:**
  - Feedback_ID, Employee_ID, Content, DateGiven, GivenBy, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Performance management, engagement

## Grievance

- **Purpose:** Records complaints or issues reported by employees.
- **Main Properties:**
  - Grievance_ID, Description, Status, Employee_ID, SubmittedDate, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - HR help desk, compliance

## Incident

- **Purpose:** Records accidents or problems that happen at work.
- **Main Properties:**
  - Incident_ID, Description, DateReported, Status, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Compliance, safety, HR help desk

## Interview

- **Purpose:** Schedules and tracks job interviews for candidates, including schedules and feedback from interviewers.
- **Main Properties:**
  - Interview_ID, Application_ID, ScheduledAt, Interviewer, Feedback, Status, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Recruitment, ATS

## JobPosting

- **Purpose:** Shows job openings that the company is looking to fill.
- **Main Properties:**
  - JobPosting_ID, Title, Description, PostedDate, EndDate, IsActive, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Recruitment, ATS integration

## JobRole

- **Purpose:** Describes the job title and main responsibilities for each position in the company.
- **Main Properties:**
  - JobRole_ID, Role_Name, Role_Description, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Role assignment, analytics, permissions

## Leave

- **Purpose:** Manages employee requests for time off, such as vacation or sick leave.
- **Main Properties:**
  - Leave_ID, StartDate, EndDate, LeaveType, Status, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Leave management, approvals, reporting

## MentalHealthResource

- **Purpose:** Provides information about support and resources for mental health.
- **Main Properties:**
  - MentalHealthResource_ID, Title, Description, ContactInfo, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Employee support, resource management

## Message

- **Purpose:** Stores messages sent between employees inside the company.
- **Main Properties:**
  - Message_ID, Sender_ID, Recipient_ID, Content, SentAt, IsRead, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Communication, engagement

## Nomination

- **Purpose:** Tracks when employees are suggested for awards or recognition.
- **Main Properties:**
  - Nomination_ID, Employee_ID, Reason, DateNominated, IsAwarded, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Recognition, engagement

## OKRGoal

- **Purpose:** Tracks important goals and results for the company and employees. (OKR means "Objectives and Key Results")
- **Main Properties:**
  - OKRGoal_ID, Objective, KeyResults, StartDate, EndDate, IsCompleted, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Goal setting, performance tracking

## Payroll

- **Purpose:** Keeps track of employee salaries, payments, and bonuses.
- **Main Properties:**
  - Payroll_ID, Salary, Payment_Date, Bonus, NetPay, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Payroll processing, salary history, reporting

## PerformanceReview

- **Purpose:** Stores feedback and ratings about how well employees are doing at work.
- **Main Properties:**
  - PerformanceReview_ID, ReviewDate, Reviewer, Comments, Score, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Performance management, analytics

## PersonalGoal

- **Purpose:** Lists personal goals set by each employee for their own growth or improvement.
- **Main Properties:**
  - PersonalGoal_ID, Goal, TargetDate, IsAchieved, Employee_ID, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Personal development, performance

## Policy

- **Purpose:** Lists the company’s official rules and guidelines.
- **Main Properties:**
  - Policy_ID, Title, Content, EffectiveDate, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Policy management, compliance

## Project

- **Purpose:** Stores details about company projects that employees work on, such as project name, deadline, and budget.
- **Main Properties:**
  - Project_ID, Project_Name, Deadline, Budget, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Project assignment, tracking, analytics

## Survey

- **Purpose:** Lists surveys sent to employees to collect their opinions or feedback.
- **Main Properties:**
  - Survey_ID, Title, Description, DateCreated, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Engagement, analytics

## Training

- **Purpose:** Lists training sessions or courses offered to employees.
- **Main Properties:**
  - Training_ID, Title, Date, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Learning management, compliance

## WellnessProgram

- **Purpose:** Describes programs to help employees stay healthy and well.
- **Main Properties:**
  - WellnessProgram_ID, Title, Description, StartDate, EndDate, IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Use Cases:**
  - Wellness tracking, engagement

---

For more details, see the corresponding API files and data models in the `/Data` and `/Api` folders.
