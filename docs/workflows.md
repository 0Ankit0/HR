# HR System Workflows

This document explains how data moves through the HR system for common scenarios, showing which API endpoints are involved at each step. Each workflow is written in simple language for easy understanding.

---

# 1. Authentication & Account Management

## 1.1 User Registration & Account Activation Workflow

1. **User Registration**: New user submits registration form.
   - Endpoint: `POST /api/account/register`
2. **Email Confirmation**: System sends confirmation email with link.
   - Endpoint: `POST /api/account/resendemailconfirmation`
3. **Resend Email Verification**: User requests a new verification email (from manage page or registration).
   - Endpoint: `POST /api/account/resendemailconfirmation`
4. **Email Activation**: User clicks link to activate account.
   - Endpoint: `/account/confirmemail?userId=...&code=...` (GET, from email link)

---

## 1.2 Login Workflow

1. **Login Attempt**: User logs in with credentials.
   - Endpoint: `POST /api/account/login`
2. **2FA Required**: If two-factor authentication is enabled, user is prompted for a 2FA code.
   - Endpoint: `POST /api/account/loginwith2fa`
3. **Login with Recovery Code**: If user cannot access 2FA device, they can use a recovery code.
   - Endpoint: `POST /api/account/loginwithrecoverycode`
4. **Successful Login**: User is authenticated and receives access.
   - Endpoint: (Handled by previous endpoints)
5. **Account Lockout**: If too many failed attempts or user locks their own account, account is locked.
   - Endpoint: `/account/lockout` (GET, redirect or handled in login response)
6. **Unlock Account**: User unlocks their account (e.g., after lockout period or via unlock process).
   - Endpoint: `POST /api/account/unlock`
7. **Request Unlock**: User requests unlock instructions.
   - Endpoint: `POST /api/account/requestunlock`

---

## 1.3 Password Reset Workflow

1. **Password Reset**: User sets new password via link received in email.
   - Endpoint: `POST /api/account/resetpassword`

---

## 1.4 Two-Factor Authentication (2FA) Workflow

1. **Enable 2FA**: User sets up two-factor authentication.
   - Endpoint: `POST /api/twofactor/enable`
2. **2FA Verification**: User enters code from authenticator app or SMS.
   - Endpoint: `POST /api/twofactor/verify`
3. **Disable 2FA**: User disables two-factor authentication.
   - Endpoint: `POST /api/twofactor/disable`
4. **Reset Authenticator**: User resets their authenticator app.
   - Endpoint: `POST /api/twofactor/reset`
5. **Generate Recovery Codes**: User generates backup codes for 2FA.
   - Endpoint: `POST /api/twofactor/generaterecoverycodes`
6. **View 2FA Status**: User checks if 2FA is enabled.
   - Endpoint: `GET /api/twofactor/status`
7. **Enable/Disable Lockout**: User enables or disables account lockout for failed 2FA attempts.
   - Endpoint: `POST /api/account/enablelockout`, `POST /api/account/disablelockout`

---

## 1.5 Account Management Workflows (Manage Page)

1. **Change Email**: User requests to change their email address.
   - Endpoint: `POST /api/user/changeemail`
2. **Confirm Email Change**: User confirms new email via code.
   - Endpoint: `POST /api/user/confirm-email-change`
3. **Send Email Verification**: User requests a verification email for their current or new email.
   - Endpoint: `POST /api/user/sendemailverification`
4. **Verify Email**: User verifies their email using a code.
   - Endpoint: `POST /api/user/confirm-email`
5. **Change Phone Number**: User requests to change their phone number.
   - Endpoint: `POST /api/user/changephone`
6. **Send Phone Verification SMS**: User requests a verification SMS for their phone.
   - Endpoint: `POST /api/user/sendphonesms`
7. **Verify Phone**: User verifies their phone using a code.
   - Endpoint: `POST /api/user/confirmphone`
8. **Change Password**: User changes their password.
   - Endpoint: `POST /api/account/changepassword`
9. **Set Password**: User sets a password if one does not exist.
   - Endpoint: `POST /api/account/setpassword`
10. **Download Personal Data**: User downloads their personal data.
    - Endpoint: `GET /api/account/downloadpersonaldata`
11. **Delete Personal Data**: User requests deletion of their personal data.
    - Endpoint: `POST /api/account/deletepersonaldata`
12. **External Logins**: User manages external login providers (add/remove).
    - Endpoint: `GET /api/account/externallogins`, `POST /api/account/linklogin`, `POST /api/account/removelogin`
13. **Profile Info**: User views or updates their profile information.
    - Endpoint: `GET /api/account/profile`, `PUT /api/account/profile`

---

# 2. User & Role Management

## 2.1 Authentication, Authorization, Roles & Claims Workflow

1. **Role Creation**: Admin creates a new role (e.g., Manager, HR).
   - Endpoint: `POST /api/roles`
2. **Assign Role to User**: Admin assigns roles to users.
   - Endpoint: `POST /api/roles/assign`
3. **Remove Role from User**: Admin removes a role from a user.
   - Endpoint: `POST /api/roles/remove`
4. **Get All Roles**: List all roles.
   - Endpoint: `GET /api/roles`
5. **Get Users in Role**: List users in a specific role.
   - Endpoint: `GET /api/roles/{roleId}/users`
6. **Get Role by Id**: Get details of a specific role.
   - Endpoint: `GET /api/roles/{roleId}`
7. **Update Role**: Update a role's details.
   - Endpoint: `PUT /api/roles/{roleId}`
8. **Get Role Claims**: List all claims for a role.
   - Endpoint: `GET /api/roles/{roleId}/claims`
9. **Update Role Claims**: Replace all claims for a role.
   - Endpoint: `PUT /api/roles/{roleId}/claims`
10. **Add Claim to User**: Add a claim to a user.
    - Endpoint: `POST /api/users/{id}/claims`
11. **Remove Claim from User**: Remove a claim from a user.
    - Endpoint: `DELETE /api/users/{id}/claims`
12. **Get User Claims**: Get claims for the current user.
    - Endpoint: `GET /api/user/claims`
13. **Role-Based Access**: User accesses protected endpoints based on role and claims.
    - Example: `GET /api/employees` (requires HR/Admin role or claim)
14. **Logout**: User logs out, token/session is invalidated.
    - Endpoint: `GET /api/account/logout`

---

## 2.2 User Management Workflow

1. **List Users**: Admin views all users.
   - Endpoint: `GET /api/users`
2. **Delete User**: Admin deletes a user account.
   - Endpoint: `DELETE /api/users/{id}`
3. **Check If User Has Password**: Check if a user has a password set (for lockout/delete scenarios).
   - Endpoint: `GET /api/user/has-password`

---

# 3. Core HR Operations

## 3.1 Employee Onboarding Workflow

1. **Employee Record Creation**: HR adds new employee details.
   - Endpoint: `POST /api/employees`
2. **Assign Department & Role**: Employee is linked to department and job role.
   - Endpoint: `PUT /api/employees/{id}`
3. **Benefits Enrollment**: Employee selects benefits.
   - Endpoint: `POST /api/benefits`
4. **Training Assignment**: Employee is assigned required trainings.
   - Endpoint: `POST /api/employeetrainings`

---

## 3.2 Payroll & Attendance Workflow

1. **Attendance Tracking**: Employee clock-in/out is recorded.
   - Endpoint: `POST /api/attendances`
2. **Leave Requests**: Employee submits leave request.
   - Endpoint: `POST /api/leaves`
3. **Payroll Calculation**: Payroll is processed using attendance and leave data.
   - Endpoint: `POST /api/payrolls`

---

## 3.3 Grievance & Incident Management Workflow

1. **Grievance Submission**: Employee submits a grievance.
   - Endpoint: `POST /api/grievances`
2. **Incident Reporting**: Employee or HR reports an incident.
   - Endpoint: `POST /api/incidents`
3. **Status Updates**: HR updates status as issues are resolved.
   - Endpoints: `PUT /api/grievances/{id}`, `PUT /api/incidents/{id}`

---

## 3.4 Benefits Management Workflow

1. **Benefit Offering**: HR adds new benefit options.
   - Endpoint: `POST /api/benefits`
2. **Employee Enrollment**: Employees enroll or update benefits.
   - Endpoint: `PUT /api/benefits/{id}`

---

## 3.5 Policy Management Workflow

1. **List Policies**: View all company policies.
   - Endpoint: `GET /api/policies`
2. **Get Policy Details**: View a specific policy.
   - Endpoint: `GET /api/policies/{id}`
3. **Create Policy**: HR creates a new policy.
   - Endpoint: `POST /api/policies`
4. **Update Policy**: HR updates an existing policy.
   - Endpoint: `PUT /api/policies/{id}`
5. **Delete Policy**: HR deletes a policy (soft delete).
   - Endpoint: `DELETE /api/policies/{id}`

---

## 3.6 Job Posting Management Workflow

1. **List Job Postings**: View all open job postings.
   - Endpoint: `GET /api/jobpostings`
2. **Get Job Posting Details**: View a specific job posting.
   - Endpoint: `GET /api/jobpostings/{id}`
3. **Create Job Posting**: HR creates a new job posting.
   - Endpoint: `POST /api/jobpostings`
4. **Update Job Posting**: HR updates an existing job posting.
   - Endpoint: `PUT /api/jobpostings/{id}`
5. **Delete Job Posting**: HR deletes a job posting.
   - Endpoint: `DELETE /api/jobpostings/{id}`

---

## 3.7 Application Management Workflow

1. **List Applications**: View all job applications.
   - Endpoint: `GET /api/applications`
2. **Get Application Details**: View a specific application.
   - Endpoint: `GET /api/applications/{id}`
3. **Submit Application**: Candidate submits a job application.
   - Endpoint: `POST /api/applications`
4. **Update Application**: HR or candidate updates an application.
   - Endpoint: `PUT /api/applications/{id}`
5. **Delete Application**: HR deletes an application.
   - Endpoint: `DELETE /api/applications/{id}`

---

## 3.8 Interview Scheduling Workflow

1. **List Interviews**: View all scheduled interviews.
   - Endpoint: `GET /api/interviews/schedule`
2. **Get Interview Details**: View a specific interview schedule.
   - Endpoint: `GET /api/interviews/schedule/{id}`
3. **Schedule Interview**: HR schedules a new interview.
   - Endpoint: `POST /api/interviews/schedule`
4. **Update Interview**: HR updates an interview schedule.
   - Endpoint: `PUT /api/interviews/schedule/{id}`
5. **Delete Interview**: HR deletes an interview schedule.
   - Endpoint: `DELETE /api/interviews/schedule/{id}`

---

## 3.9 Project Management Workflow

1. **List Projects**: View all projects.
   - Endpoint: `GET /api/projects`
2. **Get Project Details**: View a specific project.
   - Endpoint: `GET /api/projects/{id}`
3. **Create Project**: HR or manager creates a new project.
   - Endpoint: `POST /api/projects`
4. **Update Project**: HR or manager updates a project.
   - Endpoint: `PUT /api/projects/{id}`
5. **Delete Project**: HR or manager deletes a project.
   - Endpoint: `DELETE /api/projects/{id}`

---

## 3.10 Employee-Project Assignment Workflow

1. **List Employee-Project Assignments**: View all employee-project assignments.
   - Endpoint: `GET /api/employeeprojects`
2. **Get Assignment Details**: View a specific assignment.
   - Endpoint: `GET /api/employeeprojects/{id}`
3. **Assign Employee to Project**: Assign an employee to a project.
   - Endpoint: `POST /api/employeeprojects`
4. **Update Assignment**: Update an employee-project assignment.
   - Endpoint: `PUT /api/employeeprojects/{id}`
5. **Remove Assignment**: Remove an employee from a project.
   - Endpoint: `DELETE /api/employeeprojects/{id}`

---

## 3.11 Employee-Training Assignment Workflow

1. **List Employee-Training Assignments**: View all employee-training assignments.
   - Endpoint: `GET /api/employeetrainings`
2. **Get Assignment Details**: View a specific assignment.
   - Endpoint: `GET /api/employeetrainings/{id}`
3. **Assign Employee to Training**: Assign an employee to a training.
   - Endpoint: `POST /api/employeetrainings`
4. **Update Assignment**: Update an employee-training assignment.
   - Endpoint: `PUT /api/employeetrainings/{id}`
5. **Remove Assignment**: Remove an employee from a training.
   - Endpoint: `DELETE /api/employeetrainings/{id}`

---

## 3.12 Department Management Workflow

1. **List Departments**: View all departments.
   - Endpoint: `GET /api/departments`
2. **Get Department Details**: View a specific department.
   - Endpoint: `GET /api/departments/{id}`
3. **Create Department**: HR creates a new department.
   - Endpoint: `POST /api/departments`
4. **Update Department**: HR updates a department.
   - Endpoint: `PUT /api/departments/{id}`
5. **Delete Department**: HR deletes a department.
   - Endpoint: `DELETE /api/departments/{id}`

---

## 3.13 Job Role Management Workflow

1. **List Job Roles**: View all job roles.
   - Endpoint: `GET /api/jobroles`
2. **Get Job Role Details**: View a specific job role.
   - Endpoint: `GET /api/jobroles/{id}`
3. **Create Job Role**: HR creates a new job role.
   - Endpoint: `POST /api/jobroles`
4. **Update Job Role**: HR updates a job role.
   - Endpoint: `PUT /api/jobroles/{id}`
5. **Delete Job Role**: HR deletes a job role.
   - Endpoint: `DELETE /api/jobroles/{id}`

---

# 4. Performance & Development

## 4.1 Performance Management Workflow

1. **Goal Setting**: Employee and manager set OKR and personal goals.
   - Endpoints: `POST /api/goals/okr`, `POST /api/goals/personal`
2. **Continuous Feedback**: Feedback is given throughout the period.
   - Endpoint: `POST /api/feedback/continuous`
3. **Performance Review**: Formal review is conducted.
   - Endpoint: `POST /api/performancereviews`

---

## 4.2 Learning & Development Workflow

1. **Training Creation**: HR creates training sessions.
   - Endpoint: `POST /api/trainings`
2. **Employee Assignment**: Employees are assigned to trainings.
   - Endpoint: `POST /api/employeetrainings`
3. **Completion Tracking**: Training completion is recorded.
   - Endpoint: `PUT /api/employeetrainings/{id}`

---

# 5. Wellbeing & Culture

## 5.1 Wellness & Mental Health Resource Workflow

1. **List Wellness Programs**: View all wellness programs.
   - Endpoint: `GET /api/wellnessprograms`
2. **Create Wellness Program**: HR creates a new wellness program.
   - Endpoint: `POST /api/wellnessprograms`
3. **List Mental Health Resources**: View all mental health resources.
   - Endpoint: `GET /api/mentalhealthresources`
4. **Create Mental Health Resource**: HR adds a new mental health resource.
   - Endpoint: `POST /api/mentalhealthresources`

---

## 5.2 DEI Resource Workflow

1. **List DEI Resources**: View all diversity, equity, and inclusion resources.
   - Endpoint: `GET /api/deiresources`
2. **Create DEI Resource**: HR adds a new DEI resource.
   - Endpoint: `POST /api/deiresources`

---

## 5.3 Recognition & Awards Workflow

1. **List Awards**: View all employee awards.
   - Endpoint: `GET /api/recognition/awards`
2. **Create Award**: HR or manager creates a new award.
   - Endpoint: `POST /api/recognition/awards`
3. **List Nominations**: View all award nominations.
   - Endpoint: `GET /api/recognition/nominations`
4. **Create Nomination**: Employee or manager nominates someone for an award.
   - Endpoint: `POST /api/recognition/nominations`

---

## 5.4 Survey & Feedback Workflow

1. **List Surveys**: View all employee surveys.
   - Endpoint: `GET /api/feedback/surveys`
2. **Create Survey**: HR creates a new survey.
   - Endpoint: `POST /api/feedback/surveys`
3. **Submit Feedback**: Employee submits feedback.
   - Endpoint: `POST /api/feedback/continuous`

---

# 6. Analytics & Reporting

## 6.1 Analytics & Reporting Workflow

1. **Data Aggregation**: System collects data from all modules.
   - Endpoint: `GET /api/analytics/*`
2. **Dashboard Display**: HR views reports and dashboards.
   - Endpoint: `GET /api/analytics/summary`

---

## 6.2 Analytics & Workforce Insights Workflow

1. **Employee Summary**: View summary analytics for employees.
   - Endpoint: `GET /api/analytics/employee-summary`
2. **Turnover Analytics**: View employee turnover statistics.
   - Endpoint: `GET /api/analytics/turnover`
3. **Diversity Analytics**: View diversity statistics.
   - Endpoint: `GET /api/analytics/diversity`
4. **Absenteeism Analytics**: View absenteeism rates.
   - Endpoint: `GET /api/analytics/absenteeism`
5. **Training Participation**: View training participation rates.
   - Endpoint: `GET /api/analytics/training-participation`
6. **Performance Distribution**: View performance review distribution.
   - Endpoint: `GET /api/analytics/performance-distribution`
7. **Compensation by Department**: View compensation analytics by department.
   - Endpoint: `GET /api/analytics/compensation-by-department`
8. **Headcount Trend**: View headcount trends over time.
   - Endpoint: `GET /api/analytics/headcount-trend`

---

# 7. Communication

## 7.1 Communication & Announcements Workflow

1. **Announcement Creation**: HR posts a company-wide announcement.
   - Endpoint: `POST /api/announcements`
2. **Internal Messaging**: Employees send messages to each other.
   - Endpoint: `POST /api/messages`

---

For more details, see the API documentation and data model reference.
