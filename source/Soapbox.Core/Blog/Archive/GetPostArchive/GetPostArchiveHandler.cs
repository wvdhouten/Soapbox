namespace Soapbox.Application.Blog.Archive.GetPostArchive;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Blog.Archive;
using Soapbox.Domain.Results;

[Injectable]
public class GetPostArchiveHandler
{
    private readonly IBlogRepository _blogService;

    public GetPostArchiveHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<PostArchive>> GetPostArchiveAsync(int year, int month)
    {
        if (year < 1)
            return Error.ValidationError("Bad Request.", new() { { year.ToString(), "Year must be greater than 0." } });

        return month switch
        {
            0 => await GetYearArchiveAsync(year),
            > 0 and < 13 => await GetMonthArchive(year, month),
            _ => Error.ValidationError("Bad Request.", new() { { year.ToString(), "Month must be between 1 and 12." } })
        };
    }

    private async Task<Result<PostArchive>> GetYearArchiveAsync(int year)
    {
        var archive = new PostArchive { Year = year, Month = null };
        var posts = await _blogService.GetPostsAsync(post => post.Status == PostStatus.Published && post.PublishedOn.Year == year);
        foreach (var post in posts)
        {
            if (!archive.Days.Any(x => x.Key == post.PublishedOn.Date))
                archive.Days.Add(post.PublishedOn.Date, []);

            archive.Days[post.PublishedOn.Date].Add(post);
        }

        return archive;
    }

    private async Task<Result<PostArchive>> GetMonthArchive(int year, int month)
    {
        var archive = new PostArchive { Year = year, Month = month };
        var startOfMonth = new DateTime(year, month, 1);
        var startOffset = startOfMonth.DayOfWeek == archive.StartOfWeek ? 7 : (7 + startOfMonth.DayOfWeek - archive.StartOfWeek) % 7;
        var startOfCalendar = startOfMonth.AddDays(-startOffset);

        for (var day = 0; day < 42; day++)
            archive.Days.Add(startOfCalendar.AddDays(day).Date, []);

        var posts = await _blogService.GetPostsAsync(post =>
            post.Status == PostStatus.Published
            && post.PublishedOn.Year == year
            && post.PublishedOn.Month == month);

        foreach (var post in posts)
            archive.Days[post.PublishedOn.Date].Add(post);

        return archive;
    }
}
