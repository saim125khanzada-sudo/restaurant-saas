import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:uuid/uuid.dart';
import '../../models/order_models.dart';
import '../../services/sync_engine.dart';

// --- Events ---
abstract class OrderCartEvent extends Equatable {
  const OrderCartEvent();
  @override
  List<Object?> get props => [];
}

class SetActiveTable extends OrderCartEvent {
  final String branchId;
  final String tableId;
  final String tableNumber;
  const SetActiveTable({required this.branchId, required this.tableId, required this.tableNumber});
  @override
  List<Object?> get props => [branchId, tableId, tableNumber];
}

class AddItemToCart extends OrderCartEvent {
  final OrderCartItem item;
  const AddItemToCart(this.item);
  @override
  List<Object?> get props => [item];
}

class RemoveItemFromCart extends OrderCartEvent {
  final int itemIndex;
  const RemoveItemFromCart(this.itemIndex);
  @override
  List<Object?> get props => [itemIndex];
}

class ClearCart extends OrderCartEvent {}

class FireKotToKitchen extends OrderCartEvent {
  final String? specialInstructions;
  const FireKotToKitchen({this.specialInstructions});
  @override
  List<Object?> get props => [specialInstructions];
}

// --- States ---
abstract class OrderCartState extends Equatable {
  const OrderCartState();
  @override
  List<Object?> get props => [];
}

class OrderCartEmpty extends OrderCartState {}

class OrderCartActive extends OrderCartState {
  final String branchId;
  final String tableId;
  final String tableNumber;
  final List<OrderCartItem> items;

  const OrderCartActive({
    required this.branchId,
    required this.tableId,
    required this.tableNumber,
    this.items = const [],
  });

  double get subtotal => items.fold<double>(0.0, (sum, i) => sum + i.totalPrice);

  OrderCartActive copyWith({
    String? branchId,
    String? tableId,
    String? tableNumber,
    List<OrderCartItem>? items,
  }) {
    return OrderCartActive(
      branchId: branchId ?? this.branchId,
      tableId: tableId ?? this.tableId,
      tableNumber: tableNumber ?? this.tableNumber,
      items: items ?? this.items,
    );
  }

  @override
  List<Object?> get props => [branchId, tableId, tableNumber, items];
}

class OrderSubmitting extends OrderCartState {}

class OrderSubmittedSuccess extends OrderCartState {
  final String message;
  final bool isQueuedOffline;
  const OrderSubmittedSuccess({required this.message, this.isQueuedOffline = false});
  @override
  List<Object?> get props => [message, isQueuedOffline];
}

class OrderSubmissionError extends OrderCartState {
  final String error;
  const OrderSubmissionError(this.error);
  @override
  List<Object?> get props => [error];
}

// --- Bloc ---
class OrderCartBloc extends Bloc<OrderCartEvent, OrderCartState> {
  final SyncEngine _syncEngine;

  OrderCartBloc({SyncEngine? syncEngine})
      : _syncEngine = syncEngine ?? SyncEngine(),
        super(OrderCartEmpty()) {
    on<SetActiveTable>(_onSetActiveTable);
    on<AddItemToCart>(_onAddItemToCart);
    on<RemoveItemFromCart>(_onRemoveItemFromCart);
    on<ClearCart>(_onClearCart);
    on<FireKotToKitchen>(_onFireKotToKitchen);
  }

  void _onSetActiveTable(SetActiveTable event, Emitter<OrderCartState> emit) {
    emit(OrderCartActive(
      branchId: event.branchId,
      tableId: event.tableId,
      tableNumber: event.tableNumber,
      items: const [],
    ));
  }

  void _onAddItemToCart(AddItemToCart event, Emitter<OrderCartState> emit) {
    if (state is OrderCartActive) {
      final current = state as OrderCartActive;
      final updatedList = List<OrderCartItem>.from(current.items)..add(event.item);
      emit(current.copyWith(items: updatedList));
    }
  }

  void _onRemoveItemFromCart(RemoveItemFromCart event, Emitter<OrderCartState> emit) {
    if (state is OrderCartActive) {
      final current = state as OrderCartActive;
      if (event.itemIndex >= 0 && event.itemIndex < current.items.length) {
        final updatedList = List<OrderCartItem>.from(current.items)..removeAt(event.itemIndex);
        emit(current.copyWith(items: updatedList));
      }
    }
  }

  void _onClearCart(ClearCart event, Emitter<OrderCartState> emit) {
    emit(OrderCartEmpty());
  }

  Future<void> _onFireKotToKitchen(FireKotToKitchen event, Emitter<OrderCartState> emit) async {
    if (state is! OrderCartActive) return;
    final current = state as OrderCartActive;
    if (current.items.isEmpty) return;

    emit(OrderSubmitting());

    final draft = OrderDraft(
      clientDraftId: const Uuid().v4(),
      branchId: current.branchId,
      tableId: current.tableId,
      tableNumber: current.tableNumber,
      items: current.items,
      specialInstructions: event.specialInstructions,
      createdAt: DateTime.now(),
    );

    try {
      await _syncEngine.submitOrder(draft: draft);
      emit(const OrderSubmittedSuccess(
        message: 'Order fired to Kitchen successfully!',
        isQueuedOffline: false,
      ));
    } catch (_) {
      // Offline fallback
      emit(const OrderSubmittedSuccess(
        message: 'Network unreachable: Order queued in local Outbox for automatic sync.',
        isQueuedOffline: true,
      ));
    }
  }
}
