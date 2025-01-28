using System.Linq.Expressions;

namespace StudentProjectsCenter.Core.IRepositories
{
    public interface IStatusUpdater<T> where T : class
    {
        public Task DeleteAsync(Expression<Func<T, bool>>? expression);
        public Task UpdateAsync(Expression<Func<T, bool>>? expression, string propertyName, object newData);
    }
}
