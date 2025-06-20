namespace Soapbox.DataAccess.FileSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.DataAccess.FileSystem.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Users;

[Injectable<IBlogRepository>]
public class BlogRepository : IBlogRepository
{
    private readonly IBlogStore _blogStore;
    private readonly IUserStore<SoapboxUser> _userStore;

    private IQueryable<Post> Posts => _blogStore.Posts.AsQueryable();

    private IQueryable<PostCategory> Categories => _blogStore.Categories.AsQueryable();

    public BlogRepository(IBlogStore blogStore, IUserStore<SoapboxUser> userStore)
    {
        _blogStore = blogStore;
        _userStore = userStore;
    }

    public Task<IPagedList<Post>> GetPostsPageAsync(int page = 1, int pageSize = 25, bool isPublished = true)
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

    public Task<IEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate) 
        => Task.FromResult(Posts.Where(predicate).AsEnumerable());

    public Task<IEnumerable<Post>> GetPostsByCategoryAsync(string categoryId) 
        => Task.FromResult(Posts.Where(p => p.Categories.Any(c => c.Id == categoryId)).AsEnumerable());

    public Task<IEnumerable<Post>> GetPostsByAuthorAsync(string authorId) 
        => Task.FromResult(Posts.Where(p => p.Author.Id == authorId).AsEnumerable());

    public Task<Post?> GetPostByIdAsync(string postId) 
        => Task.FromResult(Posts.FirstOrDefault(p => p.Id == postId));

    public Task<Post?> GetPostBySlugAsync(string slug) 
        => Task.FromResult(Posts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase)));

    public Task CreatePostAsync(Post post)
    {
        _blogStore.StorePost(post);

        return Task.CompletedTask;
    }

    public Task UpdatePostAsync(Post post)
    {
        _blogStore.StorePost(post);

        return Task.CompletedTask;
    }

    public Task DeletePostByIdAsync(string postId)
    {
        _blogStore.DeletePost(postId);

        return Task.CompletedTask;
    }

    public Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false)
        => Task.FromResult(Categories.GetPaged(page, pageSize));

    public Task<IEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
        => Task.FromResult(Categories.AsEnumerable());

    public Task<PostCategory?> GetCategoryByIdAsync(string categoryId, bool includePosts = false)
        => Task.FromResult(Categories.FirstOrDefault(category => category.Id == categoryId));

    public Task<PostCategory?> GetCategoryBySlugAsync(string slug, bool includePosts = false)
        => Task.FromResult(Categories.FirstOrDefault(category => category.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase)));

    public Task CreateCategoryAsync(PostCategory category)
    {
        _blogStore.StoreCategory(category);

        return Task.CompletedTask;
    }

    public Task UpdateCategoryAsync(PostCategory category)
    {
        var existingCategory = _blogStore.Categories.FirstOrDefault(c => c.Id == category.Id);

        var list = new List<PostCategory>();
        _blogStore.StoreCategory(category);

        return Task.CompletedTask;
    }

    public Task DeleteCategoryByIdAsync(string categoryId)
    {
        _blogStore.DeleteCategory(categoryId);

        return Task.CompletedTask;
    }

    public async Task<SoapboxUser?> GetAuthorByIdAsync(string authorId)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        return await _userStore.FindByIdAsync(authorId, cts.Token);
    }
}
