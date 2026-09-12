import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'blocs/auth_bloc.dart';
import 'blocs/table_bloc.dart';
import 'blocs/catalog_bloc.dart';
import 'blocs/order_cart_bloc.dart';
import 'services/sync_engine.dart';
import 'services/signalr_service.dart';
import 'ui/screens/login_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  final syncEngine = SyncEngine();
  syncEngine.startPeriodicSync();

  final signalRService = SignalRService();

  runApp(WaiterApp(
    syncEngine: syncEngine,
    signalRService: signalRService,
  ));
}

class WaiterApp extends StatelessWidget {
  final SyncEngine syncEngine;
  final SignalRService signalRService;

  const WaiterApp({
    super.key,
    required this.syncEngine,
    required this.signalRService,
  });

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(create: (_) => AuthBloc()),
        BlocProvider(create: (_) => TableBloc()),
        BlocProvider(create: (_) => CatalogBloc()),
        BlocProvider(create: (_) => OrderCartBloc(syncEngine: syncEngine)),
      ],
      child: MaterialApp(
        title: 'Restaurant SaaS - Waiter',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
          useMaterial3: true,
        ),
        home: const LoginScreen(),
      ),
    );
  }
}
