using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;

namespace StudentProjectsCenter.Services
{
    public class StatusUpdateService
    {
        private readonly IStatusUpdater<WorkgroupTask> taskStatusUpdater;
        private readonly IStatusUpdater<LocalUser> userStatusUpdater;

        public StatusUpdateService(
            IStatusUpdater<WorkgroupTask> taskStatusUpdater,
            IStatusUpdater<LocalUser> userStatusUpdater)
        {
            this.taskStatusUpdater = taskStatusUpdater;
            this.userStatusUpdater = userStatusUpdater;
        }

        // Method to update the status of a Task
        public async Task UpdateTaskStatusAsync()
        {
            await taskStatusUpdater.UpdateAsync(
                t =>
                    t.Status.ToLower() == "not started"
                    && t.Start <= DateTime.UtcNow,
                "Status",
                "in progress"
            );
            await taskStatusUpdater.UpdateAsync(
                t =>
                    (t.Status.ToLower() == "in progress" || t.Status.ToLower() == "rejected")
                    && t.End <= DateTime.UtcNow,
                "Status",
                "overdue"
            );
        }


        // Method to delete the user not confirm email
        public async Task DeleteUserAsync()
        {
            await userStatusUpdater.DeleteAsync(u => !u.EmailConfirmed);
        }
    }
}
