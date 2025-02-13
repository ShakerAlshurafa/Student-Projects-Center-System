using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading.Channels;

namespace StudentProjectsCenterSystem.Controllers
{
    [Authorize]
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task.WorkgroupFile file;
        private readonly AzureFileUploader _uploadHandler;
        private readonly IEmailService emailService;

        public TasksController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            AzureFileUploader uploadHandler,
            IEmailService emailService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            file = new StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task.WorkgroupFile();
            _uploadHandler = uploadHandler;
            this.emailService = emailService;
        }

        [Authorize(Roles = "supervisor")]
        [HttpGet("all-supervisor-tasks")]
        public async Task<ActionResult<ApiResponse>> GetAllSupervisorTasks()
        {
            var supervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(supervisorId))
            {
                return Unauthorized(new ApiResponse(401, "Supervisor id not Find."));
            }

            Expression<Func<WorkgroupTask, bool>> filter = t => t.Workgroup.Project != null &&
                t.Workgroup.Project.UserProjects
                    .Any(u => u.Role == "supervisor" && u.UserId == supervisorId && !u.IsDeleted);

            // Fetch tasks for the specified workgroup with pagination
            var tasks = await unitOfWork.taskRepository.GetAll(filter: filter, "Workgroup.Project.UserProjects");

            // Check if tasks are available
            if (tasks == null || !tasks.Any())
            {
                return Ok(new ApiResponse(200, "No tasks found."));
            }

            var taskDto = mapper.Map<List<AllTaskDTO>>(tasks);

            return Ok(new ApiResponse(200, "Tasks retrieved successfully.", taskDto));
        }


        [HttpGet("all-workgroup-tasks/{workgroupId}")]
        public async Task<ActionResult<ApiResponse>> GetAllWorkgroupTasks(int workgroupId)
        {
            // Validate workgroup existence
            var workgroupExists = await unitOfWork.workgroupRepository.IsExist(workgroupId);
            if (!workgroupExists)
            {
                return NotFound(new ApiResponse(404, "Workgroup not found."));
            }

            // Fetch tasks for the specified workgroup with pagination
            var tasks = await unitOfWork.taskRepository.GetAll(filter: t => t.WorkgroupId == workgroupId);

            // Check if tasks are available
            if (tasks == null || !tasks.Any())
            {
                return Ok(new ApiResponse(200, "No tasks found for the specified workgroup."));
            }

            var taskDto = mapper.Map<List<AllWorkgroupTaskDTO>>(tasks);

            return Ok(new ApiResponse(200, "Tasks retrieved successfully.", taskDto));
        }


        [Authorize(Roles = "supervisor")]
        [HttpPost("{workgroupId}")]
        public async Task<ActionResult<ApiResponse>> Create(
            int workgroupId,
            [FromForm, Required] TaskCreateDTO taskDto)
        {
            //if (!taskDto.ValidExtensions.Any())
            //    return BadRequest(new ApiResponse(400, "Valid extensions are required."));

            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId, "Project.UserProjects.User");
            if (workgroup == null)
            {
                return NotFound($"Workgroup with ID {workgroupId} was not found.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(401, "User not found."));
            }

            if (workgroup.Project == null)
            {
                return Unauthorized(new ApiResponse(401, "Project not associated with workgroup."));
            }

            bool isSupervisor = workgroup.Project.UserProjects
                .Any(u => u.UserId == userId && (u.Role == "supervisor" || u.Role == "co-supervisor"));

            if (!isSupervisor)
            {
                return Unauthorized(new ApiResponse(401, "User is not a supervisor."));
            }


            var countCompletedTasks = await unitOfWork.taskRepository.Count(t =>
                t.WorkgroupId == workgroupId && t.Status.ToLower() == "completed",
                includeProperty: "Workgroup");
            var countAllTasks = await unitOfWork.taskRepository.Count(t =>
                t.WorkgroupId == workgroup.Id && t.Status.ToLower() != "canceled",
                includeProperty: "Workgroup");
            countAllTasks++;

            workgroup.Progress = countAllTasks == 0 ?  100 :
                    (int)(((double)countCompletedTasks / countAllTasks) * 100);

            // Check if dates are in the future
            if (taskDto.Start < DateTime.UtcNow || taskDto.End < DateTime.UtcNow)
                return BadRequest(new ApiResponse(400, "Dates must not be in the past."));

            // Check if Start and End dates are valid
            if (taskDto.Start >= taskDto.End)
                return BadRequest(new ApiResponse(400, "The start date must be earlier than the end date."));

            var uploadedFiles = new List<FileDTO>();
            if (taskDto.QuestionFile != null)
            {
                foreach (var file in taskDto.QuestionFile)
                {
                    if (file.Length == 0)
                    {
                        return BadRequest(new ApiResponse(400, "File is Empty."));
                    }
                    var fileDto = await _uploadHandler.UploadAsync(file, "resources");

                    if (!fileDto.ErrorMessage.IsNullOrEmpty())
                    {
                        return BadRequest(new ApiResponse(400, fileDto.ErrorMessage));
                    }

                    uploadedFiles.Add(fileDto);
                }
            }

            var task = new WorkgroupTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Start = taskDto.Start,
                End = taskDto.End,
                WorkgroupId = workgroupId,
                Author = User?.Identity?.Name ?? "",
                Files = uploadedFiles.Select(f => new WorkgroupFile()
                {
                    Path = f.Path ?? "",
                    Name = f.Name ?? "",
                    Type = "question"
                }).ToList()
            };

            unitOfWork.workgroupRepository.Update(workgroup);
            await unitOfWork.taskRepository.Create(task);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create failed"));
            }


            var students = workgroup.Project?.UserProjects
                .Where(up => !up.IsDeleted && up.Role == "student" 
                        && up.User.EmailConfirmed && !string.IsNullOrWhiteSpace(up.User.Email))
                .Select(up => up.User)
                .ToList();

            if (students != null && students.Any())
            {
                foreach (var student in students)
                {
                    try
                    {
                        string emailContent = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                        border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                                <h2 style='color: #333; text-align: center;'>New Task Assigned</h2>
                                <p style='font-size: 16px; color: #555;'>Hello <strong>{student.FirstName}</strong>,</p>
                                <p style='font-size: 16px; color: #555;'>
                                    A new task titled <strong>'{taskDto.Title}'</strong> has been assigned in your workgroup by 
                                    <strong>{User?.Identity?.Name ?? "an unknown user"}</strong>.
                                </p>
                                <p><strong>Description:</strong> {taskDto.Description}</p>
                                <p><strong>Due Date:</strong> {taskDto.End?.ToString("yyyy-MM-dd") ?? "No deadline"}</p>
                                <p style='text-align: center;'>
                                    <a href='http://localhost:5173/workgroups/{workgroupId}/tasks' 
                                        style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                                        color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                        View Task Details
                                    </a>
                                </p>
                                <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                                    If you have any questions, please contact your supervisor.
                                </p>
                            </div>
                                    ";

                        var emailSent = await emailService.SendEmailAsync(student.Email ?? "", "New Task Assigned", emailContent, true);

                        if (!emailSent.IsSuccess)
                        {
                            Console.WriteLine($"Error sending email to {student.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {student.Email}: {ex.Message}");
                    }
                }
            }



            return CreatedAtAction(nameof(Create), new { id = task.Id },
                new ApiResponse(201, "Task created successfully", result: task.Id));
        }

        [Authorize(Roles = "supervisor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(
            int id,
            [FromForm, Required] TaskUpdateDTO taskDto)
        {
            // Validate taskDto
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid data."));
            }

            // Fetch the task from the database
            var existingTask = await unitOfWork.taskRepository.GetById(id, "Files");
            if (existingTask == null)
            {
                return NotFound(new ApiResponse(404, "Task not found."));
            }

            // Check if dates are in the future
            if ((existingTask.Start != taskDto.Start && taskDto.Start < DateTime.UtcNow)
                || (existingTask.End != taskDto.End && taskDto.End < DateTime.UtcNow))
                return BadRequest(new ApiResponse(400, "Dates must not be in the past."));

            existingTask.Start = taskDto.Start;
            existingTask.End = taskDto.End;

            // Check if Start and End dates are valid
            if (existingTask.Start >= existingTask.End)
                return BadRequest(new ApiResponse(400, "The start date must be earlier than the end date."));

            // Update properties
            existingTask.Title = taskDto.Title ?? "";
            existingTask.Description = taskDto.Description ?? "";

            existingTask.Files = null;

            if (taskDto.QuestionFile != null)
            {
                //if (!taskDto.ValidExtensions.Any())
                //    return BadRequest(new ApiResponse(400, "Valid extensions are required for the file."));

                var uploadedFiles = new List<FileDTO>();
                var existingFiles = new List<WorkgroupFile>();

                foreach (var file in taskDto.QuestionFile)
                {
                    if (file is IFormFile uploadedFile) // Check if it's a new uploaded file
                    {
                        if (uploadedFile.Length == 0)
                        {
                            return BadRequest(new ApiResponse(400, "File is Empty."));
                        }

                        var fileDto = await _uploadHandler.UploadAsync(uploadedFile, "resources");

                        if (!fileDto.ErrorMessage.IsNullOrEmpty())
                        {
                            return BadRequest(new ApiResponse(400, fileDto.ErrorMessage));
                        }

                        uploadedFiles.Add(fileDto);
                    }
                    else // it's an existing file
                    {
                        WorkgroupFile existingFile = (WorkgroupFile)file;
                        existingFiles.Add(new WorkgroupFile
                        {
                            Path = existingFile.Path,
                            Name = existingFile.Name ?? "",
                            Type = "question"
                        });
                    }
                }

                // Save new files
                var newFiles = uploadedFiles.Select(f => new WorkgroupFile
                {
                    Path = f.Path ?? "",
                    Name = f.Name ?? "",
                    Type = "question"
                }).ToList();

                // Combine new files with existing files
                existingTask.Files = newFiles.Concat(existingFiles).ToList();
            }

            existingTask.LastUpdateBy = User?.Identity?.Name ?? "";
            existingTask!.LastUpdatedAt = DateTime.UtcNow;

            // Save changes
            unitOfWork.taskRepository.Update(existingTask);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update failed"));
            }

            return Ok(new ApiResponse(200, "Task updated successfully", result: existingTask));
        }


        // GET: api/task/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetTask(int id)
        {
            var task = await unitOfWork.taskRepository.GetById(id, "Files");
            if (task == null)
            {
                return NotFound(new ApiResponse(404, "Task not found"));
            }

            var questionFiles = new List<FileDTO>();
            var answerFiles = new List<FileDTO>();

            if (task.Files != null)
            {
                foreach (var file in task.Files.ToList())
                {
                    if (file.Type == "question")
                    {
                        questionFiles.Add(new FileDTO()
                        {
                            Path = file.Path,
                            Name = file.Name,
                            CreatedAt = file.CreatedAt,
                            Type = file.Type
                        });
                    }
                    else
                    {
                        answerFiles.Add(new FileDTO()
                        {
                            Path = file.Path,
                            Name = file.Name,
                            CreatedAt = file.CreatedAt,
                            Type = file.Type
                        });
                    }
                }
            }
            var taskDto = new TaskDTO()
            {
                Title = task.Title,
                Description = task.Description,
                Start = task.Start,
                End = task.End,
                Author = task.Author,
                LastUpdateBy = task.LastUpdateBy,
                LastUpdatedAt = task.LastUpdatedAt,
                SubmittedBy = task.SubmittedBy,
                SubmittedAt = task.SubmittedAt,
                QuestionFiles = questionFiles,
                AnswerFiles = answerFiles,
                Status = task.Status
            };

            return Ok(new ApiResponse(200, result: taskDto));
        }

        [HttpGet("task-statuses")]
        public ActionResult<ApiResponse> GetTaskStatuses()
        {
            return Ok(new ApiResponse(200, result: new
            {
                NotStarted = "Task has been created but work has not yet begun.",
                InProgress = "Task is currently being worked on.",
                Submitted = "Task has been completed and submitted for review.",
                Completed = "Task is finished",
                Rejected = "Task submission has been reviewed and requires changes or adjustments",
                Canceled = "Task is no longer needed and has been closed"
            }));
        }

        [Authorize(Roles ="supervisor")]
        [HttpPut("{id}/change-status")]
        public async Task<ActionResult<ApiResponse>> ChangeStatus(
            int id,
            [Required, FromBody] string status)
        {
            // Validate the status input
            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new ApiResponse(400, "Status is required."));
            }

            status = status.Trim().ToLower();

            var taskStatus = new List<string> { "completed", "rejected", "canceled" };
            if (!taskStatus.Contains(status))
            {
                return BadRequest(new ApiResponse(400,
                    $"The provided status '{status}' is invalid. Valid statuses are: {string.Join(", ", taskStatus)}."));
            }

            // Retrieve the task from the repository
            var existingTask = await unitOfWork.taskRepository.GetById(id);
            if (existingTask == null)
            {
                return NotFound(new ApiResponse(404, "Task not found."));
            }

            if (existingTask.Status.ToLower() == status)
            {
                return BadRequest(new ApiResponse(400, "The status is already set to the specified task."));
            }


            // Retrieve workgroup only if necessary
            var workgroup = await unitOfWork.workgroupRepository.GetById(existingTask.WorkgroupId, "Project.UserProjects.User");
            if (workgroup == null)
            {
                return NotFound(new ApiResponse(404, $"Workgroup with ID {existingTask.WorkgroupId} was not found."));
            }

            var students = workgroup.Project?.UserProjects
                .Where(up => !up.IsDeleted && up.Role == "student"
                        && up.User.EmailConfirmed && !string.IsNullOrWhiteSpace(up.User.Email))
                .Select(up => up.User)
                .ToList();
                
            if (status == "completed" || existingTask.Status.ToLower() == "completed")
            {
                int countCompletedTasks = await unitOfWork.taskRepository.Count(t =>
                    t.WorkgroupId == workgroup.Id && t.Status.ToLower() == "completed");

                int countAllTasks = await unitOfWork.taskRepository.Count(t =>
                    t.WorkgroupId == workgroup.Id && t.Status.ToLower() != "canceled");

                // Adjust the count when status changes
                if (status == "completed") countCompletedTasks++;
                if (existingTask.Status.ToLower() == "completed") countCompletedTasks--;

                workgroup.Progress = countAllTasks == 0 ? (status == "completed" ? 100 : 0) :
                    (int)(((double)countCompletedTasks / countAllTasks) * 100);

                unitOfWork.workgroupRepository.Update(workgroup);
            }

            // Update the status
            existingTask.Status = status;

            unitOfWork.taskRepository.Update(existingTask); // Save the updated task

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update task status."));
            }

            if (students != null && students.Any())
            {
                foreach (var student in students)
                {
                    try
                    {
                        string emailContent = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                        border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                                <h2 style='color: #333; text-align: center;'>Task Status Updated</h2>
                                <p style='font-size: 16px; color: #555;'>Hello <strong>{student.FirstName}</strong>,</p>
                                <p style='font-size: 16px; color: #555;'>
                                    The status of the task <strong>'{existingTask.Title}'</strong> has been updated by 
                                    <strong>{User?.Identity?.Name ?? "an unknown user"}</strong>.
                                </p>
                                <p><strong>New Status:</strong> <span style='color: #007bff;'>{status}</span></p>
                                <p><strong>Description:</strong> {existingTask.Description}</p>
                                <p style='text-align: center;'>
                                    <a href='http://localhost:5173/workgroups/{workgroup.Id}/tasks' 
                                        style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                                        color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                        View Task Details
                                    </a>
                                </p>
                                <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                                    If you have any questions, please contact your supervisor.
                                </p>
                            </div>";

                        var emailSent = await emailService.SendEmailAsync(student.Email ?? "", "Task Status Updated", emailContent, true);

                        if (!emailSent.IsSuccess)
                        {
                            Console.WriteLine($"Error sending email to {student.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {student.Email}: {ex.Message}");
                    }
                }
            }


            return Ok(new ApiResponse(200, "Task status updated successfully.", result: status));
        }

        [HttpPost("{id}/submit-answer")]
        public async Task<ActionResult<ApiResponse>> SubmitAnswer(
            int id,
            [FromForm, Required] TaskSubmitDTO taskSubmitDTO)
        {
            if (taskSubmitDTO == null || taskSubmitDTO.File == null)
            {
                return BadRequest(new ApiResponse(400, "A valid file is required."));
            }

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new ApiResponse(401, "Student id not Find."));
            }

            //var task = await unitOfWork.taskRepository.GetById(id);

            Expression<Func<WorkgroupTask, bool>> filter = t => t.Id == id
                && t.Workgroup.Project != null &&
                    t.Workgroup.Project.UserProjects
                        .Any(u => u.Role == "student" && u.UserId == studentId);

            var tasks = await unitOfWork.taskRepository.GetAll(filter, "Workgroup.Project.UserProjects.User");
            var task = tasks.FirstOrDefault();
            if (task == null)
            {
                return NotFound(new ApiResponse(404, "Task or student not found."));
            }

            if(task.Status.ToLower() == "canceled" || task.Status.ToLower() == "completed")

            // Ensure the task is not end
            if (task.End <= DateTime.UtcNow)
            {
                return BadRequest(new ApiResponse(400, "The task is already finalized."));
            }

            var uploadedFiles = new List<FileDTO>();
            foreach (var file in taskSubmitDTO.File)
            {
                if (file.Length == 0)
                {
                    return BadRequest(new ApiResponse(400, "File is Empty."));
                }

                var fileDto = await _uploadHandler.UploadAsync(file, "submissions");
                if (fileDto.ErrorMessage != null)
                {
                    return BadRequest(new ApiResponse(400, fileDto.ErrorMessage));
                }
                uploadedFiles.Add(fileDto);
            }

            // Update task with the submitted file information
            foreach (var file in uploadedFiles)
            {
                await unitOfWork.fileRepository.Create(new WorkgroupFile()
                {
                    Path = file.Path ?? "",
                    Name = file.Name ?? "",
                    Type = "answer",
                    WorkgroupTask = task
                });
            }

            task.SubmittedBy = User?.Identity?.Name ?? "";
            task.Status = "submitted";  // Update task status to Submitted
            task.SubmittedAt = DateTime.UtcNow;

            unitOfWork.taskRepository.Update(task);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to save submission."));
            }

            var supervisors = task.Workgroup?.Project?.UserProjects
                .Where(u => (u.Role == "supervisor" || u.Role == "co-supervisor") && !u.IsDeleted
                             && u.User.EmailConfirmed && !string.IsNullOrWhiteSpace(u.User.Email))
                .Select(u => u.User)
                .ToList();

            if (supervisors != null && supervisors.Any())
            {
                foreach (var supervisor in supervisors)
                {
                    try
                    {
                        string emailContent = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                        border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                                <h2 style='color: #333; text-align: center;'>Task Submitted</h2>
                                <p style='font-size: 16px; color: #555;'>Hello <strong>{supervisor.FirstName}</strong>,</p>
                                <p style='font-size: 16px; color: #555;'>
                                    The task <strong>'{task.Title}'</strong> has been submitted by 
                                    <strong>{User?.Identity?.Name ?? "a student"}</strong>.
                                </p>
                                <p><strong>Submission Date:</strong> <span style='color: #007bff;'>{DateTime.UtcNow}</span></p>
                                <p><strong>Description:</strong> {task.Description}</p>
                                <p style='text-align: center;'>
                                    <a href='http://localhost:5173/workgroups/{task.WorkgroupId}/tasks' 
                                        style='display: inline-block; padding: 12px 20px; background-color: #28a745; 
                                        color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                        Review Submission
                                    </a>
                                </p>
                                <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                                    Please review the submission and provide feedback.
                                </p>
                            </div>";

                        var emailSent = await emailService.SendEmailAsync(supervisor.Email ?? "", "Task Submitted", emailContent, true);

                        if (!emailSent.IsSuccess)
                        {
                            Console.WriteLine($"Error sending email to {supervisor.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {supervisor.Email}: {ex.Message}");
                    }
                }
            }


            return Ok(new ApiResponse(200, "File submitted successfully."));
        }

        [Authorize(Roles = "supervisor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var task = await unitOfWork.taskRepository.GetById(id, "Workgroup.Project.UserProjects.User");

            int successDelete = unitOfWork.taskRepository.Delete(id);
            if (successDelete == 0)
            {
                return NotFound(new ApiResponse(404));
            }

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Deleted failed!"));
            }

            var students = task.Workgroup?.Project?.UserProjects
                .Where(up => !up.IsDeleted && up.Role == "student"
                        &&up.User.EmailConfirmed  && !string.IsNullOrWhiteSpace(up.User.Email))
                .Select(up => up.User)
                .ToList();

            if (students != null && students.Any())
            {
                foreach (var student in students)
                {
                    try
                    {
                        string emailContent = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                        border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                                <h2 style='color: #d9534f; text-align: center;'>Task Deleted</h2>
                                <p style='font-size: 16px; color: #555;'>Hello <strong>{student.FirstName}</strong>,</p>
                                <p style='font-size: 16px; color: #555;'>
                                    The task <strong>'{task.Title}'</strong> has been <span style='color: #d9534f;'>deleted</span> by 
                                    <strong>{User?.Identity?.Name ?? "an administrator"}</strong>.
                                </p>
                                <p><strong>Deleted On:</strong> <span style='color: #007bff;'>{DateTime.UtcNow}</span></p>
                                <p style='font-size: 14px; color: #777;'>
                                    If this was a mistake or you need further information, please contact your supervisor.
                                </p>
                            </div>";

                        var emailSent = await emailService.SendEmailAsync(student.Email ?? "", "Task Deleted", emailContent, true);

                        if (!emailSent.IsSuccess)
                        {
                            Console.WriteLine($"Error sending email to {student.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {student.Email}: {ex.Message}");
                    }
                }
            }


            return Ok(new ApiResponse(200, "Deleted Successfully"));
        }
    }
}
