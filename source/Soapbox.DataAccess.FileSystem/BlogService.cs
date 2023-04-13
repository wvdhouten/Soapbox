namespace Soapbox.DataAccess.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Soapbox.Core.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;

    public class BlogService : IBlogService
    {
        private readonly IBlogStore _blogStore;
        private readonly IUserStore<SoapboxUser> _userStore;

        private IQueryable<Post> Posts => _blogStore.Posts.AsQueryable();

        private IQueryable<PostCategory> Categories => _blogStore.Categories.AsQueryable();

        public BlogService(IBlogStore blogStore, IUserStore<SoapboxUser> userStore)
        {
            _blogStore = blogStore;
            _userStore = userStore;
        }

        public Task<IPagedList<Post>> GetPostsPageAsync(int page = 0, int pageSize = 25, bool isPublished = true)
        {
            var posts = Posts;

            if (isPublished)
            {
                var now = DateTime.UtcNow;
                posts = posts.Where(p => p.Status == PostStatus.Published && p.PublishedOn < now);
            }

            posts = posts.OrderByDescending(post => post.PublishedOn);

            return Task.FromResult(posts.GetPaged(page, pageSize));
        }

        public Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate)
        {
            return Task.FromResult(Posts.Where(predicate).ToAsyncEnumerable());
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByCategoryAsync(long categoryId)
        {
            return Task.FromResult(Posts.Where(p => p.Categories.Any(c => c.Id == categoryId)).ToAsyncEnumerable());
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByAuthorAsync(string authorId)
        {
            return Task.FromResult(Posts.Where(p => p.Author.Id == authorId).ToAsyncEnumerable());
        }

        public Task<Post> GetPostByIdAsync(string postId)
        {
            return Task.FromResult(Posts.FirstOrDefault(p => p.Id == postId));
        }

        public Task<Post> GetPostBySlugAsync(string slug)
        {
            return Task.FromResult(Posts.FirstOrDefault(p => p.Slug == slug.ToLowerInvariant()));
        }

        public Task CreatePostAsync(Post post)
        {
            _blogStore.UpdatePost(post);

            return Task.CompletedTask;
        }

        public Task UpdatePostAsync(Post post)
        {
            _blogStore.UpdatePost(post);

            return Task.CompletedTask;
        }

        public Task DeletePostByIdAsync(string postId)
        {
            _blogStore.DeletePost(postId);

            return Task.CompletedTask;
        }

        public Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false)
        {
            return Task.FromResult(Categories.GetPaged(page, pageSize));
        }

        public Task<IAsyncEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
        {
            return Task.FromResult(Categories.ToAsyncEnumerable());
        }

        public Task<PostCategory> GetCategoryByIdAsync(long categoryId, bool includePosts = false)
        {
            return Task.FromResult(Categories.FirstOrDefault(category => category.Id == categoryId));
        }

        public Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false)
        {
            return Task.FromResult(Categories.FirstOrDefault(category => category.Slug == slug.ToLowerInvariant()));
        }

        public Task CreateCategoryAsync(PostCategory category)
        {
            _blogStore.Categories.Add(category);

            return Task.CompletedTask;
        }

        public Task UpdateCategoryAsync(PostCategory category)
        {
            var existingCategory = _blogStore.Categories.FirstOrDefault(c => c.Id == category.Id);

            var list = new List<PostCategory>();
            _blogStore.Categories.Add(category);

            return Task.CompletedTask;
        }

        public Task DeleteCategoryByIdAsync(long categoryId)
        {
            throw new NotImplementedException();
        }

        public async Task<SoapboxUser> GetAuthorByIdAsync(string authorId)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            return await _userStore.FindByIdAsync(authorId, cts.Token);
        }
    }
}
