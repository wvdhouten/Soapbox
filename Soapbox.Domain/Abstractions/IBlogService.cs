namespace Soapbox.Domain.Abstractions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soapbox.Models;

    public interface IBlogService
    {
        Task<IAsyncEnumerable<Post>> GetAllPostsAsync();

        Task<IAsyncEnumerable<Post>> GetRecentPosts(int count);

        Task CreateOrUpdatePostAsync(Post post);

        Task<Post> GetPostByIdAsync(string id);

        Task<Post> GetPostBySlugAsync(string slug);

        Task DeletePostByIdAsync(string id);
    }
}
