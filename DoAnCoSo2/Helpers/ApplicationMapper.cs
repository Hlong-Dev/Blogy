using AutoMapper;
using DoAnCoSo2.Data;
using DoAnCoSo2.Models;

namespace DoAnCoSo2.Helpers
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<Blog, BlogModel>().ReverseMap();    
        }
    }
}
