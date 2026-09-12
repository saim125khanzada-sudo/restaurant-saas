import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../models/table_models.dart';
import '../../services/api_service.dart';
import '../../services/local_database_service.dart';

// --- Events ---
abstract class TableEvent extends Equatable {
  const TableEvent();
  @override
  List<Object?> get props => [];
}

class LoadTables extends TableEvent {
  final String branchId;
  const LoadTables(this.branchId);
  @override
  List<Object?> get props => [branchId];
}

class TableStatusUpdatedEvent extends TableEvent {
  final String tableId;
  final String newStatus;
  const TableStatusUpdatedEvent(this.tableId, this.newStatus);
  @override
  List<Object?> get props => [tableId, newStatus];
}

// --- States ---
abstract class TableState extends Equatable {
  const TableState();
  @override
  List<Object?> get props => [];
}

class TableInitial extends TableState {}
class TableLoading extends TableState {}
class TableLoaded extends TableState {
  final List<TableModel> tables;
  const TableLoaded(this.tables);
  @override
  List<Object?> get props => [tables];
}
class TableError extends TableState {
  final String message;
  const TableError(this.message);
  @override
  List<Object?> get props => [message];
}

// --- Bloc ---
class TableBloc extends Bloc<TableEvent, TableState> {
  final ApiService _apiService;
  final LocalDatabaseService _dbService;

  TableBloc({
    ApiService? apiService,
    LocalDatabaseService? dbService,
  })  : _apiService = apiService ?? ApiService(),
        _dbService = dbService ?? LocalDatabaseService.instance,
        super(TableInitial()) {
    on<LoadTables>(_onLoadTables);
    on<TableStatusUpdatedEvent>(_onTableStatusUpdated);
  }

  Future<void> _onLoadTables(LoadTables event, Emitter<TableState> emit) async {
    emit(TableLoading());
    try {
      // Fetch fresh tables from API
      final tables = await _apiService.getTables(event.branchId);
      await _dbService.cacheTables(tables);
      emit(TableLoaded(tables));
    } catch (_) {
      // Fallback to offline cached tables
      final cached = await _dbService.getCachedTables();
      if (cached.isNotEmpty) {
        emit(TableLoaded(cached));
      } else {
        emit(const TableError('Failed to load tables and no offline cache available.'));
      }
    }
  }

  void _onTableStatusUpdated(TableStatusUpdatedEvent event, Emitter<TableState> emit) {
    if (state is TableLoaded) {
      final currentTables = (state as TableLoaded).tables;
      final updated = currentTables.map((t) {
        if (t.id == event.tableId) {
          return t.copyWith(status: event.newStatus);
        }
        return t;
      }).toList();
      emit(TableLoaded(updated));
    }
  }
}
