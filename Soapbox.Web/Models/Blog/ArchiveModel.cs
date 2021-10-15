namespace Soapbox.Web.Models.Blog
{
    using System;
    using System.Collections.Generic;
    using Soapbox.Models;

    public class ArchiveModel
    {
        public DayOfWeek StartOfWeek { get; set; } = DayOfWeek.Monday;

        public DateTime CurrentMonth { get; set; } = DateTime.Now;

        public IDictionary<DateTime, ICollection<Post>> Days { get; } = new Dictionary<DateTime, ICollection<Post>>();
    }
}
