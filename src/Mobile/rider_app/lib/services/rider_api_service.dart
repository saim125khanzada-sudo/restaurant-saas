import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/delivery_model.dart';

class RiderApiService {
  final Dio _dio;
  final FlutterSecureStorage _storage;

  RiderApiService({
    Dio? dio,
    FlutterSecureStorage? storage,
    String baseUrl = 'http://10.0.2.2:5000/api/v1',
  })  : _dio = dio ?? Dio(BaseOptions(baseUrl: baseUrl, connectTimeout: const Duration(seconds: 5))),
        _storage = storage ?? const FlutterSecureStorage() {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storage.read(key: 'jwt_token');
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          final restaurantId = await _storage.read(key: 'restaurant_id');
          if (restaurantId != null) {
            options.headers['X-Restaurant-Id'] = restaurantId;
          }
          return handler.next(options);
        },
      ),
    );
  }

  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await _dio.post('/auth/login', data: {
      'email': email,
      'password': password,
    });
    final data = response.data as Map<String, dynamic>;
    if (data['accessToken'] != null) {
      await _storage.write(key: 'jwt_token', value: data['accessToken'] as String);
      if (data['userId'] != null) {
        await _storage.write(key: 'rider_id', value: data['userId'] as String);
      }
      if (data['restaurantId'] != null) {
        await _storage.write(key: 'restaurant_id', value: data['restaurantId'] as String);
      }
    }
    return data;
  }

  Future<void> logout() async {
    await _storage.deleteAll();
  }

  Future<List<DeliveryModel>> getActiveDeliveries(String riderId) async {
    final response = await _dio.get('/deliveries/rider/$riderId/active');
    final list = response.data as List<dynamic>;
    return list.map((item) => DeliveryModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  Future<void> updateDeliveryStatus({
    required String dispatchId,
    required DeliveryStatus newStatus,
    double? cashCollected,
    String? failureReason,
  }) async {
    await _dio.patch(
      '/deliveries/$dispatchId/status',
      data: {
        'dispatchId': dispatchId,
        'newStatus': newStatus.toInt(),
        'cashCollected': cashCollected,
        'failureReason': failureReason,
      },
    );
  }

  Future<Map<String, dynamic>> reconcileCash({
    required String riderId,
    required String branchId,
    required DateTime shiftDate,
    required double totalCashSubmitted,
    String? notes,
  }) async {
    final response = await _dio.post(
      '/deliveries/reconcile',
      data: {
        'riderId': riderId,
        'branchId': branchId,
        'shiftDate': shiftDate.toIso8601String(),
        'totalCashSubmitted': totalCashSubmitted,
        'notes': notes,
      },
    );
    return response.data as Map<String, dynamic>;
  }
}
