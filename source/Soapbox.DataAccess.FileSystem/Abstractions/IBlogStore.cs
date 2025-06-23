namespace Soapbox.DataAccess.FileSystem.Abstractions;

using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

public interface IBlogStore
{
    public IQueryable<Post> Posts { get; }

    public IQueryable<PostCategory> Categories { get; }

    public Result StorePost(Post post);

    public Result DeletePost(string postId);

    public Result StoreCategory(PostCategory category);

    public Result DeleteCategory(string postId);

    public void Init();

    public void Refresh();
}