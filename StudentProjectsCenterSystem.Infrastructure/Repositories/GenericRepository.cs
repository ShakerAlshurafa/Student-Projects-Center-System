using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System.Linq.Expressions;


namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Create(T model)
        {
            await dbContext.Set<T>().AddAsync(model);
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter, string? includeProperty = null)
        {

            IQueryable<T> query = dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includeProperty))
            {
                foreach (var property in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, int page_size = 6, int page_number = 1, string? includeProperty = null)
        {
            if (page_size <= 0)
            {
                page_size = 6;
            }else if(page_size > 45)
            {
                page_size = 45;
            }

            if (page_number <= 0)
            {
                page_number = 1; 
            }

            IQueryable<T> query = dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includeProperty))
            {
                foreach(var property in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            if(filter != null)
            {
                query = query.Where(filter);
            }

            var models = await query
                .Skip((page_number - 1) * page_size)
                .Take(page_size)
                .ToListAsync();

            return models;
        }

        public async Task<T?> GetById(int id, string? includeProperty = null)
        {
            IQueryable<T> query = dbContext.Set<T>();

            // Include related properties if provided
            if (!string.IsNullOrEmpty(includeProperty))
            {
                foreach (var property in includeProperty.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            // Find the entity by ID
            return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
        }


        public void Update(T model)
        {
            dbContext.Set<T>().Update(model);
        }

        public int Delete(int id)
        {
            var model = dbContext.Set<T>().Find(id);
            if (model != null)
            {
                dbContext.Set<T>().Remove(model);
                return 1;
            }
            return 0;
        }

        public async Task<bool> IsExist(int id)
        {
            var model = await dbContext.Set<T>().FindAsync(id);
            if (model != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsEmpty(Expression<Func<T, bool>>? filter)
        {
            return await dbContext.Set<T>().CountAsync(filter ?? (x => true)) == 0;
        }


        public async Task<int> Count(Expression<Func<T, bool>>? filter)
        {
            IQueryable<T> query = dbContext.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }
    }
}
