namespace AuroraGate.Contracts;

public record CreateUserRequest(string Username, string Email, string Password, string? Role);
