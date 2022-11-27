namespace Soapbox.Models
{
    using System.Collections.Generic;

    public interface IPagedList<T> : IPagingData
    {
        IList<T> Items { get; set; }
    }
}