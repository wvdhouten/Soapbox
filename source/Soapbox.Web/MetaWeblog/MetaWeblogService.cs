namespace Soapbox.Web.MetaWeblog;

using System.Threading.Tasks;
using WilderMinds.MetaWeblog;

public class MetaWeblogService : IMetaWeblogProvider
{
    public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
    {
        throw new NotImplementedException();
    }

    public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
    {
        throw new NotImplementedException();
    }

    public Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
    {
        throw new NotImplementedException();
    }

    public Task<Author[]> GetAuthorsAsync(string blogid, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages)
    {
        throw new NotImplementedException();
    }

    public Task<Post> GetPostAsync(string postid, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
    {
        throw new NotImplementedException();
    }

    public Task<Tag[]> GetTagsAsync(string blogid, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
    {
        throw new NotImplementedException();
    }
}
