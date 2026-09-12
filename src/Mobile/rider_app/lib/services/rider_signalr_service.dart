import 'package:signalr_netcore/signalr_netcore.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

typedef NewDispatchCallback = void Function(Map<String, dynamic> dispatch);

class RiderSignalRService {
  HubConnection? _hubConnection;
  final FlutterSecureStorage _storage;
  final String _hubUrl;

  RiderSignalRService({
    FlutterSecureStorage? storage,
    String hubUrl = 'http://10.0.2.2:5000/hubs/delivery',
  })  : _storage = storage ?? const FlutterSecureStorage(),
        _hubUrl = hubUrl;

  bool get isConnected => _hubConnection?.state == HubConnectionState.Connected;

  Future<void> connect({
    required String branchId,
    required NewDispatchCallback onNewDispatch,
  }) async {
    final token = await _storage.read(key: 'jwt_token');

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          _hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => token ?? '',
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hubConnection?.on('NewDeliveryAssigned', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        onNewDispatch(data);
      }
    });

    try {
      await _hubConnection?.start();
      await _hubConnection?.invoke('JoinBranchTracking', args: [branchId]);
    } catch (_) {}
  }

  Future<void> streamLocation({
    required String branchId,
    required double latitude,
    required double longitude,
    double? heading,
    double? speedKmh,
  }) async {
    if (isConnected) {
      try {
        await _hubConnection?.invoke('StreamRiderLocation', args: [
          branchId,
          latitude,
          longitude,
          heading,
          speedKmh,
        ]);
      } catch (_) {}
    }
  }

  Future<void> disconnect() async {
    if (_hubConnection != null) {
      await _hubConnection?.stop();
      _hubConnection = null;
    }
  }
}
