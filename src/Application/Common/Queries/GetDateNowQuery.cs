using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Queries;

public record GetDateNowQuery : IRequest<DateTime>;

public class GetDateNowQueryHandler : IRequestHandler<GetDateNowQuery, DateTime>
{
    private readonly IDateTimeService _dateTimeService;
    public GetDateNowQueryHandler(IDateTimeService dateTimeService)
    {
        _dateTimeService = dateTimeService;
    }

    public Task<DateTime> Handle(GetDateNowQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_dateTimeService.UtcNow);
    }
}