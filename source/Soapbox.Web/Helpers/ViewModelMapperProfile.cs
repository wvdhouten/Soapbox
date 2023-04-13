namespace Soapbox.Web.Helpers
{
    using System.Linq;
    using AutoMapper;
    using Soapbox.DataAccess.FileSystem.Models;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Categories;
    using Soapbox.Web.Areas.Admin.Models.Posts;

    public class ViewModelMapperProfile : Profile
    {
        public ViewModelMapperProfile()
        {
            CreateMap<Post, PostViewModel>().ReverseMap();
            CreateMap<Post, PostRecord>()
                .ForMember(target => target.AuthorId, options => options.MapFrom(source => source.Author.Id))
                .ForMember(target => target.Categories, options => options.MapFrom(source => source.Categories.Select(c => c.Id)))
                .ReverseMap()
                .ForMember(target => target.Author, options => options.MapFrom(source => new SoapboxUser { Id = source.AuthorId }))
                .ForMember(target => target.Categories, options => options.MapFrom(source => source.Categories.Select(c => new PostCategory { Name = c })));
            CreateMap<PostCategory, SelectableCategoryViewModel>().ReverseMap();
            CreateMap<PostCategory, PostCategoryViewModel>().ReverseMap();
        }
    }
}
