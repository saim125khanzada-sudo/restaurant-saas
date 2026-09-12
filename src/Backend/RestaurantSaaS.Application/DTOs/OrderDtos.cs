using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.DTOs;

public record OrderItemAddonRequest(Guid AddonId, string AddonName, decimal UnitPrice, int Quantity);
public record CreateOrderItemRequest(Guid ProductId, Guid? ProductVariantId, string ItemName, int Quantity, decimal UnitPrice, string? KitchenNotes, List<OrderItemAddonRequest>? Addons);

public record CreateOrderRequest(
    Guid? BranchId,
    Guid? TableId,
    Guid? WaiterId,
    OrderType OrderType,
    string? CustomerName,
    string? CustomerPhone,
    string? DeliveryAddress,
    decimal DiscountTotal,
    string? DiscountReason,
    Guid IdempotencyKey,
    List<CreateOrderItemRequest> Items
);

public record OrderItemDto(Guid Id, Guid ProductId, string ItemName, int Quantity, decimal UnitPrice, decimal TotalPrice, string? KitchenNotes, List<OrderItemAddonRequest> Addons);

public record OrderDto(
    Guid Id,
    Guid RestaurantId,
    Guid? BranchId,
    string OrderNumber,
    OrderType OrderType,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    Guid? TableId,
    string? TableNumber,
    Guid? WaiterId,
    string? WaiterName,
    string? CustomerName,
    string? CustomerPhone,
    decimal Subtotal,
    decimal TaxTotal,
    decimal DiscountTotal,
    decimal GrandTotal,
    DateTimeOffset CreatedAt,
    List<OrderItemDto> Items
);

public record ProcessPaymentRequest(Guid OrderId, decimal Amount, PaymentMethod Method, string? TransactionReference);
public record PaymentDto(Guid Id, Guid OrderId, decimal Amount, PaymentMethod Method, string? TransactionReference, DateTimeOffset ProcessedAt);

public record TransitionOrderStatusRequest(Guid OrderId, OrderStatus TargetStatus, string? Reason);

public record ReceiptPrintDto(string OrderNumber, string RestaurantName, string BranchName, string DateFormatted, string OrderType, string? TableNumber, List<string> Lines, decimal Subtotal, decimal Tax, decimal Total, string FooterMessage);
