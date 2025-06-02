namespace Soapbox.Domain.Users;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Blog;

public class SoapboxUser : IdentityUser
{
    [PersonalData]
    public string? DisplayName { get; set; }

    public string ShownName => !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : UserName ?? string.Empty;

    public UserRole Role { get; set; }

    [JsonIgnore] // The author is stored on the Post entity.
    public IList<Post> Posts { get; set; } = [];
}
