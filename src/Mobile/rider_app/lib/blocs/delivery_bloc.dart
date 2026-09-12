import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../models/delivery_model.dart';
import '../../services/rider_api_service.dart';

// --- Events ---
abstract class DeliveryEvent extends Equatable {
  const DeliveryEvent();
  @override
  List<Object?> get props => [];
}

class LoadRiderDeliveries extends DeliveryEvent {
  final String riderId;
  const LoadRiderDeliveries(this.riderId);
  @override
  List<Object?> get props => [riderId];
}

class AcceptDeliveryEvent extends DeliveryEvent {
  final String dispatchId;
  const AcceptDeliveryEvent(this.dispatchId);
  @override
  List<Object?> get props => [dispatchId];
}

class PickupDeliveryEvent extends DeliveryEvent {
  final String dispatchId;
  const PickupDeliveryEvent(this.dispatchId);
  @override
  List<Object?> get props => [dispatchId];
}

class CompleteDeliveryEvent extends DeliveryEvent {
  final String dispatchId;
  final double cashCollected;
  const CompleteDeliveryEvent({required this.dispatchId, required this.cashCollected});
  @override
  List<Object?> get props => [dispatchId, cashCollected];
}

// --- States ---
abstract class DeliveryState extends Equatable {
  const DeliveryState();
  @override
  List<Object?> get props => [];
}

class DeliveryInitial extends DeliveryState {}
class DeliveryLoading extends DeliveryState {}
class DeliveryLoaded extends DeliveryState {
  final List<DeliveryModel> activeDeliveries;
  const DeliveryLoaded(this.activeDeliveries);
  @override
  List<Object?> get props => [activeDeliveries];
}
class DeliveryError extends DeliveryState {
  final String message;
  const DeliveryError(this.message);
  @override
  List<Object?> get props => [message];
}

// --- Bloc ---
class DeliveryBloc extends Bloc<DeliveryEvent, DeliveryState> {
  final RiderApiService _apiService;
  String? _currentRiderId;

  DeliveryBloc({RiderApiService? apiService})
      : _apiService = apiService ?? RiderApiService(),
        super(DeliveryInitial()) {
    on<LoadRiderDeliveries>(_onLoadRiderDeliveries);
    on<AcceptDeliveryEvent>(_onAcceptDelivery);
    on<PickupDeliveryEvent>(_onPickupDelivery);
    on<CompleteDeliveryEvent>(_onCompleteDelivery);
  }

  Future<void> _onLoadRiderDeliveries(LoadRiderDeliveries event, Emitter<DeliveryState> emit) async {
    _currentRiderId = event.riderId;
    emit(DeliveryLoading());
    try {
      final deliveries = await _apiService.getActiveDeliveries(event.riderId);
      emit(DeliveryLoaded(deliveries));
    } catch (e) {
      emit(DeliveryError(e.toString()));
    }
  }

  Future<void> _onAcceptDelivery(AcceptDeliveryEvent event, Emitter<DeliveryState> emit) async {
    try {
      await _apiService.updateDeliveryStatus(
        dispatchId: event.dispatchId,
        newStatus: DeliveryStatus.accepted,
      );
      if (_currentRiderId != null) {
        add(LoadRiderDeliveries(_currentRiderId!));
      }
    } catch (e) {
      emit(DeliveryError(e.toString()));
    }
  }

  Future<void> _onPickupDelivery(PickupDeliveryEvent event, Emitter<DeliveryState> emit) async {
    try {
      await _apiService.updateDeliveryStatus(
        dispatchId: event.dispatchId,
        newStatus: DeliveryStatus.pickedUp,
      );
      if (_currentRiderId != null) {
        add(LoadRiderDeliveries(_currentRiderId!));
      }
    } catch (e) {
      emit(DeliveryError(e.toString()));
    }
  }

  Future<void> _onCompleteDelivery(CompleteDeliveryEvent event, Emitter<DeliveryState> emit) async {
    try {
      await _apiService.updateDeliveryStatus(
        dispatchId: event.dispatchId,
        newStatus: DeliveryStatus.delivered,
        cashCollected: event.cashCollected,
      );
      if (_currentRiderId != null) {
        add(LoadRiderDeliveries(_currentRiderId!));
      }
    } catch (e) {
      emit(DeliveryError(e.toString()));
    }
  }
}
