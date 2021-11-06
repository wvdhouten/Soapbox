namespace Soapbox.Models
{
    using System.Collections.Generic;

    public class PostCategory
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public IList<Post> Posts { get; set; } = new List<Post>();
    }
}