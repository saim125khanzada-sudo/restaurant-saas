import 'dart:convert';
import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';
import '../models/catalog_models.dart';
import '../models/table_models.dart';
import '../models/order_models.dart';

class LocalDatabaseService {
  static final LocalDatabaseService instance = LocalDatabaseService._init();
  static Database? _database;

  LocalDatabaseService._init();

  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDB('waiter_offline.db');
    return _database!;
  }

  Future<Database> _initDB(String filePath) async {
    final dbPath = await getDatabasesPath();
    final path = join(dbPath, filePath);

    return await openDatabase(
      path,
      version: 1,
      onCreate: _createDB,
    );
  }

  Future<void> _createDB(Database db, int version) async {
    // 1. Categories Cache
    await db.execute('''
      CREATE TABLE cached_categories (
        id TEXT PRIMARY KEY,
        name TEXT NOT NULL,
        description TEXT,
        display_order INTEGER NOT NULL
      )
    ''');

    // 2. Products Cache
    await db.execute('''
      CREATE TABLE cached_products (
        id TEXT PRIMARY KEY,
        category_id TEXT NOT NULL,
        name TEXT NOT NULL,
        description TEXT,
        base_price REAL NOT NULL,
        image_url TEXT,
        is_available INTEGER NOT NULL,
        variants_json TEXT,
        addons_json TEXT
      )
    ''');

    // 3. Tables Cache
    await db.execute('''
      CREATE TABLE cached_tables (
        id TEXT PRIMARY KEY,
        branch_id TEXT NOT NULL,
        section_id TEXT NOT NULL,
        table_number TEXT NOT NULL,
        capacity INTEGER NOT NULL,
        status TEXT NOT NULL
      )
    ''');

    // 4. Outbox: Pending Sync Orders
    await db.execute('''
      CREATE TABLE outbox_orders (
        client_draft_id TEXT PRIMARY KEY,
        idempotency_key TEXT NOT NULL,
        branch_id TEXT NOT NULL,
        table_id TEXT NOT NULL,
        table_number TEXT NOT NULL,
        items_json TEXT NOT NULL,
        special_instructions TEXT,
        created_at TEXT NOT NULL,
        retry_count INTEGER DEFAULT 0,
        last_error TEXT,
        is_synced INTEGER DEFAULT 0
      )
    ''');
  }

  // --- Caching Operations ---
  Future<void> cacheCatalog(List<CategoryModel> categories, List<ProductModel> products) async {
    final db = await instance.database;
    await db.transaction((txn) async {
      await txn.delete('cached_categories');
      for (final cat in categories) {
        await txn.insert('cached_categories', {
          'id': cat.id,
          'name': cat.name,
          'description': cat.description,
          'display_order': cat.displayOrder,
        });
      }

      await txn.delete('cached_products');
      for (final prod in products) {
        await txn.insert('cached_products', {
          'id': prod.id,
          'category_id': prod.categoryId,
          'name': prod.name,
          'description': prod.description,
          'base_price': prod.basePrice,
          'image_url': prod.imageUrl,
          'is_available': prod.isAvailable ? 1 : 0,
          'variants_json': jsonEncode(prod.variants.map((v) => v.toJson()).toList()),
          'addons_json': jsonEncode(prod.addons.map((a) => a.toJson()).toList()),
        });
      }
    });
  }

  Future<List<CategoryModel>> getCachedCategories() async {
    final db = await instance.database;
    final maps = await db.query('cached_categories', orderBy: 'display_order ASC');
    return maps.map((m) => CategoryModel(
      id: m['id'] as String,
      name: m['name'] as String,
      description: m['description'] as String?,
      displayOrder: m['display_order'] as int,
    )).toList();
  }

  Future<List<ProductModel>> getCachedProducts() async {
    final db = await instance.database;
    final maps = await db.query('cached_products');
    return maps.map((m) {
      final variantsRaw = jsonDecode(m['variants_json'] as String) as List<dynamic>;
      final addonsRaw = jsonDecode(m['addons_json'] as String) as List<dynamic>;

      return ProductModel(
        id: m['id'] as String,
        categoryId: m['category_id'] as String,
        name: m['name'] as String,
        description: m['description'] as String?,
        basePrice: (m['base_price'] as num).toDouble(),
        imageUrl: m['image_url'] as String?,
        isAvailable: (m['is_available'] as int) == 1,
        variants: variantsRaw.map((v) => ProductVariantModel.fromJson(v as Map<String, dynamic>)).toList(),
        addons: addonsRaw.map((a) => AddonModel.fromJson(a as Map<String, dynamic>)).toList(),
      );
    }).toList();
  }

  Future<void> cacheTables(List<TableModel> tables) async {
    final db = await instance.database;
    await db.transaction((txn) async {
      await txn.delete('cached_tables');
      for (final t in tables) {
        await txn.insert('cached_tables', t.toJson());
      }
    });
  }

  Future<List<TableModel>> getCachedTables() async {
    final db = await instance.database;
    final maps = await db.query('cached_tables');
    return maps.map((m) => TableModel.fromJson(m)).toList();
  }

  // --- Outbox Sync Operations ---
  Future<void> queueOrderForSync({
    required OrderDraft draft,
    required String idempotencyKey,
  }) async {
    final db = await instance.database;
    await db.insert('outbox_orders', {
      'client_draft_id': draft.clientDraftId,
      'idempotency_key': idempotencyKey,
      'branch_id': draft.branchId,
      'table_id': draft.tableId,
      'table_number': draft.tableNumber,
      'items_json': jsonEncode(draft.items.map((i) => i.toJson()).toList()),
      'special_instructions': draft.specialInstructions,
      'created_at': draft.createdAt.toIso8601String(),
      'retry_count': 0,
      'last_error': null,
      'is_synced': 0,
    });
  }

  Future<List<Map<String, dynamic>>> getPendingSyncOrders() async {
    final db = await instance.database;
    return await db.query('outbox_orders', where: 'is_synced = ?', whereArgs: [0], orderBy: 'created_at ASC');
  }

  Future<void> markOrderAsSynced(String clientDraftId) async {
    final db = await instance.database;
    await db.update(
      'outbox_orders',
      {'is_synced': 1},
      where: 'client_draft_id = ?',
      whereArgs: [clientDraftId],
    );
  }

  Future<void> updateSyncError(String clientDraftId, String error) async {
    final db = await instance.database;
    await db.rawUpdate('''
      UPDATE outbox_orders 
      SET retry_count = retry_count + 1, last_error = ? 
      WHERE client_draft_id = ?
    ''', [error, clientDraftId]);
  }
}
