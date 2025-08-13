using AuroraGate.Contracts;
using AuroraGate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuroraGate.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder Map(this IEndpointRouteBuilder app)
        {
            var grp = app.MapGroup("/api")
                         .WithTags("Auth");
                         //////.WithOpenApi(); // si usas Swashbuckle

            grp.MapPost("/users", async ([FromBody] CreateUserRequest req, IUserService users, CancellationToken ct) =>
            {
                var u = await users.CreateAsync(req.Username, req.Email, req.Password, req.Role, ct);
                return Results.Created($"/api/users/{u.Id}", new { u.Id, u.Username, u.Email, u.IsActive });
            }).RequireAuthorization("AdminOnly");

            grp.MapGet("/users", async (int skip, int take, IUserService users, CancellationToken ct) =>
            {
                var list = await users.ListAsync(skip, take == 0 ? 50 : take, ct);
                return Results.Ok(list.Select(u => new { u.Id, u.Username, u.Email, u.IsActive }));
            }).RequireAuthorization("AdminOnly");

            grp.MapGet("/users/{id:guid}", async (Guid id, IUserService users, CancellationToken ct) =>
            {
                var u = await users.GetAsync(id, ct);
                return u is null ? Results.NotFound() : Results.Ok(new { u.Id, u.Username, u.Email, u.IsActive });
            }).RequireAuthorization("AdminOnly");

            grp.MapPut("/users/{id:guid}", async (Guid id, [FromBody] UpdateUserRequest req, IUserService users, CancellationToken ct) =>
            {
                await users.UpdateAsync(id, req.Email, req.IsActive, req.NewPassword, req.Role, ct);
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");

            grp.MapDelete("/users/{id:guid}", async (Guid id, IUserService users, CancellationToken ct) =>
            {
                await users.DeleteAsync(id, ct);
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");

            grp.MapPost("/auth/login", async ([FromBody] LoginRequest req, IUserService users, ITokenService tokens, CancellationToken ct) =>
            {
                var user = await users.ValidateCredentialsAsync(req.Username, req.Password, ct);
                if (user is null) return Results.Unauthorized();
                var (access, refresh) = await tokens.IssueTokensAsync(user, ct);
                return Results.Ok(new TokenResponse(access, refresh));
            });

            grp.MapPost("/auth/refresh", async ([FromBody] string refreshToken, ITokenService tokens, CancellationToken ct) =>
            {
                var (access, refresh) = await tokens.RotateRefreshAsync(refreshToken, ct);
                return Results.Ok(new TokenResponse(access, refresh));
            });

            return app;
        }
    }
}
