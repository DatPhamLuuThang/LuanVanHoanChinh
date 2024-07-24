using AutoMapper;
using CoreEntities.RequestModel;
using CoreEntities.SchoolMgntModel;

namespace CoreEntities;

public class MappingProfile : Profile {
    public MappingProfile() {

        CreateMap<Student, CreateStudentRerquest>();
        CreateMap<CreateStudentRerquest, Student>();
    }
}