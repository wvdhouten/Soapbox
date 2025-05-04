namespace Soapbox.Domain.Users;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The roles that can be assigned to a user.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Can access all administration features.
    /// </summary>
    [Display(Name = "Administrator")]
    Administrator,

    /// <summary>
    /// Can publish and manage all posts.
    /// </summary>
    [Display(Name = "Editor")]
    Editor,

    /// <summary>
    /// Can publish and manage own posts.
    /// </summary>
    [Display(Name = "Author")]
    Author,

    /// <summary>
    /// Can manage own posts.
    /// </summary>
    [Display(Name = "Contributor")]
    Contributor,

    /// <summary>
    /// Can manage own profile only.
    /// </summary>
    [Display(Name = "Subscriber")]
    Subscriber
}
