using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Application.Taxation.Services;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Taxation.Commands;

public record TransmitFiscalInvoiceCommand(
    Guid OrderId
) : IRequest<FiscalInvoiceResultDto>;

public record FiscalInvoiceResultDto(
    Guid FiscalRecordId,
    Guid OrderId,
    string FbrInvoiceNumber,
    string PosRegistrationNumber,
    string QrCodeData,
    FiscalSyncStatus Status,
    decimal TotalSalesValue,
    decimal TotalTaxCharged,
    DateTime? SyncedAt
);

public class TransmitFiscalInvoiceCommandValidator : AbstractValidator<TransmitFiscalInvoiceCommand>
{
    public TransmitFiscalInvoiceCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public class TransmitFiscalInvoiceCommandHandler : IRequestHandler<TransmitFiscalInvoiceCommand, FiscalInvoiceResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFbrFiscalService _fiscalService;

    public TransmitFiscalInvoiceCommandHandler(IApplicationDbContext context, IFbrFiscalService fiscalService)
    {
        _context = context;
        _fiscalService = fiscalService;
    }

    public async Task<FiscalInvoiceResultDto> Handle(TransmitFiscalInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order {request.OrderId} not found.");

        // Check if already fiscalized
        var existing = await _context.FiscalInvoiceRecords
            .FirstOrDefaultAsync(f => f.OrderId == order.Id, cancellationToken);

        if (existing != null && existing.Status == FiscalSyncStatus.Success)
        {
            return new FiscalInvoiceResultDto(
                existing.Id,
                existing.OrderId,
                existing.FbrInvoiceNumber,
                existing.PosRegistrationNumber,
                existing.QrCodeData,
                existing.Status,
                existing.TotalSalesValue,
                existing.TotalTaxCharged,
                existing.SyncedAt
            );
        }

        var fiscalResult = await _fiscalService.FiscalizeInvoiceAsync(order, cancellationToken);

        var record = existing ?? new FiscalInvoiceRecord
        {
            RestaurantId = order.RestaurantId,
            BranchId = order.BranchId ?? Guid.Empty,
            OrderId = order.Id
        };

        record.PosRegistrationNumber = fiscalResult.IsSuccess ? "POS-" + (order.BranchId?.ToString().Substring(0, 6) ?? "CORP01").ToUpperInvariant() : "OFFLINE";
        record.FbrInvoiceNumber = fiscalResult.FbrInvoiceNumber;
        record.QrCodeData = fiscalResult.QrCodeData;
        record.TotalSalesValue = order.GrandTotal;
        record.TotalTaxCharged = order.TaxTotal;
        record.ResponsePayload = fiscalResult.ResponsePayload;
        record.Status = fiscalResult.IsSuccess ? FiscalSyncStatus.Success : FiscalSyncStatus.QueuedOffline;
        record.SyncedAt = fiscalResult.IsSuccess ? DateTime.UtcNow : null;
        if (!fiscalResult.IsSuccess) record.RetryCount++;

        if (existing == null)
        {
            _context.FiscalInvoiceRecords.Add(record);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new FiscalInvoiceResultDto(
            record.Id,
            record.OrderId,
            record.FbrInvoiceNumber,
            record.PosRegistrationNumber,
            record.QrCodeData,
            record.Status,
            record.TotalSalesValue,
            record.TotalTaxCharged,
            record.SyncedAt
        );
    }
}
