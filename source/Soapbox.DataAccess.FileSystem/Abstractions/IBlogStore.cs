namespace Soapbox.DataAccess.FileSystem.Abstractions;

using Soapbox.Domain.Blog;

public interface IBlogStore
{
    public IQueryable<Post> Posts { get; }

    public IQueryable<PostCategory> Categories { get; }

    public StorageResult UpdatePost(Post post);

    public StorageResult DeletePost(string postId);

    public void Refresh();
}