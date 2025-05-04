namespace Soapbox.Web.Blog.Archive
{
    using Alkaline64.Injectable;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Domain.Blog;
    using Soapbox.Web.Models.Blog;

    [Injectable]
    public class GetYearArchiveQuery
    {
        private readonly IBlogRepository _blogService;

        public GetYearArchiveQuery(IBlogRepository blogService)
        {
            _blogService = blogService;
        }

        public async Task<ArchiveModel> ExecuteAsync(int year)
        {
            var model = new ArchiveModel { Year = year, Month = null };

            var posts = await _blogService.GetPostsAsync(post => post.Status == PostStatus.Published && post.PublishedOn.Year == year);
            foreach (var post in posts)
            {
                if (!model.Days.Any(x => x.Key == post.PublishedOn.Date))
                    model.Days.Add(post.PublishedOn.Date, []);

                model.Days[post.PublishedOn.Date].Add(post);
            }

            return model;
        }
    }
}
