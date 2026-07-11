using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Persisted refresh token. The raw token value is NEVER stored — only a SHA-256
/// hash (<see cref="TokenHash"/>). Tokens are single-use: refreshing rotates the
/// token, stamping <see cref="RevokedAt"/> and <see cref="ReplacedByTokenHash"/>
/// on the consumed row so a replayed (already-rotated) token can be detected.
/// </summary>
public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash (Base64) of the raw refresh token.</summary>
    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token that superseded this one during rotation.</summary>
    [MaxLength(64)]
    public string? ReplacedByTokenHash { get; set; }

    public User? User { get; set; }

    [NotMapped]
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}
