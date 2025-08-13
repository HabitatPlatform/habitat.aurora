namespace AuroraGate.Data;

public class MongoSettings
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
    public string UsersCollection { get; set; } = "users";
    public string RolesCollection { get; set; } = "roles";
    public string RefreshTokensCollection { get; set; } = "refresh_tokens";
}
