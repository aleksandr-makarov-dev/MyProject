namespace MyProject.WebApi.Domain;

public interface IAuditable
{
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}