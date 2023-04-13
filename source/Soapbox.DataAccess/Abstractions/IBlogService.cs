namespace Soapbox.DataAccess.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Soapbox.Models;

    public interface IBlogService
    {
        Task<IPagedList<Post>> GetPostsPageAsync(int page = 0, int pageSize = 25, bool isPublished = true);

        Task<IAsyncEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate);

        Task<IAsyncEnumerable<Post>> GetPostsByCategoryAsync(long categoryId);

        Task<IAsyncEnumerable<Post>> GetPostsByAuthorAsync(string authorId);

        Task<Post> GetPostByIdAsync(string postId);

        Task<Post> GetPostBySlugAsync(string slug);

        Task CreatePostAsync(Post post);

        Task UpdatePostAsync(Post post);

        Task DeletePostByIdAsync(string postId);

        Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false);

        Task<IAsyncEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false);

        Task<PostCategory> GetCategoryByIdAsync(long categoryId, bool includePosts = false);

        Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false);

        Task CreateCategoryAsync(PostCategory category);

        Task UpdateCategoryAsync(PostCategory category);

        Task DeleteCategoryByIdAsync(long categoryId);

        Task<SoapboxUser> GetAuthorByIdAsync(string authorId);
    }
}
