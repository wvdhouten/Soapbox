namespace Soapbox.DataAccess.FileSystem;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alkaline64.Injectable;
using Soapbox.Application.Constants;
using Soapbox.DataAccess.FileSystem.Abstractions;
using Soapbox.DataAccess.FileSystem.Blog;
using Soapbox.Domain.Blog;
using YamlDotNet.Serialization;

[Injectable<IBlogStore>(Lifetime.Singleton)]
public partial class BlogStore : IBlogStore
{
    private static readonly ISerializer _yamlSerializer = new SerializerBuilder().Build();
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    private readonly string _contentPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Content, FolderNames.Posts);

    private IList<Post> _posts = [];
    private IList<PostCategory> _categories = [];

    public IList<Post> Posts
    {
        get
        {
            if (_posts.Count < 1)
                Restore();

            return _posts;
        }
    }

    public IList<PostCategory> Categories
    {
        get
        {
            if (_categories.Count < 1)
                Restore();

            return _categories;
        }
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

            PostRecord frontmatter = post;

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("---");
            contentBuilder.Append(_yamlSerializer.Serialize(frontmatter));
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

        Posts.Remove(post);

        return StorageResult.Success;
    }

    private Post RestorePost(string filePath)
    {
         var fileContent = File.ReadAllText(filePath);

        var frontMatterRegex = FrontMatterRegex();
        var match = frontMatterRegex.Match(fileContent);

        var frontMatter = _yamlDeserializer.Deserialize<PostRecord>(match.Groups["frontMatter"].Value);
        Post post = frontMatter;
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
        _posts = [];
        _categories = [];

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
