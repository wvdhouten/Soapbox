namespace Soapbox.Domain.Blog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
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

        public async Task<IAsyncEnumerable<Post>> GetPostsAsync(int count, int skip = 0)
        {
            return await _postRepository.ListAsync(skip, count);
        }

        public async Task<IAsyncEnumerable<Post>> GetPostsByAuthorAsync(string id)
        {
            return await _postRepository.ListAsync(p => p.Author.Id == id);
        }

        public async Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate)
        {
            return await _postRepository.ListAsync(predicate);
        }

        public async Task CreateOrUpdatePostAsync(Post post)
        {
            var existing = await _postRepository.GetByIdAsync(post.Id).ConfigureAwait(false) ?? post;
            existing.Title = post.Title.Trim();
            existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : CreateSlug(post.Title);
            existing.ModifiedOn = DateTime.UtcNow;
            existing.PublishedOn = post.PublishedOn;
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
            title = title.RemoveDiacritics();
            title = title.RemoveReservedUrlCharacters();

            return title.ToLowerInvariant();
        }

        public async Task<IAsyncEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
        {
            return await _postRepository.GetCategoriesAsync(includePosts);
        }

        public async Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false)
        {
            return await _postRepository.GetCategoryBySlug(slug, includePosts);
        }

        public async Task<IAsyncEnumerable<Post>> GetPostsByCategoryAsync(long id)
        {
            return await _postRepository.ListAsync(p => p.Categories.Any(c => c.Id == id));
        }

        public async Task<SoapboxUser> GetAuthorByIdAsync(string id)
        {
            return await _postRepository.GetUserByIdAsync(id);
        }
    }
}
