namespace Soapbox.Domain.Common;

using System.Collections.Generic;

public interface IPagedList<T> : IPagingData
{
    public IList<T> Items { get; set; }
}