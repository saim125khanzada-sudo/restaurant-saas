import 'dart:async';
import 'dart:convert';
import 'package:uuid/uuid.dart';
import '../models/order_models.dart';
import 'api_service.dart';
import 'local_database_service.dart';

class SyncEngine {
  final ApiService _apiService;
  final LocalDatabaseService _dbService;
  Timer? _syncTimer;
  bool _isSyncing = false;

  SyncEngine({
    ApiService? apiService,
    LocalDatabaseService? dbService,
  })  : _apiService = apiService ?? ApiService(),
        _dbService = dbService ?? LocalDatabaseService.instance;

  void startPeriodicSync({Duration interval = const Duration(seconds: 15)}) {
    _syncTimer?.cancel();
    _syncTimer = Timer.periodic(interval, (_) => syncPendingOrders());
  }

  void stopSync() {
    _syncTimer?.cancel();
    _syncTimer = null;
  }

  Future<void> submitOrder({
    required OrderDraft draft,
  }) async {
    final idempotencyKey = const Uuid().v4();
    try {
      // 1. Attempt online submission first
      await _apiService.createOrder(draft: draft, idempotencyKey: idempotencyKey);
    } catch (e) {
      // 2. Wi-Fi dropped / offline fallback: queue in SQLite outbox
      await _dbService.queueOrderForSync(
        draft: draft,
        idempotencyKey: idempotencyKey,
      );
      rethrow; // Inform UI that order is queued locally
    }
  }

  Future<int> syncPendingOrders() async {
    if (_isSyncing) return 0;
    _isSyncing = true;
    int syncedCount = 0;

    try {
      final pendingList = await _dbService.getPendingSyncOrders();
      for (final raw in pendingList) {
        final clientDraftId = raw['client_draft_id'] as String;
        final idempotencyKey = raw['idempotency_key'] as String;
        final branchId = raw['branch_id'] as String;
        final tableId = raw['table_id'] as String;
        final tableNumber = raw['table_number'] as String;
        final itemsRaw = jsonDecode(raw['items_json'] as String) as List<dynamic>;
        final specialInstructions = raw['special_instructions'] as String?;
        final createdAt = DateTime.parse(raw['created_at'] as String);

        final items = itemsRaw
            .map((item) => OrderCartItem.fromJson(item as Map<String, dynamic>))
            .toList();

        final draft = OrderDraft(
          clientDraftId: clientDraftId,
          branchId: branchId,
          tableId: tableId,
          tableNumber: tableNumber,
          items: items,
          specialInstructions: specialInstructions,
          createdAt: createdAt,
        );

        try {
          // Idempotent retry to server
          await _apiService.createOrder(
            draft: draft,
            idempotencyKey: idempotencyKey,
          );
          await _dbService.markOrderAsSynced(clientDraftId);
          syncedCount++;
        } catch (err) {
          await _dbService.updateSyncError(clientDraftId, err.toString());
        }
      }
    } finally {
      _isSyncing = false;
    }

    return syncedCount;
  }
}
