import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/catalog_models.dart';
import '../models/table_models.dart';
import '../models/order_models.dart';

class ApiService {
  final Dio _dio;
  final FlutterSecureStorage _storage;

  ApiService({
    Dio? dio,
    FlutterSecureStorage? storage,
    String baseUrl = 'http://10.0.2.2:5000/api/v1', // standard Android emulator host mapping
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

  // --- Auth ---
  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await _dio.post('/auth/login', data: {
      'email': email,
      'password': password,
    });
    final data = response.data as Map<String, dynamic>;
    if (data['accessToken'] != null) {
      await _storage.write(key: 'jwt_token', value: data['accessToken'] as String);
      if (data['restaurantId'] != null) {
        await _storage.write(key: 'restaurant_id', value: data['restaurantId'] as String);
      }
    }
    return data;
  }

  Future<void> logout() async {
    await _storage.deleteAll();
  }

  // --- Catalog ---
  Future<List<CategoryModel>> getCategories() async {
    final response = await _dio.get('/categories');
    final list = response.data as List<dynamic>;
    return list.map((item) => CategoryModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  Future<List<ProductModel>> getProducts({String? categoryId}) async {
    final response = await _dio.get(
      '/products',
      queryParameters: categoryId != null ? {'categoryId': categoryId} : null,
    );
    final list = response.data as List<dynamic>;
    return list.map((item) => ProductModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  // --- Tables ---
  Future<List<TableModel>> getTables(String branchId) async {
    final response = await _dio.get('/tables', queryParameters: {'branchId': branchId});
    final list = response.data as List<dynamic>;
    return list.map((item) => TableModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  // --- Orders ---
  Future<Map<String, dynamic>> createOrder({
    required OrderDraft draft,
    required String idempotencyKey,
  }) async {
    final response = await _dio.post(
      '/orders',
      data: draft.toApiRequest(idempotencyKey),
    );
    return response.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> getOrderById(String orderId) async {
    final response = await _dio.get('/orders/$orderId');
    return response.data as Map<String, dynamic>;
  }
}
