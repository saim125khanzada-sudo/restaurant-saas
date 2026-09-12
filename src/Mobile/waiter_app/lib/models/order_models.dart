import 'package:equatable/equatable.dart';

class OrderCartAddon extends Equatable {
  final String addonId;
  final String name;
  final double unitPrice;

  const OrderCartAddon({
    required this.addonId,
    required this.name,
    required this.unitPrice,
  });

  Map<String, dynamic> toJson() => {
    'addonId': addonId,
    'unitPrice': unitPrice,
  };

  factory OrderCartAddon.fromJson(Map<String, dynamic> json) => OrderCartAddon(
    addonId: json['addonId'] as String,
    name: json['name'] as String? ?? '',
    unitPrice: (json['unitPrice'] as num).toDouble(),
  );

  @override
  List<Object?> get props => [addonId, name, unitPrice];
}

class OrderCartItem extends Equatable {
  final String productId;
  final String productName;
  final String? variantId;
  final String? variantName;
  final int quantity;
  final double unitPrice;
  final String? notes;
  final List<OrderCartAddon> addons;

  const OrderCartItem({
    required this.productId,
    required this.productName,
    this.variantId,
    this.variantName,
    required this.quantity,
    required this.unitPrice,
    this.notes,
    this.addons = const [],
  });

  double get totalPrice {
    final addonsTotal = addons.fold<double>(0.0, (sum, a) => sum + a.unitPrice);
    return (unitPrice + addonsTotal) * quantity;
  }

  OrderCartItem copyWith({
    int? quantity,
    String? notes,
  }) {
    return OrderCartItem(
      productId: productId,
      productName: productName,
      variantId: variantId,
      variantName: variantName,
      quantity: quantity ?? this.quantity,
      unitPrice: unitPrice,
      notes: notes ?? this.notes,
      addons: addons,
    );
  }

  Map<String, dynamic> toJson() => {
    'productId': productId,
    'productVariantId': variantId,
    'quantity': quantity,
    'notes': notes,
    'addons': addons.map((a) => a.toJson()).toList(),
  };

  factory OrderCartItem.fromJson(Map<String, dynamic> json) => OrderCartItem(
    productId: json['productId'] as String,
    productName: json['productName'] as String? ?? 'Item',
    variantId: json['productVariantId'] as String?,
    variantName: json['variantName'] as String?,
    quantity: json['quantity'] as int,
    unitPrice: (json['unitPrice'] as num?)?.toDouble() ?? 0.0,
    notes: json['notes'] as String?,
    addons: (json['addons'] as List<dynamic>?)
            ?.map((a) => OrderCartAddon.fromJson(a as Map<String, dynamic>))
            .toList() ??
        [],
  );

  @override
  List<Object?> get props => [productId, variantId, quantity, unitPrice, notes, addons];
}

class OrderDraft extends Equatable {
  final String clientDraftId; // local uuid
  final String branchId;
  final String tableId;
  final String tableNumber;
  final List<OrderCartItem> items;
  final String? specialInstructions;
  final DateTime createdAt;
  final bool isSynced;

  const OrderDraft({
    required this.clientDraftId,
    required this.branchId,
    required this.tableId,
    required this.tableNumber,
    required this.items,
    this.specialInstructions,
    required this.createdAt,
    this.isSynced = false,
  });

  double get subtotal => items.fold<double>(0.0, (sum, i) => sum + i.totalPrice);

  Map<String, dynamic> toApiRequest(String idempotencyKey) => {
    'branchId': branchId,
    'orderType': 'DineIn',
    'tableId': tableId,
    'idempotencyKey': idempotencyKey,
    'notes': specialInstructions,
    'items': items.map((i) => i.toJson()).toList(),
  };

  Map<String, dynamic> toLocalDbMap() => {
    'client_draft_id': clientDraftId,
    'branch_id': branchId,
    'table_id': tableId,
    'table_number': tableNumber,
    'special_instructions': specialInstructions,
    'created_at': createdAt.toIso8601String(),
    'is_synced': isSynced ? 1 : 0,
  };

  @override
  List<Object?> get props => [clientDraftId, branchId, tableId, items, specialInstructions, isSynced];
}
