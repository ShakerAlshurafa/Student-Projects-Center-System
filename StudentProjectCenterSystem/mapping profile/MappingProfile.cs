﻿using AutoMapper;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Message;
using StudentProjectsCenter.Core.Entities.DTO.Profile;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetailsSection;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.mapping_profile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectDetailsDTO>().ReverseMap();

            // change => enter details from my project
            //CreateMap<Project, ProjectCreateDTO>()
            //    .ForMember(dest => dest.ProjectDetails, opt => opt.MapFrom(src => src.ProjectDetails))
            //    .ReverseMap()
            //    .ForMember(dest => dest.ProjectDetails, opt => opt.MapFrom(src => src.ProjectDetails));

            CreateMap<ProjectDetailEntity, ProjectDetailsEditDTO>().ReverseMap();
            CreateMap<Project, ProjectUpdateDTO>().ReverseMap();
            CreateMap<Project, MyProjectDTO>().ReverseMap();

            CreateMap<ProjectDetailsSection, ProjectDetailsSectionDTO>().ReverseMap();
            CreateMap<ProjectDetailsSection, ProjectDetailsSectionCreateDTO>().ReverseMap();

            CreateMap<LocalUser, LocalUserDTO>().ReverseMap();
            CreateMap<LocalUser, UserDTO>().ReverseMap();
            CreateMap<LocalUser, UserInfoDto>().ReverseMap();
            CreateMap<LocalUser, GetByRoleDTO>().ReverseMap();

            CreateMap<LocalUser, UserProfileDTO>().ReverseMap();

            CreateMap<WorkgroupTask, AllTaskDTO>().ReverseMap();
            CreateMap<WorkgroupTask, AllWorkgroupTaskDTO>().ReverseMap();

            CreateMap<Message, MessageDTO>().ReverseMap();

            CreateMap<Celender, CreateCelenderEventDTO>().ReverseMap();
            CreateMap<Celender, CelenderEventDTO>().ReverseMap();

            CreateMap<OurCustomerDTO, LocalUser>().ReverseMap();
        }

    }
}
