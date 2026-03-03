using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Sale.Domain.Domain;
using Sale.Domain.Enum;
using Shared.Application.Common;

namespace Sale.Application.Commands.GenerateInvoice;

public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISalesOrderRepository _orderRepository;
    private readonly IStoreInfoService _storeInfoService;
    private readonly ISaleUnitOfWork _unitOfWork;

    public GenerateInvoiceCommandHandler(
        ISalesOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IStoreInfoService storeInfoService,
        ISaleUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _storeInfoService = storeInfoService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InvoiceDto>> Handle(
        GenerateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if invoice already exists (idempotency)
        var existingInvoice = await _invoiceRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingInvoice is not null)
        {
            return Result<InvoiceDto>.Success(InvoiceMapper.MapToDto(existingInvoice));
        }

        // 2. Load order
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<InvoiceDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        // 3. Validate order status
        if (order.Status != OrderStatus.Completed)
            return Result<InvoiceDto>.Failure("Chỉ có thể xuất hóa đơn cho đơn hàng đã hoàn thành.");

        // 4. Generate invoice number
        var invoiceNumber = await _invoiceNumberGenerator.GenerateNextAsync(cancellationToken);

        // 5. Get store info
        var storeInfo = await _storeInfoService.GetCurrentAsync(cancellationToken);

        // 6. Create invoice
        var invoice = Invoice.CreateFromOrder(order, storeInfo, invoiceNumber);

        // 7. Atomically save invoice and link it to the order
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _invoiceRepository.AddAsync(invoice, cancellationToken);

            // 8. Update order with invoice reference
            order.SetInvoiceId(invoice.Id);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result<InvoiceDto>.Success(InvoiceMapper.MapToDto(invoice));
    }
}