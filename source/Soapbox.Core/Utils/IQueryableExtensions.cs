namespace Soapbox.Application.Utils;

using System.Linq;
using Soapbox.Domain;
using Soapbox.Domain.Common;

public static class IQueryableExtensions
{
    public static IPagedList<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize) where T : class
    {
        var result = new PagedList<T>
        {
            Page = page,
            PageSize = pageSize,
            Total = query.Count()
        };

        var skip = (page - 1) * pageSize;
        result.Items = [.. query.Skip(skip).Take(pageSize)];

        return result;
    }
}
