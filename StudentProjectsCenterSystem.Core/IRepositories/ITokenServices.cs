using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface ITokenServices
    {
        Task<string> CreateTokenAsync(LocalUser localUser);
    }
}
