namespace MyProject.WebApi.Domain;

public interface ISoftDeletable
{
    public bool IsDeleted { get; init; }
    public DateTime? DeletedAtUtc { get; init; }
}