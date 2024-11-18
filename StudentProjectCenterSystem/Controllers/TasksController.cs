using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using StudentProjectsCenterSystem.Infrastructure.Utilities;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IUnitOfWork<WorkgroupTask> unitOfWork;
        private FileDTO file;

        public TasksController(IUnitOfWork<WorkgroupTask> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            file = new FileDTO();
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

            if (taskDto.File != null)
            {
                UploadHandler uploadHandler = new UploadHandler();

                file = await uploadHandler.UploadAsync(taskDto.File);

                if (file.ErrorMessage != null)
                {
                    return BadRequest(new ApiResponse(400, file.ErrorMessage));
                }

            }

            var task = new WorkgroupTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Start = taskDto.Start,
                End = taskDto.End,
                WorkgroupId = workgroupId,
                QuestionFilePath = file.FilePath,
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

                // Delete the old file if it exists
                if (!string.IsNullOrEmpty(existingTask.QuestionFilePath) && System.IO.File.Exists(existingTask.QuestionFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(existingTask.QuestionFilePath);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new ApiResponse(500, $"Failed to delete the old file: {ex.Message}"));
                    }
                }

                UploadHandler uploadHandler = new UploadHandler();
                file = await uploadHandler.UploadAsync(taskDto.QuestionFile);

                if (file.ErrorMessage != null)
                {
                    return BadRequest(new ApiResponse(400, file.FileName));
                }

                 existingTask.QuestionFilePath = file.FilePath;
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
            if (taskSubmitDTO == null || taskSubmitDTO.File == null || taskSubmitDTO.File.Length == 0)
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
                || task.Status.ToLower() == "on hold" || task.Status.ToLower() == "canceled")
            {
                return BadRequest(new ApiResponse(400, "The task is already finalized or in a non-submittable state, and cannot accept new submissions."));
            }


            UploadHandler uploadHandler = new UploadHandler();
            var uploadedFile = await uploadHandler.UploadAsync(taskSubmitDTO.File);
            if (uploadedFile.ErrorMessage != null)
            {
                return BadRequest(new ApiResponse(400, uploadedFile.ErrorMessage));
            }

            // Update task with the submitted file information
            task.SubmittedFilePath = uploadedFile.FilePath;  // Store the submitted file path
            task.Status = "Submitted";  // Update task status to Submitted

            unitOfWork.taskRepository.Update(task);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to save submission."));
            }

            return Ok(new ApiResponse(200, "File submitted successfully.", result: task.SubmittedFilePath));
        }

    }
}
