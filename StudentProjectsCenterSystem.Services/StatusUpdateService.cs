using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Services
{
    public class StatusUpdateService
    {
        private readonly IStatusUpdater<WorkgroupTask> taskStatusUpdater;

        public StatusUpdateService(IStatusUpdater<WorkgroupTask> taskStatusUpdater)
        {
            this.taskStatusUpdater = taskStatusUpdater;
        }

        // Method to update the status of a Task
        public async Task UpdateTaskStatusAsync()
        {
            await taskStatusUpdater.UpdateAsync(
                t => 
                    t.Status.ToLower() == "not started" 
                    && t.Start >= DateTime.UtcNow, 
                "Status", 
                "In Progress"
            );
            await taskStatusUpdater.UpdateAsync(
                t => 
                    (t.Status.ToLower() == "in progress" || t.Status.ToLower() == "rejected") 
                    && t.End <= DateTime.UtcNow,
                "Status",
                "Overdue"
            );
        }
    }
}
