import 'package:signalr_netcore/signalr_netcore.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

typedef OrderStatusCallback = void Function(String orderId, String status);
typedef TableStatusCallback = void Function(String tableId, String status);

class SignalRService {
  HubConnection? _hubConnection;
  final FlutterSecureStorage _storage;
  final String _hubUrl;

  SignalRService({
    FlutterSecureStorage? storage,
    String hubUrl = 'http://10.0.2.2:5000/hubs/orders',
  })  : _storage = storage ?? const FlutterSecureStorage(),
        _hubUrl = hubUrl;

  bool get isConnected => _hubConnection?.state == HubConnectionState.Connected;

  Future<void> connect({
    required OrderStatusCallback onOrderStatusChanged,
    TableStatusCallback? onTableStatusChanged,
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

    _hubConnection?.on('OrderStatusUpdated', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        final orderId = data['orderId'] as String? ?? '';
        final status = data['status'] as String? ?? '';
        onOrderStatusChanged(orderId, status);
      }
    });

    _hubConnection?.on('TableStatusUpdated', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        final tableId = data['tableId'] as String? ?? '';
        final status = data['status'] as String? ?? '';
        if (onTableStatusChanged != null) {
          onTableStatusChanged(tableId, status);
        }
      }
    });

    try {
      await _hubConnection?.start();
    } catch (e) {
      // Offline fallback: will auto-reconnect when connection recovers
    }
  }

  Future<void> disconnect() async {
    if (_hubConnection != null) {
      await _hubConnection?.stop();
      _hubConnection = null;
    }
  }
}
