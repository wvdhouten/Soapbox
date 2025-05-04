namespace Soapbox.Web.Blog.Archive;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Web.Models.Blog;

[Injectable]
public class GetMonthArchiveQuery
{
    private readonly IBlogRepository _blogService;

    public GetMonthArchiveQuery(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<ArchiveModel> ExecuteAsync(int year, int month)
    {
        var model = new ArchiveModel { Year = year, Month = month };

        var startOfMonth = new DateTime(year, month, 1);
        var startOffset = startOfMonth.DayOfWeek == model.StartOfWeek ? 7 : (7 + startOfMonth.DayOfWeek - model.StartOfWeek) % 7;
        var startOfCalendar = startOfMonth.AddDays(-startOffset);

        for (var day = 0; day < 42; day++)
            model.Days.Add(startOfCalendar.AddDays(day).Date, []);

        var posts = await _blogService.GetPostsAsync(post =>
            post.Status == PostStatus.Published
            && post.PublishedOn.Year == year
            && post.PublishedOn.Month == month);

        foreach (var post in posts)
            model.Days[post.PublishedOn.Date].Add(post);

        return model;
    }
}
