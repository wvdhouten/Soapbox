namespace Soapbox.DataAccess.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Soapbox.DataAccess.FileSystem.Models;
    using Soapbox.Models;
    using YamlDotNet.Serialization;

    public partial class BlogStore : IBlogStore
    {
        public enum StorageResult
        {
            Success,
            Fail,
        }

        private static readonly ISerializer YamlSerializer = new SerializerBuilder().Build();
        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        private readonly IMapper _mapper;
        private readonly string _contentPath = Path.Combine(Environment.CurrentDirectory, "Content");
        private IList<Post> _posts;
        private IList<PostCategory> _categories;

        public IList<Post> Posts
        {
            get
            {
                if (_posts == null)
                {
                    Restore();
                }

                return _posts;
            }
        }

        public IList<PostCategory> Categories
        {
            get
            {
                if (_categories == null)
                {
                    Restore();
                }

                return _categories;
            }
        }

        public BlogStore(IMapper mapper)
        {
            _mapper = mapper;
        }

        public StorageResult UpdatePost(Post post)
        {
            try
            {
                if (string.IsNullOrEmpty(post.Id))
                {
                    post.Id = Guid.NewGuid().ToString();
                }

                var filePath = Path.Combine(_contentPath, $"{post.Id}.md");

                var frontmatter = _mapper.Map<PostRecord>(post);

                var contentBuilder = new StringBuilder();
                contentBuilder.AppendLine("---");
                contentBuilder.Append(YamlSerializer.Serialize(frontmatter));
                contentBuilder.AppendLine("---");
                contentBuilder.Append(post.Content);

                SaveFile(filePath, contentBuilder.ToString());
            }
            catch
            {
                return StorageResult.Fail;
            }

            Posts.Add(post);
            foreach (var category in post.Categories)
            {
                category.Posts.Add(post);
            }
            post.Author.Posts.Add(post);

            return StorageResult.Fail;
        }

        public StorageResult DeletePost(string postId)
        {
            var post = Posts.FirstOrDefault(post => post.Id == postId);
            if (post == null)
            {
                return StorageResult.Fail;
            }

            try
            {
                var filePath = Path.Combine(_contentPath, $"{post.Id}.md");
                DeleteFile(filePath);
            }
            catch
            {
                return StorageResult.Fail;
            }

            foreach (var category in post.Categories)
            {
                category.Posts.Remove(post);
            }
            post.Author.Posts.Remove(post);

            Posts.Remove(post);

            return StorageResult.Success;
        }

        private Post RestorePost(string filePath)
        {
             var fileContent = File.ReadAllText(filePath);

            var frontMatterRegex = FrontMatterRegex();
            var match = frontMatterRegex.Match(fileContent);

            var frontMatter = YamlDeserializer.Deserialize<PostRecord>(match.Groups["frontMatter"].Value);
            var post = _mapper.Map<Post>(frontMatter);
            post.Content = fileContent[match.Length..];

            return post;
        }

        private void SaveFile(string filePath, string fileContent)
        {
            File.WriteAllText(filePath, fileContent);
        }

        private void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public void Restore()
        {
            _posts = new List<Post>();
            _categories = new List<PostCategory>();

            var files = Directory.GetFiles(_contentPath, "*.md");
            foreach (var file in files)
            {
                var post = RestorePost(file);

                _posts.Add(post);
            }
        }

        [GeneratedRegex("^---\\s*\\n(?<frontMatter>(.|\\n)*?)\\n---\\s*\\n", RegexOptions.Multiline)]
        private static partial Regex FrontMatterRegex();
    }
}
