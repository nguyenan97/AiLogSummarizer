namespace Domain.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public string? EntityName { get; }
    public object? Key { get; }

    public EntityNotFoundException() { }

    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}

