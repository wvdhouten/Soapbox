namespace Soapbox.DataAccess.FileSystem
{
    using System.Collections.Generic;
    using Soapbox.Models;

    public interface IBlogStore
    {
        IList<Post> Posts { get; }

        IList<PostCategory> Categories { get; }

        BlogStore.StorageResult UpdatePost(Post post);

        BlogStore.StorageResult DeletePost(string postId);

        void Restore();
    }
}