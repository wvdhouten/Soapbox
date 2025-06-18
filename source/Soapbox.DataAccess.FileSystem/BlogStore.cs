namespace Soapbox.DataAccess.FileSystem;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alkaline64.Injectable;
using Microsoft.Extensions.FileSystemGlobbing;
using Soapbox.Application.Constants;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.FileSystem.Abstractions;
using Soapbox.DataAccess.FileSystem.Blog;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;
using YamlDotNet.Serialization;

[Injectable<IBlogStore>(Lifetime.Singleton)]
public partial class BlogStore : IBlogStore
{
    private const string _postFileExtension = "sbpost";
    private const string _metaFileExtension = "sbmeta";

    private static readonly ISerializer _yamlSerializer = new SerializerBuilder().Build();
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    private readonly string _contentPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Content, FolderNames.Posts);

    private readonly Dictionary<string, Post> _posts = [];
    private readonly Dictionary<string, PostCategory> _categories = [];

    public void Refresh()
    {
        _posts.Clear();
        _categories.Clear();

        var categoriesMetaFilePath = Path.Combine(_contentPath, $"categories.{_metaFileExtension}");
        if (File.Exists(categoriesMetaFilePath))
        {
            var categoriesMeta = File.ReadAllText(categoriesMetaFilePath);
            var categories = _yamlDeserializer.Deserialize<IEnumerable<PostCategory>>(categoriesMeta);
            foreach (var category in categories)
                _categories.Add(category.Id, category);
        }

        var files = Directory.GetFiles(_contentPath, $"*.{_postFileExtension}");
        foreach (var file in files)
            LoadPost(file);
    }

    public IQueryable<Post> Posts => _posts.Values.AsQueryable();

    public IQueryable<PostCategory> Categories => _categories.Values.AsQueryable();

    public Result StorePost(Post post)
    {
        try
        {
            if (string.IsNullOrEmpty(post.Id))
                post.Id = Guid.NewGuid().ToString();

            var filePath = Path.Combine(_contentPath, $"{post.Id}.{_postFileExtension}");

            PostRecord record = post;

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("---");
            contentBuilder.Append(_yamlSerializer.Serialize(record));
            contentBuilder.AppendLine("---");
            contentBuilder.Append(post.Content);

            SaveFile(filePath, contentBuilder.ToString());
        }
        catch
        {
            return Error.Unknown();
        }

        _posts[post.Id] = post;
        foreach (var category in post.Categories)
            category.Posts.Add(post);

        return Result.Success();
    }

    public Result DeletePost(string postId)
    {
        var post = Posts.FirstOrDefault(post => post.Id == postId);
        if (post == null)
            return Error.NotFound("Post not found.");

        try
        {
            var filePath = Path.Combine(_contentPath, $"{postId}.md");
            DeleteFile(filePath);
        }
        catch
        {
            return Error.Unknown();
        }

        foreach (var category in post.Categories)
        {
            category.Posts.Remove(post);
        }

        _posts.Remove(postId);

        return Result.Success();
    }

    public Result StoreCategory(PostCategory category)
    {
        try
        {
            if (string.IsNullOrEmpty(category.Id))
                category.Id = Guid.NewGuid().ToString();

            var filePath = Path.Combine(_contentPath, $"categories.{_metaFileExtension}");

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(_yamlSerializer.Serialize(_categories.Values));

            SaveFile(filePath, contentBuilder.ToString());
        }
        catch
        {
            return Error.Unknown();
        }

        _categories[category.Id] = category;

        return Result.Success();
    }

    public Result DeleteCategory(string postId)
    {
        var post = Posts.FirstOrDefault(post => post.Id == postId);
        if (post == null)
            return Error.NotFound("Category not found.");

        try
        {
            var filePath = Path.Combine(_contentPath, $"{postId}.md");
            DeleteFile(filePath);
        }
        catch
        {
            return Error.Unknown();
        }

        foreach (var category in post.Categories)
        {
            category.Posts.Remove(post);
        }

        _posts.Remove(postId);

        return Result.Success();
    }

    private void LoadPost(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);

        var match = FrontMatterRegex().Match(fileContent);
        var frontMatter = _yamlDeserializer.Deserialize<PostRecord>(match.Groups["frontMatter"].Value);

        Post post = frontMatter;
        post.Content = fileContent[match.Length..];
        _posts[post.Id] = post;

        foreach(var reference in post.Categories) { 
            var category = LoadCategory(reference);
            post.Categories.Remove(reference);
            post.Categories.Add(category);
        }
    }

    private PostCategory LoadCategory(PostCategory reference)
    {
        return Categories.FirstOrDefault(c => c.Id == reference.Id) 
            ?? new()
            {
                Id = Ulid.NewUlid().ToString(),
                Name = reference.Name,
                Slug = Slugifier.Slugify(reference.Slug),
            };
    }

    private static void SaveFile(string filePath, string fileContent) => File.WriteAllText(filePath, fileContent);

    private static void DeleteFile(string filePath) => File.Delete(filePath);

    [GeneratedRegex("^---\\s*\\n(?<frontMatter>(.|\\n)*?)\\n---\\s*\\n", RegexOptions.Multiline)]
    private static partial Regex FrontMatterRegex();
}
