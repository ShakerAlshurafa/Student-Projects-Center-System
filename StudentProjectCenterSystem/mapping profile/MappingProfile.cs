using AutoMapper;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.mapping_profile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectDTO>().ReverseMap();
            CreateMap<Project, ProjectDetailsDTO>().ReverseMap();

            CreateMap<Project, ProjectCreateDTO>()
                .ForMember(dest => dest.ProjectDetails, opt => opt.MapFrom(src => src.ProjectDetails))
                .ReverseMap()
                .ForMember(dest => dest.ProjectDetails, opt => opt.MapFrom(src => src.ProjectDetails));

            CreateMap<ProjectDetails, ProjectDetailsCreateDTO>().ReverseMap();
            CreateMap<Project, ProjectUpdateDTO>().ReverseMap();

            CreateMap<LocalUser, LocalUserDTO>().ReverseMap();
        }
            
    }
}
