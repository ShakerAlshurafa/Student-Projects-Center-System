using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Infrastructure.Repositories
{
    public class StatusUpdater<T> : IStatusUpdater<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;

        public StatusUpdater(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task UpdateAsync(Expression<Func<T, bool>>? expression, string propertyName, object newData)
        {
            if (expression == null)
            {
                throw new ArgumentException("No Expression Found!");
            }
            
            var items = dbContext.Set<T>()
                .Where(expression)
                .ToList();

            // Use reflection to get the PropertyInfo for the field to update
            var property = typeof(T).GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"Property {propertyName} does not exist on type {typeof(T).Name}");
            }

            // Ensure the property is writable (not readonly)
            if (!property.CanWrite)
            {
                throw new InvalidOperationException($"Property {propertyName} is not writable.");
            }

            foreach (var item in items)
            {
                // Set the value of the specified propertyName to the new data
                property.SetValue(item, Convert.ChangeType(newData, property.PropertyType));
            }

            await dbContext.SaveChangesAsync();
        }


        public async Task DeleteAsync(Expression<Func<T, bool>>? expression)
        {
            if (expression == null)
            {
                throw new ArgumentException("No Expression Found!");
            }

            var items = dbContext.Set<T>()
                .Where(expression)
                .ToList();

            dbContext.Set<T>().RemoveRange(items);
            await dbContext.SaveChangesAsync();
        }
    }
}
