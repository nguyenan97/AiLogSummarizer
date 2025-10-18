namespace Domain.MentionParsing.Models;

/// <summary>
/// Marker interface for all parsed mention parameter models.
/// Every case carries a <see cref="CaseContext"/> to provide
/// consistent Datadog-friendly scoping (service, env, time, tags...).
/// </summary>
public interface ICaseParameters
{
    /// <summary>
    /// Common contextual fields extracted from the mention. These fields are
    /// shared across cases to make downstream Datadog queries consistent.
    /// </summary>
    CaseContext Context { get; }
}
