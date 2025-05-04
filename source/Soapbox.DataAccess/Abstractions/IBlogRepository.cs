namespace Soapbox.DataAccess.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Users;

public interface IBlogRepository
{
    public Task<IPagedList<Post>> GetPostsPageAsync(int page = 0, int pageSize = 25, bool isPublished = true);

    public Task<IEnumerable<Post>> GetPostsAsync(Expression<Func<Post, bool>> predicate);

    public Task<IEnumerable<Post>> GetPostsByCategoryAsync(long categoryId);

    public Task<IEnumerable<Post>> GetPostsByAuthorAsync(string authorId);

    public Task<Post> GetPostByIdAsync(string postId);

    public Task<Post> GetPostBySlugAsync(string slug);

    public Task CreatePostAsync(Post post);

    public Task UpdatePostAsync(Post post);

    public Task DeletePostByIdAsync(string postId);

    public Task<IPagedList<PostCategory>> GetCategoriesPageAsync(int page, int pageSize, bool includePosts = false);

    public Task<IEnumerable<PostCategory>> GetAllCategoriesAsync(bool includePosts = false);

    public Task<PostCategory> GetCategoryByIdAsync(long categoryId, bool includePosts = false);

    public Task<PostCategory> GetCategoryBySlugAsync(string slug, bool includePosts = false);

    public Task CreateCategoryAsync(PostCategory category);

    public Task UpdateCategoryAsync(PostCategory category);

    public Task DeleteCategoryByIdAsync(long categoryId);

    public Task<SoapboxUser> GetAuthorByIdAsync(string authorId);
}
