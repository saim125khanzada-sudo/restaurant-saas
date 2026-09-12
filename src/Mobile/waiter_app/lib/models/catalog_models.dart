import 'package:equatable/equatable.dart';

class CategoryModel extends Equatable {
  final String id;
  final String name;
  final String? description;
  final int displayOrder;

  const CategoryModel({
    required this.id,
    required this.name,
    this.description,
    required this.displayOrder,
  });

  factory CategoryModel.fromJson(Map<String, dynamic> json) {
    return CategoryModel(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      displayOrder: json['displayOrder'] as int,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'description': description,
    'displayOrder': displayOrder,
  };

  @override
  List<Object?> get props => [id, name, description, displayOrder];
}

class ProductVariantModel extends Equatable {
  final String id;
  final String productId;
  final String name;
  final double price;
  final String? sku;

  const ProductVariantModel({
    required this.id,
    required this.productId,
    required this.name,
    required this.price,
    this.sku,
  });

  factory ProductVariantModel.fromJson(Map<String, dynamic> json) {
    return ProductVariantModel(
      id: json['id'] as String,
      productId: json['productId'] as String,
      name: json['name'] as String,
      price: (json['price'] as num).toDouble(),
      sku: json['sku'] as String?,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'productId': productId,
    'name': name,
    'price': price,
    'sku': sku,
  };

  @override
  List<Object?> get props => [id, productId, name, price, sku];
}

class AddonModel extends Equatable {
  final String id;
  final String name;
  final double price;

  const AddonModel({
    required this.id,
    required this.name,
    required this.price,
  });

  factory AddonModel.fromJson(Map<String, dynamic> json) {
    return AddonModel(
      id: json['id'] as String,
      name: json['name'] as String,
      price: (json['price'] as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'price': price,
  };

  @override
  List<Object?> get props => [id, name, price];
}

class ProductModel extends Equatable {
  final String id;
  final String categoryId;
  final String name;
  final String? description;
  final double basePrice;
  final String? imageUrl;
  final bool isAvailable;
  final List<ProductVariantModel> variants;
  final List<AddonModel> addons;

  const ProductModel({
    required this.id,
    required this.categoryId,
    required this.name,
    this.description,
    required this.basePrice,
    this.imageUrl,
    required this.isAvailable,
    this.variants = const [],
    this.addons = const [],
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) {
    return ProductModel(
      id: json['id'] as String,
      categoryId: json['categoryId'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      basePrice: (json['basePrice'] as num).toDouble(),
      imageUrl: json['imageUrl'] as String?,
      isAvailable: json['isAvailable'] as bool? ?? true,
      variants: (json['variants'] as List<dynamic>?)
              ?.map((v) => ProductVariantModel.fromJson(v as Map<String, dynamic>))
              .toList() ??
          [],
      addons: (json['addons'] as List<dynamic>?)
              ?.map((a) => AddonModel.fromJson(a as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'categoryId': categoryId,
    'name': name,
    'description': description,
    'basePrice': basePrice,
    'imageUrl': imageUrl,
    'isAvailable': isAvailable,
    'variants': variants.map((v) => v.toJson()).toList(),
    'addons': addons.map((a) => a.toJson()).toList(),
  };

  @override
  List<Object?> get props => [id, categoryId, name, description, basePrice, isAvailable, variants, addons];
}
