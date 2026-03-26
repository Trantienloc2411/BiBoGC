using MediatR;
using Notification.Application.Interfaces;
using Shared.Application.Common;

namespace Notification.Application.Commands.CreateNotification;

public class CreateNotificationCommandHandler
    : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public CreateNotificationCommandHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = Domain.Entities.Notification.Create(
            request.Title,
            request.Message,
            request.Type,
            request.Role,
            request.ReferenceId,
            request.ReferenceType);

        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(notification.Id);
    }
}
