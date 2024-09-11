using AutoMapper;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.mapping_profile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectDTO>();
                //.ForMember(To => To.Name, from => from.MapFrom(x => x.Name != null ? x. . : null));
        }
            
    }
}
