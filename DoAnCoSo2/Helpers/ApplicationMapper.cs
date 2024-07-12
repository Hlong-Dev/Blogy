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
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            CreateMap<BlogModel, Blog>().ReverseMap();
            CreateMap<CategoryModel, Category>().ReverseMap();
            CreateMap<Blog, BlogModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.CategorySlug))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.ViewCount))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
                .ReverseMap();
        }
    }
}
