namespace Soapbox.Web.Helpers
{
    using AutoMapper;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Categories;
    using Soapbox.Web.Areas.Admin.Models.Posts;

    public class ViewModelMapperProfile : Profile
    {
        public ViewModelMapperProfile()
        {
            CreateMap<Post, PostViewModel>().ReverseMap();
            CreateMap<PostCategory, SelectableCategoryViewModel>().ReverseMap();
            CreateMap<PostCategory, PostCategoryViewModel>().ReverseMap();
        }
    }
}
