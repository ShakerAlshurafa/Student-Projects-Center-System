using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<T>> GetAll(int page_size = 6, int page_number = 1, string? includeProperty = null)
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

            var models = await query
                .Skip((page_number - 1) * page_size)
                .Take(page_size)
                .ToListAsync();

            return models;
        }

        public async Task<T> GetById(int id)
        {
            var model = await dbContext.Set<T>().FindAsync(id);
            return model;
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
    }
}
