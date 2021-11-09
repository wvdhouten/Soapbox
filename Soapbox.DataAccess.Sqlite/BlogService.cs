namespace Soapbox.DataAccess.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Soapbox.Core.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.DataAccess.Data;
    using Soapbox.Models;

    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IAsyncEnumerable<Post>> GetAllPostsAsync()
        {
            return GetPostsAsync();
        }

        public Task<IAsyncEnumerable<Post>> GetPostsAsync(int pageSize = 0, int page = 0)
        {
            IQueryable<Post> posts = _context.Posts.Include(p => p.Author).Include(p => p.Categories);
            var now = DateTime.UtcNow;
            // posts = posts.Where(post => post.Status == PostStatus.Published && post.PublishedOn <= now);
            posts = posts.OrderByDescending(post => EF.Property<Post>(post, "PublishedOn"));

            if (page >= 0 && pageSize > 0)
            {
                posts = posts.Skip(page * pageSize).Take(pageSize);
            }

            return Task.FromResult(posts.AsAsyncEnumerable());
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByAuthorAsync(string id)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(p => p.Author.Id == id).AsAsyncEnumerable());
        }

        public Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(predicate).AsAsyncEnumerable());
        }

        public async Task CreateOrUpdatePostAsync(Post post)
        {
            var existing = await _context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == post.Id) ?? post;
            existing.Title = post.Title.Trim();
            existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : CreateSlug(post.Title);
            existing.ModifiedOn = DateTime.UtcNow;
            existing.PublishedOn = post.PublishedOn;
            existing.Content = (post.Content ?? "").Trim();
            existing.Excerpt = (post.Excerpt ?? "").Trim();

            if (string.IsNullOrEmpty(existing.Id))
            {
                post.Id = Guid.NewGuid().ToString();
                await _context.Posts.AddAsync(post);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(existing);
            }

            _context.Users.Attach(post.Author);
            foreach (var category in post.Categories)
            {
                if (category.Id == default)
                {
                    _context.PostCategories.Add(category);
                }
                else
                {
                    _context.PostCategories.Attach(category);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Post> GetPostByIdAsync(string id)
        {
            return await _context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<Post> GetPostBySlugAsync(string slug)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefault(p => p.Slug == slug.ToLowerInvariant()));
        }

        public async Task DeletePostByIdAsync(string id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
        }

        private static string CreateSlug(string title)
        {
            title = title?.ToLowerInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            title = title.RemoveDiacritics();
            title = title.RemoveReservedUrlCharacters();

            return title.ToLowerInvariant();
        }

        public Task<IAsyncEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
        {
            IQueryable<PostCategory> query = _context.PostCategories;
            if (includePosts)
            {
                query = query.Include(c => c.Posts);
            }

            return Task.FromResult(query.OrderBy(c => c.Name).AsAsyncEnumerable());
        }

        public Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false)
        {
            IQueryable<PostCategory> query = _context.PostCategories;
            if (includePosts)
            {
                query = query.Include(c => c.Posts);
            }

            return Task.FromResult(query.FirstOrDefault(c => c.Slug == slug));
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByCategoryAsync(long id)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(p => p.Categories.Any(c => c.Id == id)).AsAsyncEnumerable());
        }

        public Task<SoapboxUser> GetAuthorByIdAsync(string id)
        {
            IQueryable<SoapboxUser> query = _context.Users.Include(u => u.Posts);

            return Task.FromResult(query.FirstOrDefault(u => u.Id == id));
        }
    }
}
