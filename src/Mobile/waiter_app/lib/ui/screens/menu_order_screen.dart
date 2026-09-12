import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../blocs/catalog_bloc.dart';
import '../../blocs/order_cart_bloc.dart';
import '../../models/catalog_models.dart';
import '../widgets/add_item_sheet.dart';

class MenuOrderScreen extends StatefulWidget {
  final String tableId;
  final String tableNumber;
  final String branchId;

  const MenuOrderScreen({
    super.key,
    required this.tableId,
    required this.tableNumber,
    required this.branchId,
  });

  @override
  State<MenuOrderScreen> createState() => _MenuOrderScreenState();
}

class _MenuOrderScreenState extends State<MenuOrderScreen> {
  @override
  void initState() {
    super.initState();
    context.read<CatalogBloc>().add(LoadCatalog());
  }

  void _showAddItemSheet(ProductModel product) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (_) => AddItemSheet(
        product: product,
        onAdd: (item) {
          context.read<OrderCartBloc>().add(AddItemToCart(item));
        },
      ),
    );
  }

  void _showOrderSummarySheet(BuildContext context, OrderCartActive cartState) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (ctx) => Container(
        padding: const EdgeInsets.all(16),
        height: MediaQuery.of(context).size.height * 0.7,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Order Summary - Table ${widget.tableNumber}',
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.pop(ctx),
                ),
              ],
            ),
            const Divider(),
            Expanded(
              child: ListView.separated(
                itemCount: cartState.items.length,
                separatorBuilder: (_, __) => const Divider(),
                itemBuilder: (c, idx) {
                  final item = cartState.items[idx];
                  return ListTile(
                    contentPadding: EdgeInsets.zero,
                    title: Text('${item.quantity}x ${item.productName}'),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        if (item.variantName != null)
                          Text('Variation: ${item.variantName}', style: const TextStyle(fontSize: 12)),
                        if (item.addons.isNotEmpty)
                          Text('Addons: ${item.addons.map((a) => a.name).join(", ")}', style: const TextStyle(fontSize: 12)),
                        if (item.notes != null)
                          Text('Note: ${item.notes}', style: const TextStyle(fontSize: 12, fontStyle: FontStyle.italic)),
                      ],
                    ),
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text('\$${item.totalPrice.toStringAsFixed(2)}', style: const TextStyle(fontWeight: FontWeight.bold)),
                        IconButton(
                          icon: const Icon(Icons.delete_outline, color: Colors.red),
                          onPressed: () => context.read<OrderCartBloc>().add(RemoveItemFromCart(idx)),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
            const Divider(),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text('Subtotal', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                Text('\$${cartState.subtotal.toStringAsFixed(2)}', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              ],
            ),
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.green.shade700,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
                icon: const Icon(Icons.send),
                label: const Text('Fire KOT to Kitchen', style: TextStyle(fontSize: 16)),
                onPressed: () {
                  Navigator.pop(ctx);
                  context.read<OrderCartBloc>().add(const FireKotToKitchen());
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<OrderCartBloc, OrderCartState>(
      listener: (context, state) {
        if (state is OrderSubmittedSuccess) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              backgroundColor: state.isQueuedOffline ? Colors.orange.shade800 : Colors.green.shade700,
            ),
          );
          Navigator.pop(context); // Return to Table Grid
        } else if (state is OrderSubmissionError) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(state.error), backgroundColor: Colors.red),
          );
        }
      },
      child: Scaffold(
        appBar: AppBar(
          title: Text('Table ${widget.tableNumber} - Order'),
          backgroundColor: Colors.indigo,
          foregroundColor: Colors.white,
        ),
        body: Column(
          children: [
            // Category Bar
            BlocBuilder<CatalogBloc, CatalogState>(
              builder: (context, state) {
                if (state is CatalogLoaded) {
                  return Container(
                    height: 50,
                    padding: const EdgeInsets.symmetric(vertical: 6),
                    child: ListView(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 12),
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(right: 8),
                          child: ChoiceChip(
                            label: const Text('All Items'),
                            selected: state.selectedCategoryId == null,
                            onSelected: (_) => context.read<CatalogBloc>().add(const SelectCategory(null)),
                          ),
                        ),
                        ...state.categories.map((c) => Padding(
                              padding: const EdgeInsets.only(right: 8),
                              child: ChoiceChip(
                                label: Text(c.name),
                                selected: state.selectedCategoryId == c.id,
                                onSelected: (_) => context.read<CatalogBloc>().add(SelectCategory(c.id)),
                              ),
                            )),
                      ],
                    ),
                  );
                }
                return const SizedBox.shrink();
              },
            ),
            const Divider(height: 1),

            // Products Grid
            Expanded(
              child: BlocBuilder<CatalogBloc, CatalogState>(
                builder: (context, state) {
                  if (state is CatalogLoading) {
                    return const Center(child: CircularProgressIndicator());
                  }
                  if (state is CatalogError) {
                    return Center(child: Text(state.message, style: const TextStyle(color: Colors.red)));
                  }
                  if (state is CatalogLoaded) {
                    final products = state.filteredProducts;
                    if (products.isEmpty) {
                      return const Center(child: Text('No menu items in this category.'));
                    }
                    return ListView.separated(
                      padding: const EdgeInsets.all(12),
                      itemCount: products.length,
                      separatorBuilder: (_, __) => const Divider(),
                      itemBuilder: (context, idx) {
                        final prod = products[idx];
                        return ListTile(
                          title: Text(prod.name, style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: prod.description != null ? Text(prod.description!, maxLines: 1) : null,
                          trailing: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Text(
                                '\$${prod.basePrice.toStringAsFixed(2)}',
                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                              ),
                              const SizedBox(width: 8),
                              ElevatedButton(
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: Colors.indigo,
                                  foregroundColor: Colors.white,
                                  padding: const EdgeInsets.symmetric(horizontal: 12),
                                ),
                                onPressed: () => _showAddItemSheet(prod),
                                child: const Text('Add'),
                              ),
                            ],
                          ),
                        );
                      },
                    );
                  }
                  return const SizedBox.shrink();
                },
              ),
            ),
          ],
        ),

        // Cart Bottom Bar
        bottomNavigationBar: BlocBuilder<OrderCartBloc, OrderCartState>(
          builder: (context, state) {
            if (state is OrderCartActive && state.items.isNotEmpty) {
              return Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  boxShadow: [
                    BoxShadow(color: Colors.black.withOpacity(0.08), blurRadius: 10, offset: const Offset(0, -2))
                  ],
                ),
                child: SafeArea(
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            '${state.items.fold<int>(0, (sum, i) => sum + i.quantity)} Items',
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                          Text(
                            '\$${state.subtotal.toStringAsFixed(2)}',
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.indigo),
                          ),
                        ],
                      ),
                      ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.indigo,
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                        ),
                        icon: const Icon(Icons.shopping_bag_outlined),
                        label: const Text('View Cart / Fire KOT'),
                        onPressed: () => _showOrderSummarySheet(context, state),
                      ),
                    ],
                  ),
                ),
              );
            }
            return const SizedBox.shrink();
          },
        ),
      ),
    );
  }
}
