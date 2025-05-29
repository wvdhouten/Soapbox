namespace Soapbox.Application.Utils;

using System.Linq;
using Soapbox.Application.Common;
using Soapbox.Domain.Common;

public static class IQueryableExtensions
{
    public static IPagedList<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize) where T : class
    {
        var skip = (page - 1) * pageSize;
        var result = new PagedList<T>
        {
            Page = page,
            PageSize = pageSize,
            Total = query.Count(),
            Items = [.. query.Skip(skip).Take(pageSize)]
        };

        return result;
    }
}
