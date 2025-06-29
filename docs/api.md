# API Reference

## Core HR APIs

### Employee Management

- **Employee API**

  - `GET /api/employees` — List/search employees
  - `GET /api/employees/{id}` — Get employee by ID
  - `POST /api/employees` — Create employee
  - `PUT /api/employees/{id}` — Update employee
  - `DELETE /api/employees/{id}` — Soft delete employee

- **Department API**

  - `GET /api/departments` — List/search departments
  - `GET /api/departments/{id}` — Get department by ID
  - `POST /api/departments` — Create department
  - `PUT /api/departments/{id}` — Update department
  - `DELETE /api/departments/{id}` — Soft delete department

- **JobRole API**

  - `GET /api/jobroles` — List/search job roles
  - `GET /api/jobroles/{id}` — Get job role by ID
  - `POST /api/jobroles` — Create job role
  - `PUT /api/jobroles/{id}` — Update job role
  - `DELETE /api/jobroles/{id}` — Soft delete job role

- **Payroll API**

  - `GET /api/payrolls` — List/search payroll records
  - `GET /api/payrolls/{id}` — Get payroll by ID
  - `POST /api/payrolls` — Create payroll record
  - `PUT /api/payrolls/{id}` — Update payroll record
  - `DELETE /api/payrolls/{id}` — Soft delete payroll record

- **Attendance API**

  - `GET /api/attendances` — List/search attendance records
  - `GET /api/attendances/{id}` — Get attendance by ID
  - `POST /api/attendances` — Create attendance record
  - `PUT /api/attendances/{id}` — Update attendance record
  - `DELETE /api/attendances/{id}` — Soft delete attendance record

- **Leave API**

  - `GET /api/leaves` — List/search leave records
  - `GET /api/leaves/{id}` — Get leave by ID
  - `POST /api/leaves` — Create leave record
  - `PUT /api/leaves/{id}` — Update leave record
  - `DELETE /api/leaves/{id}` — Soft delete leave record

- **Benefit API**
  - `GET /api/benefits` — List/search benefits
  - `GET /api/benefits/{id}` — Get benefit by ID
  - `POST /api/benefits` — Create benefit
  - `PUT /api/benefits/{id}` — Update benefit
  - `DELETE /api/benefits/{id}` — Soft delete benefit

---

## Recruitment & Onboarding

- **JobPosting API**

  - `GET /api/jobpostings` — List/search job postings
  - `GET /api/jobpostings/{id}` — Get job posting by ID
  - `POST /api/jobpostings` — Create job posting
  - `PUT /api/jobpostings/{id}` — Update job posting
  - `DELETE /api/jobpostings/{id}` — Soft delete job posting

- **Application API**

  - `GET /api/applications` — List/search applications
  - `GET /api/applications/{id}` — Get application by ID
  - `POST /api/applications` — Create application
  - `PUT /api/applications/{id}` — Update application
  - `DELETE /api/applications/{id}` — Soft delete application

- **Interview API**

  - `GET /api/interviews/schedule` — List/search interviews
  - `GET /api/interviews/schedule/{id}` — Get interview by ID
  - `POST /api/interviews/schedule` — Schedule interview
  - `PUT /api/interviews/schedule/{id}` — Update interview
  - `DELETE /api/interviews/schedule/{id}` — Soft delete interview

- **Goal API**
  - `GET /api/goals/okr` — List/search OKR goals
  - `GET /api/goals/okr/{id}` — Get OKR goal by ID
  - `POST /api/goals/okr` — Create OKR goal
  - `PUT /api/goals/okr/{id}` — Update OKR goal
  - `DELETE /api/goals/okr/{id}` — Soft delete OKR goal
  - `GET /api/goals/personal` — List/search personal goals
  - `GET /api/goals/personal/{id}` — Get personal goal by ID
  - `POST /api/goals/personal` — Create personal goal
  - `PUT /api/goals/personal/{id}` — Update personal goal
  - `DELETE /api/goals/personal/{id}` — Soft delete personal goal

---

## Project & Assignment

- **Project API**

  - `GET /api/projects` — List/search projects
  - `GET /api/projects/{id}` — Get project by ID
  - `POST /api/projects` — Create project
  - `PUT /api/projects/{id}` — Update project
  - `DELETE /api/projects/{id}` — Soft delete project

- **EmployeeProject API**
  - `GET /api/employeeprojects` — List/search employee-project assignments
  - `GET /api/employeeprojects/{id}` — Get assignment by ID
  - `POST /api/employeeprojects` — Assign employee to project
  - `PUT /api/employeeprojects/{id}` — Update assignment
  - `DELETE /api/employeeprojects/{id}` — Soft delete assignment

---

## Training & Learning

- **Training API**

  - `GET /api/trainings` — List/search trainings
  - `GET /api/trainings/{id}` — Get training by ID
  - `POST /api/trainings` — Create training
  - `PUT /api/trainings/{id}` — Update training
  - `DELETE /api/trainings/{id}` — Soft delete training

- **EmployeeTraining API**
  - `GET /api/employeetrainings` — List/search employee-training assignments
  - `GET /api/employeetrainings/{id}` — Get assignment by ID
  - `POST /api/employeetrainings` — Assign employee to training
  - `PUT /api/employeetrainings/{id}` — Update assignment
  - `DELETE /api/employeetrainings/{id}` — Soft delete assignment

---

## Performance & Feedback

- **PerformanceReview API**

  - `GET /api/performancereviews` — List/search performance reviews
  - `GET /api/performancereviews/{id}` — Get review by ID
  - `POST /api/performancereviews` — Create review
  - `PUT /api/performancereviews/{id}` — Update review
  - `DELETE /api/performancereviews/{id}` — Soft delete review

- **Feedback API**

  - `GET /api/feedback/continuous` — List/search feedback
  - `GET /api/feedback/continuous/{id}` — Get feedback by ID
  - `POST /api/feedback/continuous` — Create feedback
  - `PUT /api/feedback/continuous/{id}` — Update feedback
  - `DELETE /api/feedback/continuous/{id}` — Soft delete feedback

- **Survey API**
  - `GET /api/feedback/surveys` — List/search surveys
  - `GET /api/feedback/surveys/{id}` — Get survey by ID
  - `POST /api/feedback/surveys` — Create survey
  - `PUT /api/feedback/surveys/{id}` — Update survey
  - `DELETE /api/feedback/surveys/{id}` — Soft delete survey

---

## Recognition & Engagement

- **Award API**

  - `GET /api/recognition/awards` — List/search awards
  - `GET /api/recognition/awards/{id}` — Get award by ID
  - `POST /api/recognition/awards` — Create award
  - `PUT /api/recognition/awards/{id}` — Update award
  - `DELETE /api/recognition/awards/{id}` — Soft delete award

- **Nomination API**

  - `GET /api/recognition/nominations` — List/search nominations
  - `GET /api/recognition/nominations/{id}` — Get nomination by ID
  - `POST /api/recognition/nominations` — Create nomination
  - `PUT /api/recognition/nominations/{id}` — Update nomination
  - `DELETE /api/recognition/nominations/{id}` — Soft delete nomination

- **Announcement API**

  - `GET /api/communication/announcements` — List/search announcements
  - `GET /api/communication/announcements/{id}` — Get announcement by ID
  - `POST /api/communication/announcements` — Create announcement
  - `PUT /api/communication/announcements/{id}` — Update announcement
  - `DELETE /api/communication/announcements/{id}` — Soft delete announcement

- **Message API**
  - `GET /api/communication/messages` — List/search messages
  - `GET /api/communication/messages/{id}` — Get message by ID
  - `POST /api/communication/messages` — Send message
  - `PUT /api/communication/messages/{id}` — Update message
  - `DELETE /api/communication/messages/{id}` — Soft delete message

---

## Wellbeing, Policy & Compliance

- **WellnessProgram API**

  - `GET /api/wellnessprograms` — List/search wellness programs
  - `GET /api/wellnessprograms/{id}` — Get program by ID
  - `POST /api/wellnessprograms` — Create program
  - `PUT /api/wellnessprograms/{id}` — Update program
  - `DELETE /api/wellnessprograms/{id}` — Soft delete program

- **MentalHealthResource API**

  - `GET /api/mentalhealthresources` — List/search resources
  - `GET /api/mentalhealthresources/{id}` — Get resource by ID
  - `POST /api/mentalhealthresources` — Create resource
  - `PUT /api/mentalhealthresources/{id}` — Update resource
  - `DELETE /api/mentalhealthresources/{id}` — Soft delete resource

- **Policy API**

  - `GET /api/policies` — List/search policies
  - `GET /api/policies/{id}` — Get policy by ID
  - `POST /api/policies` — Create policy
  - `PUT /api/policies/{id}` — Update policy
  - `DELETE /api/policies/{id}` — Soft delete policy

- **DEIResource API**

  - `GET /api/deiresources` — List/search DEI resources
  - `GET /api/deiresources/{id}` — Get resource by ID
  - `POST /api/deiresources` — Create resource
  - `PUT /api/deiresources/{id}` — Update resource
  - `DELETE /api/deiresources/{id}` — Soft delete resource

- **Incident API**

  - `GET /api/incidents/list` — List/search incidents
  - `GET /api/incidents/list/{id}` — Get incident by ID
  - `POST /api/incidents/report` — Report incident
  - `PUT /api/incidents/list/{id}` — Update incident
  - `DELETE /api/incidents/list/{id}` — Soft delete incident

- **Grievance API**
  - `GET /api/grievances` — List/search grievances
  - `GET /api/grievances/{id}` — Get grievance by ID
  - `POST /api/grievances` — Create grievance
  - `PUT /api/grievances/{id}` — Update grievance
  - `DELETE /api/grievances/{id}` — Soft delete grievance

---

## Analytics

- **Analytics API**
  - `GET /api/analytics/employee-summary` — Employee summary analytics
  - `GET /api/analytics/turnover` — Turnover analytics
  - `GET /api/analytics/diversity` — Diversity analytics
  - `GET /api/analytics/absenteeism` — Absenteeism analytics
  - `GET /api/analytics/training-participation` — Training participation analytics
  - `GET /api/analytics/performance-distribution` — Performance review distribution

---

## Identity & Security

- **Account API**

  - `POST /api/account/login` — User login
  - `POST /api/account/logout` — User logout
  - `POST /api/account/register` — User registration
  - ... (other account endpoints)

- **Password API**

  - `POST /api/password/set` — Set password
  - `GET /api/password/has-password` — Check if user has password
  - `POST /api/password/change` — Change password
  - ... (other password endpoints)

- **Role API**

  - `GET /api/roles` — List roles
  - `POST /api/roles` — Create role
  - `DELETE /api/roles/{roleId}` — Delete role
  - `POST /api/roles/assign` — Assign role to user

- **User API**

  - `GET /api/users` — List users and roles
  - `DELETE /api/users/{id}` — Delete user
  - `POST /api/users/{id}/claims` — Add user claim
  - ... (other user endpoints)

- **TwoFactor API**
  - `GET /api/2fa/status` — Get 2FA status
  - `GET /api/2fa/setup` — Get authenticator setup info
  - ... (other 2FA endpoints)

---

All endpoints support robust filtering, search, paging, soft delete, and audit fields where applicable. See the Data Models page for details on the underlying entities.
