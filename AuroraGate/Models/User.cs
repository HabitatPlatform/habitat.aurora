using AuroraGate.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuroraGate.Models;

public class User
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    [BsonElement("username")] public string Username { get; set; } = default!;
    [BsonElement("email")] public string Email { get; set; } = default!;
    [BsonElement("pwd")] public string PasswordHash { get; set; } = default!;
    [BsonElement("active")] public bool IsActive { get; set; } = true;
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [BsonElement("lastLoginAt")] public DateTime? LastLoginAt { get; set; }

    [BsonElement("roles")] public ICollection<UserRole> UserRoles { get; set; } = [];
}