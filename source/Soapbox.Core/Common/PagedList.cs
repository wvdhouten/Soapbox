namespace Soapbox.Application.Common;

using System;
using System.Collections.Generic;
using Soapbox.Domain.Common;

public class PagedList<T> : IPagedList<T>
{
    public IList<T> Items { get; set; } = [];

    public int Total { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int PageCount => (int)Math.Ceiling((double)Total / PageSize);
}
