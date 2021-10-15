namespace Soapbox.Domain.Blog
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Soapbox.Models;

    public interface IBlogService
    {
        Task<IAsyncEnumerable<Post>> GetAllPostsAsync();

        Task<IAsyncEnumerable<Post>> GetPostsAsync(int count, int skip = 0);

        Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate);

        Task CreateOrUpdatePostAsync(Post post);

        Task<Post> GetPostByIdAsync(string id);

        Task<Post> GetPostBySlugAsync(string slug);

        Task DeletePostByIdAsync(string id);
    }
}
