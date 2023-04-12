namespace Soapbox.DataAccess.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;

    public class BlogService : IBlogService
    {
        public BlogService()
        {
        }

        public Task<IPagedList<Post>> GetPostsPageAsync(int page = 0, int pageSize = 25, bool isPublished = true)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByCategoryAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<Post>> GetPostsByAuthorAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task CreatePostAsync(Post post)
        {
            throw new NotImplementedException();
        }

        public async Task<Post> GetPostByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPostBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }

        public async Task UpdatePostAsync(Post post)
        {
            throw new NotImplementedException();
        }

        public async Task DeletePostByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false)
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false)
        {
            throw new NotImplementedException();
        }

        public async Task CreateCategoryAsync(PostCategory category)
        {
            throw new NotImplementedException();
        }

        public Task<PostCategory> GetCategoryByIdAsync(long id, bool includePosts = false)
        {
            throw new NotImplementedException();
        }

        public Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCategoryAsync(PostCategory category)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteCategoryByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<SoapboxUser> GetAuthorByIdAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
