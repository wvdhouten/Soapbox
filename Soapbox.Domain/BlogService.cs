namespace Soapbox.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soapbox.Core.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Models;

    public class BlogService : IBlogService
    {
        private readonly IPostRepository _postRepository;

        public BlogService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IAsyncEnumerable<Post>> GetAllPostsAsync()
        {
            return await _postRepository.ListAsync();
        }

        public async Task<IAsyncEnumerable<Post>> GetRecentPosts(int count)
        {
            return await _postRepository.ListAsync(0, count);
        }

        public async Task CreateOrUpdatePostAsync(Post post)
        {
            var existing = await _postRepository.GetByIdAsync(post.Id).ConfigureAwait(false) ?? post;
            existing.Categories.Clear();
            existing.Title = post.Title.Trim();
            existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : CreateSlug(post.Title);
            existing.Content = (post.Content ?? "").Trim();
            existing.Excerpt = (post.Excerpt ?? "").Trim();

            if (string.IsNullOrEmpty(existing.Id))
            {
                await _postRepository.CreateAsync(existing);
            }
            else
            {
                await _postRepository.UpdateAsync(existing);
            }
        }

        public async Task<Post> GetPostByIdAsync(string id)
        {
            return await _postRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task<Post> GetPostBySlugAsync(string slug)
        {
            return await _postRepository.GetByFilterAsync(post => post.Slug == slug.ToLowerInvariant()).ConfigureAwait(false);
        }

        public async Task DeletePostByIdAsync(string id)
        {
            await _postRepository.DeleteByIdAsync(id);
        }

        private static string CreateSlug(string title)
        {
            title = title?.ToLowerInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            title = StringUtils.RemoveDiacritics(title);
            title = StringUtils.RemoveReservedUrlCharacters(title);

            return title.ToLowerInvariant();
        }
    }
}
