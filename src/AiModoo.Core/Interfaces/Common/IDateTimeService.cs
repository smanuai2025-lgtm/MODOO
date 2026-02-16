namespace AiModoo.Core.Interfaces.Common;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
