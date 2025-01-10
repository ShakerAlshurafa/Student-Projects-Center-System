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
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
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

        public TasksController(IUnitOfWork unitOfWork, IMapper mapper, AzureFileUploader uploadHandler)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            file = new StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task.WorkgroupFile();
            _uploadHandler = uploadHandler;
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
                    .Any(u => u.Role == "supervisor" && u.UserId == supervisorId);

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

            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId, "Project.UserProjects");
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


            var countCompleteTasks = await unitOfWork.taskRepository.Count(t =>
                t.WorkgroupId == workgroupId && t.Status.ToLower() == "complete");
            var countAllTasks = await unitOfWork.taskRepository.Count(t =>
                    t.WorkgroupId == workgroup.Id && t.Status.ToLower() != "canceled");

            if (countAllTasks == 0)
            {
                workgroup.Progress = 0;
            }
            else
            {
                workgroup.Progress = (int)(((double)countCompleteTasks / countAllTasks) * 100);
            }

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

        [Authorize]
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


        [Authorize(Roles = "supervisor")]
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

            status = status.ToLower();

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

            if (existingTask.Status.ToLower() == "complete" || status.ToLower() == "complete")
            {
                var workgroup = await unitOfWork.workgroupRepository.GetById(existingTask.WorkgroupId);
                if (workgroup == null)
                {
                    return NotFound($"Workgroup with ID {existingTask.WorkgroupId} was not found.");
                }

                var countCompleteTasks = await unitOfWork.taskRepository.Count(t =>
                    t.WorkgroupId == workgroup.Id && t.Status.ToLower() == "complete");
                var countAllTasks = await unitOfWork.taskRepository.Count(t => 
                    t.WorkgroupId == workgroup.Id && t.Status.ToLower() != "canceled");

                if (countAllTasks == 0)
                {
                    workgroup.Progress = 0;
                }
                else
                {
                    workgroup.Progress = (int)(((double)countCompleteTasks / countAllTasks) * 100);
                }

                unitOfWork.workgroupRepository.Update(workgroup); // Save the updated workgroup         
            }

            // Update the status
            existingTask.Status = status.ToLower();

            unitOfWork.taskRepository.Update(existingTask); // Save the updated task

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update task status."));
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

            var tasks = await unitOfWork.taskRepository.GetAll(filter, "Workgroup.Project.UserProjects");

            var task = tasks.FirstOrDefault();
            if (task == null)
            {
                return NotFound(new ApiResponse(404, "Task or student not found."));
            }

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

            return Ok(new ApiResponse(200, "File submitted successfully."));
        }

        [Authorize(Roles = "supervisor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
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

            return Ok(new ApiResponse(200, "Deleted Successfully"));
        }
    }
}
