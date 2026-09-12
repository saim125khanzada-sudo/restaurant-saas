import 'package:flutter/material.dart';
import '../../models/delivery_model.dart';

class DeliveryCard extends StatelessWidget {
  final DeliveryModel delivery;
  final VoidCallback onAccept;
  final VoidCallback onPickup;
  final VoidCallback onComplete;
  final VoidCallback onNavigate;

  const DeliveryCard({
    super.key,
    required this.delivery,
    required this.onAccept,
    required this.onPickup,
    required this.onComplete,
    required this.onNavigate,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      margin: const EdgeInsets.only(bottom: 16),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Order #${delivery.orderNumber}',
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.blue.shade200),
                  ),
                  child: Text(
                    delivery.status.displayName,
                    style: TextStyle(color: Colors.blue.shade800, fontWeight: FontWeight.bold, fontSize: 12),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Icon(Icons.location_on, color: Colors.red, size: 20),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    delivery.deliveryAddress,
                    style: const TextStyle(fontSize: 14),
                  ),
                ),
              ],
            ),
            if (delivery.customerName != null || delivery.customerPhone != null) ...[
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(Icons.person, color: Colors.grey, size: 18),
                  const SizedBox(width: 8),
                  Text('${delivery.customerName ?? "Customer"} â€¢ ${delivery.customerPhone ?? ""}'),
                ],
              ),
            ],
            const Divider(height: 24),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Cash to Collect', style: TextStyle(fontSize: 12, color: Colors.grey)),
                    Text(
                      '\$${delivery.cashToCollect.toStringAsFixed(2)}',
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.green),
                    ),
                  ],
                ),
                if (delivery.destinationLatitude != null && delivery.destinationLongitude != null)
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.blue.shade700,
                      foregroundColor: Colors.white,
                    ),
                    icon: const Icon(Icons.navigation, size: 16),
                    label: const Text('Navigate'),
                    onPressed: onNavigate,
                  ),
              ],
            ),
            const SizedBox(height: 16),

            // Lifecycle Action Buttons
            if (delivery.status == DeliveryStatus.assigned)
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.green.shade700,
                    foregroundColor: Colors.white,
                  ),
                  onPressed: onAccept,
                  child: const Text('Accept Delivery Request'),
                ),
              )
            else if (delivery.status == DeliveryStatus.accepted)
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.orange.shade800,
                    foregroundColor: Colors.white,
                  ),
                  onPressed: onPickup,
                  child: const Text('Confirm Pickup from Kitchen'),
                ),
              )
            else if (delivery.status == DeliveryStatus.pickedUp || delivery.status == DeliveryStatus.inTransit)
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.indigo,
                    foregroundColor: Colors.white,
                  ),
                  onPressed: onComplete,
                  child: Text('Delivered & Collected \$${delivery.cashToCollect.toStringAsFixed(2)}'),
                ),
              ),
          ],
        ),
      ),
    );
  }
}
