using MediatR;
using Notification.Application.Interfaces;
using Shared.Application.Common;

namespace Notification.Application.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result<bool>>
{
    private readonly INotificationRepository _repository;

    public MarkAllAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.MarkAllAsReadAsync(request.Role, cancellationToken);
        return Result<bool>.Success(true);
    }
}
