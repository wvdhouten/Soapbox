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

    private readonly Dictionary<string, Post> _posts = [];
    private readonly Dictionary<string, PostCategory> _categories = [];

    public void Refresh()
    {
        _posts.Clear();
        _categories.Clear();

        var files = Directory.GetFiles(_contentPath, "*.md");
        foreach (var file in files)
            RestorePost(file);
    }

    public IQueryable<Post> Posts => _posts.Values.AsQueryable();

    public IQueryable<PostCategory> Categories => _categories.Values.AsQueryable();

    public StorageResult UpdatePost(Post post)
    {
        try
        {
            if (string.IsNullOrEmpty(post.Id))
                post.Id = Guid.NewGuid().ToString();

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

        _posts[post.Id] = post;
        foreach (var category in post.Categories)
            category.Posts.Add(post);

        return StorageResult.Fail;
    }

    public StorageResult DeletePost(string postId)
    {
        var post = Posts.FirstOrDefault(post => post.Id == postId);
        if (post == null)
            return StorageResult.Fail;

        try
        {
            var filePath = Path.Combine(_contentPath, $"{postId}.md");
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

        _posts.Remove(postId);

        return StorageResult.Success;
    }

    public StorageResult UpdateCategory(PostCategory category)
    {
        try
        {
            if (string.IsNullOrEmpty(category.Id))
                category.Id = Guid.NewGuid().ToString();

            var filePath = Path.Combine(_contentPath, $"categories.sbmeta");

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(_yamlSerializer.Serialize(_categories.Values));

            SaveFile(filePath, contentBuilder.ToString());
        }
        catch
        {
            return StorageResult.Fail;
        }

        _categories[category.Id] = category;

        return StorageResult.Fail;
    }

    public StorageResult DeleteCategory(string postId)
    {
        var post = Posts.FirstOrDefault(post => post.Id == postId);
        if (post == null)
            return StorageResult.Fail;

        try
        {
            var filePath = Path.Combine(_contentPath, $"{postId}.md");
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

        _posts.Remove(postId);

        return StorageResult.Success;
    }

    private static void SaveFile(string filePath, string fileContent) => File.WriteAllText(filePath, fileContent);

    private static void DeleteFile(string filePath) => File.Delete(filePath);

    private void RestorePost(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);

        var frontMatterRegex = FrontMatterRegex();
        var match = frontMatterRegex.Match(fileContent);

        var frontMatter = _yamlDeserializer.Deserialize<PostRecord>(match.Groups["frontMatter"].Value);
        Post post = frontMatter;
        post.Content = fileContent[match.Length..];

        _posts[post.Id] = post;
    }

    [GeneratedRegex("^---\\s*\\n(?<frontMatter>(.|\\n)*?)\\n---\\s*\\n", RegexOptions.Multiline)]
    private static partial Regex FrontMatterRegex();
}
