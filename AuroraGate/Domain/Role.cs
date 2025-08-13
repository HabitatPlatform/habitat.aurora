namespace AuroraGate.Domain
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
