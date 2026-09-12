using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Taxation.Services;

public record TaxCalculationResult(
    decimal NetAmount,
    decimal TaxRatePercentage,
    decimal TaxAmount,
    decimal TotalAmount,
    string AppliedRuleName
);

public interface ITaxCalculationService
{
    Task<TaxCalculationResult> CalculateTaxAsync(
        Guid restaurantId,
        Guid? branchId,
        decimal subtotal,
        OrderType orderType,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default);
}

public record FbrFiscalResponse(
    bool IsSuccess,
    string FbrInvoiceNumber,
    string QrCodeData,
    string? ResponsePayload,
    string? ErrorMessage
);

public interface IFbrFiscalService
{
    Task<FbrFiscalResponse> FiscalizeInvoiceAsync(
        Order order,
        CancellationToken cancellationToken = default);
}
