namespace Soapbox.DataAccess.Abstractions
{
    using Soapbox.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IPostRepository
    {
        public Task<IAsyncEnumerable<Post>> ListAsync(Expression<Func<Post, bool>> predicate);

        public Task<IAsyncEnumerable<Post>> ListAsync(int page = 0, int pageSize = 0);

        public Task<Post> CreateAsync(Post post);

        public Task<Post> GetByIdAsync(string id);

        public Task<Post> GetByFilterAsync(Expression<Func<Post, bool>> predicate);

        public Task<Post> UpdateAsync(Post post);

        public Task DeleteByIdAsync(string id);
    }
}
