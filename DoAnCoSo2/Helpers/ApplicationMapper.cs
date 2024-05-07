using AutoMapper;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;

namespace DoAnCoSo2.Helpers
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<Blog, BlogModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id));
            CreateMap<BlogModel, Blog>().ReverseMap();
            CreateMap<CategoryModel, Category>().ReverseMap();
        }
    }
}
