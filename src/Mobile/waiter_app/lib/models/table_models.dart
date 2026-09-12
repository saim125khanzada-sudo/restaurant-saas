import 'package:equatable/equatable.dart';

class TableModel extends Equatable {
  final String id;
  final String branchId;
  final String sectionId;
  final String tableNumber;
  final int capacity;
  final String status; // Available, Occupied, Reserved, Billed

  const TableModel({
    required this.id,
    required this.branchId,
    required this.sectionId,
    required this.tableNumber,
    required this.capacity,
    required this.status,
  });

  factory TableModel.fromJson(Map<String, dynamic> json) {
    return TableModel(
      id: json['id'] as String,
      branchId: json['branchId'] as String,
      sectionId: json['sectionId'] as String,
      tableNumber: json['tableNumber'] as String,
      capacity: json['capacity'] as int,
      status: json['status'] as String,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'branchId': branchId,
    'sectionId': sectionId,
    'tableNumber': tableNumber,
    'capacity': capacity,
    'status': status,
  };

  TableModel copyWith({
    String? id,
    String? branchId,
    String? sectionId,
    String? tableNumber,
    int? capacity,
    String? status,
  }) {
    return TableModel(
      id: id ?? this.id,
      branchId: branchId ?? this.branchId,
      sectionId: sectionId ?? this.sectionId,
      tableNumber: tableNumber ?? this.tableNumber,
      capacity: capacity ?? this.capacity,
      status: status ?? this.status,
    );
  }

  @override
  List<Object?> get props => [id, branchId, sectionId, tableNumber, capacity, status];
}

class SectionModel extends Equatable {
  final String id;
  final String branchId;
  final String name;
  final int displayOrder;

  const SectionModel({
    required this.id,
    required this.branchId,
    required this.name,
    required this.displayOrder,
  });

  factory SectionModel.fromJson(Map<String, dynamic> json) {
    return SectionModel(
      id: json['id'] as String,
      branchId: json['branchId'] as String,
      name: json['name'] as String,
      displayOrder: json['displayOrder'] as int,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'branchId': branchId,
    'name': name,
    'displayOrder': displayOrder,
  };

  @override
  List<Object?> get props => [id, branchId, name, displayOrder];
}
