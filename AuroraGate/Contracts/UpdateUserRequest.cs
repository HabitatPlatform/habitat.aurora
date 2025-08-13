namespace AuroraGate.Contracts
{
    public record UpdateUserRequest(string? Email, bool? IsActive, string? NewPassword, string? Role);

}
