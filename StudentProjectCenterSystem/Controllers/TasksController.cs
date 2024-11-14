using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public TasksController(IUnitOfWork<WorkgroupTask> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([Required,FromQuery] int workgroupId, [FromForm] TaskCreateDto taskDto)
        {
            if (taskDto == null)
                return BadRequest(new ApiResponse(400, "Task data is required."));
            
            if (!taskDto.ValidExtensions.Any())
                return BadRequest(new ApiResponse(400, "Valid Extension is required."));

            string? filePath = null;
            string? fileName = null;

            if (taskDto.File != null)
            {
                UploadHandler uploadHandler = new UploadHandler(taskDto.ValidExtensions);
                fileName = await uploadHandler.UploadAsync(taskDto.File);

                if (fileName.StartsWith("File upload failed") || fileName.StartsWith("Invalid extension") || fileName == "File is empty.")
                {
                    return BadRequest(new ApiResponse(400, fileName));
                }

                filePath = Path.Combine("Files", fileName);
            }

            var task = new WorkgroupTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                //Status = taskDto.Status,
                Start = taskDto.Start,
                End = taskDto.End,
                WorkgroupId = workgroupId,
               FilePath = filePath
            };

            await unitOfWork.taskRepository.Create(task);
            
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            // add create response dto

            return CreatedAtAction(nameof(Create), new { id = task.Id }, new ApiResponse(201, result: task));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromForm] TaskUpdateDTO taskDto)
        {
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

            string? filePath = existingTask.FilePath; 
            string? fileName;

            if (taskDto.File != null)
            {
                if (!taskDto.ValidExtensions.Any())
                    return BadRequest(new ApiResponse(400, "Valid extensions are required for the file."));

                UploadHandler uploadHandler = new UploadHandler(taskDto.ValidExtensions);
                fileName = await uploadHandler.UploadAsync(taskDto.File);

                if (fileName.StartsWith("File upload failed") || fileName.StartsWith("Invalid extension") || fileName == "File is empty.")
                {
                    return BadRequest(new ApiResponse(400, fileName));
                }

                filePath = Path.Combine("Files", fileName);
            }

            existingTask.FilePath = filePath;

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
        public async Task<IActionResult> GetTask(int id)
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
                FilePath = task.FilePath,
                Status = task.Status,
            };

            return Ok(new ApiResponse(200, result: taskDto));
        }
    
    

    }
}
