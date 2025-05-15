namespace Soapbox.DataAccess.FileSystem.Abstractions;

using System.Collections.Generic;
using Soapbox.Domain.Blog;

public interface IBlogStore
{
    public IList<Post> Posts { get; }

    public IList<PostCategory> Categories { get; }

    public StorageResult UpdatePost(Post post);

    public StorageResult DeletePost(string postId);

    public void Restore();
}