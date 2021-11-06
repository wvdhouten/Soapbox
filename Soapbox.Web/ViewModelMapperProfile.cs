namespace Soapbox.Web
{
    using AutoMapper;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Posts;

    public class ViewModelMapperProfile : Profile
    {
        public ViewModelMapperProfile()
        {
            CreateMap<Post, PostViewModel>().ReverseMap();
            CreateMap<PostCategory, PostCategoryViewModel>().ReverseMap();
        }
    }
}
