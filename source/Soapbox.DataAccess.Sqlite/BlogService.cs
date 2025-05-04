namespace Soapbox.DataAccess.Sqlite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Soapbox.DataAccess.Abstractions;
using Soapbox.DataAccess.Data;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Users;
using Soapbox.Application.Extensions;
using Soapbox.Domain;

public class BlogService : IBlogRepository
{
    private readonly ApplicationDbContext _context;

    public BlogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IPagedList<Post>> GetPostsPageAsync(int page = 0, int pageSize = 25, bool isPublished = true)
    {
        IQueryable<Post> posts = _context.Posts;

        if (isPublished)
        {
            var now = DateTime.UtcNow;
            posts = posts.Where(p => p.Status == PostStatus.Published && p.PublishedOn < now);
        }

        posts = posts.Include(p => p.Author).Include(p => p.Categories);
        posts = posts.OrderByDescending(post => EF.Property<Post>(post, nameof(post.PublishedOn)));
        return Task.FromResult(posts.GetPaged(page, pageSize));
    }

    public Task<IEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate)
    {
        return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(predicate).AsEnumerable());
    }

    public Task<IEnumerable<Post>> GetPostsByCategoryAsync(long categoryId)
    {
        return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(p => p.Categories.Any(c => c.Id == categoryId)).AsEnumerable());
    }

    public Task<IEnumerable<Post>> GetPostsByAuthorAsync(string authorId)
    {
        return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(p => p.Author.Id == authorId).AsEnumerable());
    }

    public async Task<Post> GetPostByIdAsync(string postId)
    {
        return await _context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == postId);
    }

    public Task<Post> GetPostBySlugAsync(string slug)
    {
        return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefault(p => p.Slug == slug));
    }

    public async Task CreatePostAsync(Post post)
    {
        CleanPostValues(post);

        post.Id = Guid.NewGuid().ToString();
        await _context.Posts.AddAsync(post);
        _context.Users.Attach(post.Author);
        AttachCategories(post);

        await _context.SaveChangesAsync();
    }

    public async Task UpdatePostAsync(Post post)
    {
        CleanPostValues(post);

        var existing = await _context.Posts.Include(p => p.Categories).SingleAsync(p => p.Id == post.Id);
        _context.Entry(existing).CurrentValues.SetValues(post);

        foreach(var category in existing.Categories.ToList())
        {
            if (!post.Categories.Any(c => c.Id == category.Id))
            {
                existing.Categories.Remove(category);
            }
        }
        foreach (var category in post.Categories.ToList())
        {
            if (category.Id == default || !existing.Categories.Any(c => c.Id == category.Id))
            {
                existing.Categories.Add(category);
            }
        }

        AttachCategories(existing);

        await _context.SaveChangesAsync();
    }

    public async Task DeletePostByIdAsync(string postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();
    }

    public Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false)
    {
        IQueryable<PostCategory> query = _context.PostCategories;
        if (includePosts)
        {
            query = IncludePostsInQuery(query);
        }

        return Task.FromResult(query.OrderBy(c => c.Name).GetPaged(page, pageSize));
    }

    public Task<IEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
    {
        IQueryable<PostCategory> query = _context.PostCategories;
        if (includePosts)
        {
            query = IncludePostsInQuery(query);
        }

        return Task.FromResult(query.OrderBy(c => c.Name).AsEnumerable());
    }

    public Task<PostCategory> GetCategoryByIdAsync(long categoryId, bool includePosts = false)
    {
        IQueryable<PostCategory> query = _context.PostCategories;
        if (includePosts)
        {
            query = IncludePostsInQuery(query);
        }

        return Task.FromResult(query.FirstOrDefault(c => c.Id == categoryId));
    }

    public Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false)
    {
        IQueryable<PostCategory> query = _context.PostCategories;
        if (includePosts)
        {
            query = IncludePostsInQuery(query);
        }

        return Task.FromResult(query.FirstOrDefault(c => c.Slug == slug));
    }

    private static IQueryable<PostCategory> IncludePostsInQuery(IQueryable<PostCategory> query)
    {
        return query.Where(c => c.Posts.Any(p => p.Status == PostStatus.Published)).Include(c => c.Posts.Where(p => p.Status == PostStatus.Published));
    }

    public async Task CreateCategoryAsync(PostCategory category)
    {
        if (_context.PostCategories.Any(c => c.Name == category.Name))
        {
            throw new Exception($"Category with name '{category.Name}' already exists.");
        }

        await _context.PostCategories.AddAsync(category);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(PostCategory category)
    {
        var existing = _context.PostCategories.Find(category.Id);
        _context.Entry(existing).CurrentValues.SetValues(category);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryByIdAsync(long categoryId)
    {
        var category = await _context.PostCategories.FindAsync(categoryId);

        _context.PostCategories.Remove(category);

        await _context.SaveChangesAsync();
    }

    public Task<SoapboxUser> GetAuthorByIdAsync(string authorId)
    {
        IQueryable<SoapboxUser> query = _context.Users.Include(u => u.Posts);

        return Task.FromResult(query.FirstOrDefault(u => u.Id == authorId));
    }

    private static void CleanPostValues(Post post)
    {
        post.Title = post.Title.Trim();
        post.Slug = post.Slug.Trim();
        post.ModifiedOn = post.ModifiedOn;
        post.PublishedOn = post.PublishedOn;
        post.Content = post.Content?.Trim() ?? string.Empty;
        post.Excerpt = post.Excerpt?.Trim() ?? string.Empty;
    }

    private void AttachCategories(Post post)
    {
        foreach (var category in post.Categories)
        {
            if (category.Id == default)
            {
                _context.PostCategories.Add(category);
            }
            else if (_context.PostCategories.Local.Any(c => c.Id == category.Id))
            {
                _context.PostCategories.Attach(category);
            }
        }
    }
}
