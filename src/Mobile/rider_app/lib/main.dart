import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:geolocator/geolocator.dart';
import 'blocs/delivery_bloc.dart';
import 'services/rider_signalr_service.dart';
import 'ui/screens/rider_home_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const RiderApp());
}

class RiderApp extends StatefulWidget {
  const RiderApp({super.key});

  @override
  State<RiderApp> createState() => _RiderAppState();
}

class _RiderAppState extends State<RiderApp> {
  final RiderSignalRService _signalRService = RiderSignalRService();
  StreamSubscription<Position>? _positionSubscription;

  final String dummyRiderId = "rider-demo-id";
  final String dummyBranchId = "branch-demo-id";

  @override
  void initState() {
    super.initState();
    _initTracking();
  }

  Future<void> _initTracking() async {
    await _signalRService.connect(
      branchId: dummyBranchId,
      onNewDispatch: (dispatch) {
        // Trigger notification sound / alert
      },
    );

    // Request runtime location permission and start streaming
    final permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      await Geolocator.requestPermission();
    }

    _positionSubscription = Geolocator.getPositionStream(
      locationSettings: const LocationSettings(
        accuracy: LocationAccuracy.high,
        distanceFilter: 20, // Stream every 20 meters
      ),
    ).listen((position) {
      _signalRService.streamLocation(
        branchId: dummyBranchId,
        latitude: position.latitude,
        longitude: position.longitude,
        heading: position.heading,
        speedKmh: position.speed * 3.6,
      );
    });
  }

  @override
  void dispose() {
    _positionSubscription?.cancel();
    _signalRService.disconnect();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(create: (_) => DeliveryBloc()),
      ],
      child: MaterialApp(
        title: 'Restaurant SaaS - Rider Delivery',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
          useMaterial3: true,
        ),
        home: RiderHomeScreen(
          riderId: dummyRiderId,
          branchId: dummyBranchId,
        ),
      ),
    );
  }
}
