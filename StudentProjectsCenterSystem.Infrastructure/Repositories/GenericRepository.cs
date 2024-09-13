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

        public int Delete(int id)
        {
            var model = dbContext.Set<T>().Find(id);
            if(model != null)
            {
                dbContext.Set<T>().Remove(model);
                return 1;
            }
            return 0;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            var models = await dbContext.Set<T>().ToListAsync();
            return models;
            //foreach (var model in models)
            //{
            //    yield return model;
            //}
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
    }
}
