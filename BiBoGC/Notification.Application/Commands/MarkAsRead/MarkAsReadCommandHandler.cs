using MediatR;
using Notification.Application.Interfaces;
using Shared.Application.Common;

namespace Notification.Application.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result<bool>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
            return Result<bool>.Failure($"Không tìm thấy thông báo với ID '{request.NotificationId}'.");

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
