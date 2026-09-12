import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../blocs/delivery_bloc.dart';
import '../../models/delivery_model.dart';
import '../widgets/delivery_card.dart';

class RiderHomeScreen extends StatefulWidget {
  final String riderId;
  final String branchId;

  const RiderHomeScreen({
    super.key,
    required this.riderId,
    required this.branchId,
  });

  @override
  State<RiderHomeScreen> createState() => _RiderHomeScreenState();
}

class _RiderHomeScreenState extends State<RiderHomeScreen> {
  @override
  void initState() {
    super.initState();
    context.read<DeliveryBloc>().add(LoadRiderDeliveries(widget.riderId));
  }

  Future<void> _openMapNavigation(double lat, double lng) async {
    final googleMapsUrl = Uri.parse('google.navigation:q=$lat,$lng&mode=d');
    final webUrl = Uri.parse('https://www.google.com/maps/dir/?api=1&destination=$lat,$lng');

    if (await canLaunchUrl(googleMapsUrl)) {
      await launchUrl(googleMapsUrl);
    } else {
      await launchUrl(webUrl, mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Rider Delivery Dashboard'),
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () => context.read<DeliveryBloc>().add(LoadRiderDeliveries(widget.riderId)),
          ),
        ],
      ),
      body: BlocBuilder<DeliveryBloc, DeliveryState>(
        builder: (context, state) {
          if (state is DeliveryLoading) {
            return const Center(child: CircularProgressIndicator());
          }
          if (state is DeliveryError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(state.message, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: () => context.read<DeliveryBloc>().add(LoadRiderDeliveries(widget.riderId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            );
          }
          if (state is DeliveryLoaded) {
            if (state.activeDeliveries.isEmpty) {
              return const Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.two_wheeler, size: 64, color: Colors.grey),
                    SizedBox(height: 12),
                    Text('No active delivery assignments.', style: TextStyle(fontSize: 16, color: Colors.grey)),
                  ],
                ),
              );
            }
            return ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: state.activeDeliveries.length,
              itemBuilder: (context, index) {
                final delivery = state.activeDeliveries[index];
                return DeliveryCard(
                  delivery: delivery,
                  onAccept: () {
                    context.read<DeliveryBloc>().add(AcceptDeliveryEvent(delivery.id));
                  },
                  onPickup: () {
                    context.read<DeliveryBloc>().add(PickupDeliveryEvent(delivery.id));
                  },
                  onComplete: () {
                    context.read<DeliveryBloc>().add(
                          CompleteDeliveryEvent(
                            dispatchId: delivery.id,
                            cashCollected: delivery.cashToCollect,
                          ),
                        );
                  },
                  onNavigate: () {
                    if (delivery.destinationLatitude != null && delivery.destinationLongitude != null) {
                      _openMapNavigation(delivery.destinationLatitude!, delivery.destinationLongitude!);
                    }
                  },
                );
              },
            );
          }
          return const SizedBox.shrink();
        },
      ),
    );
  }
}
