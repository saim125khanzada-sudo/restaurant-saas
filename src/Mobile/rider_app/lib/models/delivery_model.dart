import 'package:equatable/equatable.dart';

enum DeliveryStatus {
  assigned,
  accepted,
  rejected,
  pickedUp,
  inTransit,
  delivered,
  failed,
  cancelled;

  static DeliveryStatus fromInt(int value) {
    switch (value) {
      case 1: return DeliveryStatus.assigned;
      case 2: return DeliveryStatus.accepted;
      case 3: return DeliveryStatus.rejected;
      case 4: return DeliveryStatus.pickedUp;
      case 5: return DeliveryStatus.inTransit;
      case 6: return DeliveryStatus.delivered;
      case 7: return DeliveryStatus.failed;
      default: return DeliveryStatus.cancelled;
    }
  }

  int toInt() {
    switch (this) {
      case DeliveryStatus.assigned: return 1;
      case DeliveryStatus.accepted: return 2;
      case DeliveryStatus.rejected: return 3;
      case DeliveryStatus.pickedUp: return 4;
      case DeliveryStatus.inTransit: return 5;
      case DeliveryStatus.delivered: return 6;
      case DeliveryStatus.failed: return 7;
      case DeliveryStatus.cancelled: return 8;
    }
  }

  String get displayName {
    switch (this) {
      case DeliveryStatus.assigned: return 'Assigned';
      case DeliveryStatus.accepted: return 'Accepted';
      case DeliveryStatus.rejected: return 'Rejected';
      case DeliveryStatus.pickedUp: return 'Picked Up';
      case DeliveryStatus.inTransit: return 'In Transit';
      case DeliveryStatus.delivered: return 'Delivered';
      case DeliveryStatus.failed: return 'Failed';
      case DeliveryStatus.cancelled: return 'Cancelled';
    }
  }
}

class DeliveryModel extends Equatable {
  final String id;
  final String orderId;
  final String orderNumber;
  final String riderId;
  final DeliveryStatus status;
  final String deliveryAddress;
  final double? destinationLatitude;
  final double? destinationLongitude;
  final String? customerPhone;
  final String? customerName;
  final double cashToCollect;
  final double cashCollected;
  final bool isCashCollected;

  const DeliveryModel({
    required this.id,
    required this.orderId,
    required this.orderNumber,
    required this.riderId,
    required this.status,
    required this.deliveryAddress,
    this.destinationLatitude,
    this.destinationLongitude,
    this.customerPhone,
    this.customerName,
    required this.cashToCollect,
    required this.cashCollected,
    required this.isCashCollected,
  });

  factory DeliveryModel.fromJson(Map<String, dynamic> json) {
    final statusVal = json['status'];
    DeliveryStatus status;
    if (statusVal is int) {
      status = DeliveryStatus.fromInt(statusVal);
    } else {
      status = DeliveryStatus.values.firstWhere(
        (e) => e.name.toLowerCase() == (statusVal as String).toLowerCase(),
        orElse: () => DeliveryStatus.assigned,
      );
    }

    return DeliveryModel(
      id: json['id'] as String,
      orderId: json['orderId'] as String,
      orderNumber: json['orderNumber'] as String? ?? 'N/A',
      riderId: json['riderId'] as String,
      status: status,
      deliveryAddress: json['deliveryAddress'] as String,
      destinationLatitude: (json['destinationLatitude'] as num?)?.toDouble(),
      destinationLongitude: (json['destinationLongitude'] as num?)?.toDouble(),
      customerPhone: json['customerPhone'] as String?,
      customerName: json['customerName'] as String?,
      cashToCollect: (json['cashToCollect'] as num).toDouble(),
      cashCollected: (json['cashCollected'] as num?)?.toDouble() ?? 0.0,
      isCashCollected: json['isCashCollected'] as bool? ?? false,
    );
  }

  DeliveryModel copyWith({
    DeliveryStatus? status,
    double? cashCollected,
    bool? isCashCollected,
  }) {
    return DeliveryModel(
      id: id,
      orderId: orderId,
      orderNumber: orderNumber,
      riderId: riderId,
      status: status ?? this.status,
      deliveryAddress: deliveryAddress,
      destinationLatitude: destinationLatitude,
      destinationLongitude: destinationLongitude,
      customerPhone: customerPhone,
      customerName: customerName,
      cashToCollect: cashToCollect,
      cashCollected: cashCollected ?? this.cashCollected,
      isCashCollected: isCashCollected ?? this.isCashCollected,
    );
  }

  @override
  List<Object?> get props => [
        id,
        orderId,
        orderNumber,
        riderId,
        status,
        deliveryAddress,
        destinationLatitude,
        destinationLongitude,
        cashToCollect,
        cashCollected,
        isCashCollected,
      ];
}
