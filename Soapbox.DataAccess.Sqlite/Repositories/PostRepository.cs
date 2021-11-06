namespace Soapbox.DataAccess.Sqlite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Soapbox.DataAccess.Data;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;
    using Microsoft.EntityFrameworkCore;

    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IAsyncEnumerable<PostCategory>> GetCategoriesAsync(bool includePosts = false)
        {
            IQueryable<PostCategory> query = _context.PostCategories;
            if (includePosts)
            {
                query = query.Include(c => c.Posts);
            }

            return Task.FromResult(query.OrderBy(c => c.Name).AsAsyncEnumerable());
        }

        public Task<PostCategory> GetCategoryBySlug(string slug, bool includePosts = false)
        {
            IQueryable<PostCategory> query = _context.PostCategories;
            if (includePosts)
            {
                query = query.Include(c => c.Posts);
            }

            return Task.FromResult(query.FirstOrDefault(c => c.Slug == slug));
        }

        public Task<IAsyncEnumerable<Post>> ListAsync(int page = 0, int pageSize = 0)
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

        public Task<IAsyncEnumerable<Post>> ListAsync(Expression<Func<Post, bool>> predicate)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).Where(predicate).AsAsyncEnumerable());
        }

        public async Task<Post> CreateAsync(Post post)
        {
            post.Id = Guid.NewGuid().ToString();
            await _context.Posts.AddAsync(post);

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

            return post;
        }

        public async Task<Post> GetByIdAsync(string id)
        {
            return await _context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<Post> GetByFilterAsync(Expression<Func<Post, bool>> predicate)
        {
            return Task.FromResult(_context.Posts.Include(p => p.Author).Include(p => p.Categories).FirstOrDefault(predicate));
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            var entity = _context.Posts.Find(post.Id);
            if (entity == null)
            {
                throw new Exception();
            }

            _context.Entry(entity).CurrentValues.SetValues(post);
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

            return post;
        }

        public async Task DeleteByIdAsync(string id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
        }

        public Task<SoapboxUser> GetUserByIdAsync(string id)
        {
            IQueryable<SoapboxUser> query = _context.Users.Include(u => u.Posts);

            return Task.FromResult(query.FirstOrDefault(u => u.Id == id));
        }
    }
}
