import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../models/catalog_models.dart';
import '../../services/api_service.dart';
import '../../services/local_database_service.dart';

// --- Events ---
abstract class CatalogEvent extends Equatable {
  const CatalogEvent();
  @override
  List<Object?> get props => [];
}

class LoadCatalog extends CatalogEvent {}

class SelectCategory extends CatalogEvent {
  final String? categoryId;
  const SelectCategory(this.categoryId);
  @override
  List<Object?> get props => [categoryId];
}

// --- States ---
abstract class CatalogState extends Equatable {
  const CatalogState();
  @override
  List<Object?> get props => [];
}

class CatalogInitial extends CatalogState {}
class CatalogLoading extends CatalogState {}
class CatalogLoaded extends CatalogState {
  final List<CategoryModel> categories;
  final List<ProductModel> products;
  final String? selectedCategoryId;

  const CatalogLoaded({
    required this.categories,
    required this.products,
    this.selectedCategoryId,
  });

  List<ProductModel> get filteredProducts {
    if (selectedCategoryId == null || selectedCategoryId!.isEmpty) {
      return products;
    }
    return products.where((p) => p.categoryId == selectedCategoryId).toList();
  }

  CatalogLoaded copyWith({
    List<CategoryModel>? categories,
    List<ProductModel>? products,
    String? selectedCategoryId,
  }) {
    return CatalogLoaded(
      categories: categories ?? this.categories,
      products: products ?? this.products,
      selectedCategoryId: selectedCategoryId,
    );
  }

  @override
  List<Object?> get props => [categories, products, selectedCategoryId];
}

class CatalogError extends CatalogState {
  final String message;
  const CatalogError(this.message);
  @override
  List<Object?> get props => [message];
}

// --- Bloc ---
class CatalogBloc extends Bloc<CatalogEvent, CatalogState> {
  final ApiService _apiService;
  final LocalDatabaseService _dbService;

  CatalogBloc({
    ApiService? apiService,
    LocalDatabaseService? dbService,
  })  : _apiService = apiService ?? ApiService(),
        _dbService = dbService ?? LocalDatabaseService.instance,
        super(CatalogInitial()) {
    on<LoadCatalog>(_onLoadCatalog);
    on<SelectCategory>(_onSelectCategory);
  }

  Future<void> _onLoadCatalog(LoadCatalog event, Emitter<CatalogState> emit) async {
    emit(CatalogLoading());
    try {
      final categories = await _apiService.getCategories();
      final products = await _apiService.getProducts();
      await _dbService.cacheCatalog(categories, products);
      emit(CatalogLoaded(categories: categories, products: products));
    } catch (_) {
      // Offline fallback
      final cachedCats = await _dbService.getCachedCategories();
      final cachedProds = await _dbService.getCachedProducts();
      if (cachedCats.isNotEmpty) {
        emit(CatalogLoaded(categories: cachedCats, products: cachedProds));
      } else {
        emit(const CatalogError('Failed to load menu catalog and no offline cache available.'));
      }
    }
  }

  void _onSelectCategory(SelectCategory event, Emitter<CatalogState> emit) {
    if (state is CatalogLoaded) {
      final s = state as CatalogLoaded;
      emit(s.copyWith(selectedCategoryId: event.categoryId));
    }
  }
}
