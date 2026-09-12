import 'package:flutter/material.dart';
import '../../models/catalog_models.dart';
import '../../models/order_models.dart';

class AddItemSheet extends StatefulWidget {
  final ProductModel product;
  final Function(OrderCartItem item) onAdd;

  const AddItemSheet({
    super.key,
    required this.product,
    required this.onAdd,
  });

  @override
  State<AddItemSheet> createState() => _AddItemSheetState();
}

class _AddItemSheetState extends State<AddItemSheet> {
  ProductVariantModel? _selectedVariant;
  final Set<AddonModel> _selectedAddons = {};
  int _quantity = 1;
  final TextEditingController _notesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    if (widget.product.variants.isNotEmpty) {
      _selectedVariant = widget.product.variants.first;
    }
  }

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  double get _unitPrice => _selectedVariant?.price ?? widget.product.basePrice;

  double get _totalPrice {
    final addonsTotal = _selectedAddons.fold<double>(0.0, (sum, a) => sum + a.price);
    return (_unitPrice + addonsTotal) * _quantity;
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom + 16,
        top: 16,
        left: 16,
        right: 16,
      ),
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    widget.product.name,
                    style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                  ),
                ),
                Text(
                  '\$${_totalPrice.toStringAsFixed(2)}',
                  style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colors.indigo),
                ),
              ],
            ),
            if (widget.product.description != null) ...[
              const SizedBox(height: 4),
              Text(widget.product.description!, style: const TextStyle(color: Colors.grey)),
            ],
            const Divider(height: 24),

            // Variants selection
            if (widget.product.variants.isNotEmpty) ...[
              const Text('Select Variation', style: TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              Wrap(
                spacing: 8,
                children: widget.product.variants.map((v) {
                  final isSelected = _selectedVariant?.id == v.id;
                  return ChoiceChip(
                    label: Text('${v.name} (\$${v.price.toStringAsFixed(2)})'),
                    selected: isSelected,
                    onSelected: (selected) {
                      if (selected) setState(() => _selectedVariant = v);
                    },
                  );
                }).toList(),
              ),
              const SizedBox(height: 16),
            ],

            // Addons selection
            if (widget.product.addons.isNotEmpty) ...[
              const Text('Add-ons & Modifiers', style: TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              ...widget.product.addons.map((a) {
                final isChecked = _selectedAddons.contains(a);
                return CheckboxListTile(
                  dense: true,
                  contentPadding: EdgeInsets.zero,
                  title: Text(a.name),
                  subtitle: Text('+\$${a.price.toStringAsFixed(2)}'),
                  value: isChecked,
                  onChanged: (val) {
                    setState(() {
                      if (val == true) {
                        _selectedAddons.add(a);
                      } else {
                        _selectedAddons.remove(a);
                      }
                    });
                  },
                );
              }),
              const SizedBox(height: 16),
            ],

            // Quantity & Notes
            Row(
              children: [
                const Text('Quantity', style: TextStyle(fontWeight: FontWeight.bold)),
                const Spacer(),
                IconButton(
                  icon: const Icon(Icons.remove_circle_outline),
                  onPressed: _quantity > 1 ? () => setState(() => _quantity--) : null,
                ),
                Text('$_quantity', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                IconButton(
                  icon: const Icon(Icons.add_circle_outline),
                  onPressed: () => setState(() => _quantity++),
                ),
              ],
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _notesController,
              decoration: const InputDecoration(
                labelText: 'Special instructions (e.g. extra spicy, no ice)',
                border: OutlineInputBorder(),
                isDense: true,
              ),
            ),
            const SizedBox(height: 16),

            // Submit Button
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.indigo,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
                onPressed: () {
                  final item = OrderCartItem(
                    productId: widget.product.id,
                    productName: widget.product.name,
                    variantId: _selectedVariant?.id,
                    variantName: _selectedVariant?.name,
                    quantity: _quantity,
                    unitPrice: _unitPrice,
                    notes: _notesController.text.trim().isEmpty ? null : _notesController.text.trim(),
                    addons: _selectedAddons.map((a) => OrderCartAddon(
                      addonId: a.id,
                      name: a.name,
                      unitPrice: a.price,
                    )).toList(),
                  );
                  widget.onAdd(item);
                  Navigator.pop(context);
                },
                child: Text('Add to Order â€¢ \$${_totalPrice.toStringAsFixed(2)}'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
