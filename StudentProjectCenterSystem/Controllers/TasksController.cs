using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IUnitOfWork<WorkgroupTask> unitOfWork;
        private readonly IMapper mapper;
        private FileDTO file;
        private readonly AzureFileUploader _uploadHandler;

        public TasksController(IUnitOfWork<WorkgroupTask> unitOfWork, IMapper mapper, AzureFileUploader uploadHandler)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            file = new FileDTO();
            _uploadHandler = uploadHandler;
        }


        [HttpGet("all-tasks")]
        public async Task<ActionResult<ApiResponse>> GetAllTasks(
            [Required] int workgroupId,
            [FromQuery] int PageSize = 6,
            [FromQuery] int PageNumber = 1)
        {
            // Validate workgroup existence
            var workgroupExists = await unitOfWork.workgroupRepository.IsExist(workgroupId);
            if (!workgroupExists)
            {
                return NotFound(new ApiResponse(404, "Workgroup not found."));
            }

            // Fetch tasks for the specified workgroup with pagination
            var tasks = await unitOfWork.taskRepository.GetAll(
                filter: t => t.WorkgroupId == workgroupId,
                page_size: PageSize,
                page_number: PageNumber);

            // Check if tasks are available
            if (tasks == null || !tasks.Any())
            {
                return Ok(new ApiResponse(200, "No tasks found for the specified workgroup."));
            }

            var taskDto = mapper.Map<List<AllTaskDTO>>(tasks);

            return Ok(new ApiResponse(200, "Tasks retrieved successfully.", taskDto));
        }



        [HttpPost("workgroupId")]
        public async Task<ActionResult<ApiResponse>> Create([Required] int workgroupId, [FromForm] TaskCreateDto taskDto)
        {
            if (taskDto == null)
                return BadRequest(new ApiResponse(400, "Task data is required."));

            //if (!taskDto.ValidExtensions.Any())
            //    return BadRequest(new ApiResponse(400, "Valid extensions are required."));

            // Check if dates are in the future
            if (taskDto.Start < DateTime.UtcNow || taskDto.End < DateTime.UtcNow)
                return BadRequest(new ApiResponse(400, "Dates must not be in the past."));

            // Check if Start and End dates are valid
            if (taskDto.Start >= taskDto.End)
                return BadRequest(new ApiResponse(400, "The start date must be earlier than the end date."));

            var uploadedFiles = new List<FileDTO>();
            if (taskDto.File != null)
            {
                foreach(var file in taskDto.File)
                {
                    if(file.Length == 0)
                    {
                        return BadRequest(new ApiResponse(400, "File is Empty."));
                    }
                    var fileDto = await _uploadHandler.UploadAsync(file, "resources");

                    if (fileDto.ErrorMessage != null)
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
                QuestionFilePath = uploadedFiles.Select(f => f.FilePath).ToList(),
                //FileName = file.FileName,
            };

            await unitOfWork.taskRepository.Create(task);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = task.Id }, new ApiResponse(201, "Task created successfully", result: task));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromForm] TaskUpdateDTO taskDto)
        {
            // Validate taskDto
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid data."));
            }

            // Fetch the task from the database
            var existingTask = await unitOfWork.taskRepository.GetById(id);
            if (existingTask == null)
            {
                return NotFound(new ApiResponse(404, "Task not found."));
            }

            // Update properties
            existingTask.Title = taskDto.Title ?? existingTask.Title;
            existingTask.Description = taskDto.Description ?? existingTask.Description;
            existingTask.Start = taskDto.Start ?? existingTask.Start;
            existingTask.End = taskDto.End ?? existingTask.End;

            // Check if dates are in the future
            if (existingTask.Start < DateTime.UtcNow || existingTask.End < DateTime.UtcNow)
                return BadRequest(new ApiResponse(400, "Dates must not be in the past."));

            // Check if Start and End dates are valid
            if (existingTask.Start >= existingTask.End)
                return BadRequest(new ApiResponse(400, "The start date must be earlier than the end date."));


            if (taskDto.QuestionFile != null)
            {
                //if (!taskDto.ValidExtensions.Any())
                //    return BadRequest(new ApiResponse(400, "Valid extensions are required for the file."));

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

                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(fileDto.FilePath) && System.IO.File.Exists(fileDto.FilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(fileDto.FilePath);
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, new ApiResponse(500, $"Failed to delete the old file: {ex.Message}"));
                            }
                        }

                        if (fileDto.ErrorMessage != null)
                        {
                            return BadRequest(new ApiResponse(400, fileDto.ErrorMessage));
                        }

                        uploadedFiles.Add(fileDto);
                    }
                }

                existingTask.QuestionFilePath = uploadedFiles.Select(f => f.FilePath).ToList();
            }

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
            var task = await unitOfWork.taskRepository.GetById(id);
            if (task == null)
            {
                return NotFound(new ApiResponse(404, "Task not found"));
            }

            var taskDto = new TaskDTO()
            {
                Title = task.Title,
                Description = task.Description,
                Start = task.Start,
                End = task.End,
                QuestionFilePath = task.QuestionFilePath,
                SubmittedFilePath = task.SubmittedFilePath,
                //FileName = task.FileName,
                Status = task.Status
            };

            return Ok(new ApiResponse(200, result: taskDto));
        }


        [HttpPut("{id}/change-status")]
        public async Task<ActionResult<ApiResponse>> ChangeStatus([Required] int id, [Required, FromBody] string status)
        {
            // Validate the status input
            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new ApiResponse(400, "Status is required."));
            }

            // Retrieve the task from the repository
            var existingTask = await unitOfWork.taskRepository.GetById(id);
            if (existingTask == null)
            {
                return NotFound(new ApiResponse(404, "Task not found."));
            }

            // Update the status
            existingTask.Status = status;

            // Save the updated task
            unitOfWork.taskRepository.Update(existingTask);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update task status."));
            }

            return Ok(new ApiResponse(200, "Task status updated successfully.", result: status));
        }


        [HttpPost("{id}/submit-answer")]
        public async Task<ActionResult<ApiResponse>> SubmitAnswer([Required] int id, [FromForm] TaskSubmitDTO taskSubmitDTO)
        {
            if (taskSubmitDTO == null || taskSubmitDTO.File == null)
            {
                return BadRequest(new ApiResponse(400, "A valid file is required."));
            }

            var task = await unitOfWork.taskRepository.GetById(id);
            if (task == null)
            {
                return NotFound(new ApiResponse(404, "Task not found."));
            }

            // Ensure the task is not already submitted or in an invalid state
            if (task.Status.ToLower() == "submitted" || task.Status.ToLower() == "approved"
                || task.Status.ToLower() == "on hold" || task.Status.ToLower() == "canceled"
                || task.Status.ToLower() == "completed")
            {
                return BadRequest(new ApiResponse(400, "The task is already finalized or in a non-submittable state, and cannot accept new submissions."));
            }


            var uploadedFiles = new List<FileDTO>();
            foreach(var file in taskSubmitDTO.File)
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
            task.SubmittedFilePath = uploadedFiles.Select(f => f.FilePath).ToList();  // Store the submitted file path
            task.Status = "Submitted";  // Update task status to Submitted

            unitOfWork.taskRepository.Update(task);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to save submission."));
            }

            return Ok(new ApiResponse(200, "File submitted successfully.", result: task.SubmittedFilePath));
        }


        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> Delete([Required] int id)
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
