namespace Soapbox.Models
{
    using System;
    using System.Collections.Generic;

    public class PagedList<T> : IPagedList<T>
    {
        public IList<T> Items { get; set; } = new List<T>();

        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int PageCount
        {
            get
            {
                var pageCount = (double)Total / PageSize;
                return (int)Math.Ceiling(pageCount);
            }
        }
    }
}
