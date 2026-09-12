import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../blocs/auth_bloc.dart';
import '../../blocs/table_bloc.dart';
import '../../blocs/order_cart_bloc.dart';
import 'menu_order_screen.dart';
import '../widgets/table_card.dart';

class TableGridScreen extends StatefulWidget {
  final String branchId;
  const TableGridScreen({super.key, required this.branchId});

  @override
  State<TableGridScreen> createState() => _TableGridScreenState();
}

class _TableGridScreenState extends State<TableGridScreen> {
  @override
  void initState() {
    super.initState();
    context.read<TableBloc>().add(LoadTables(widget.branchId));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Dining Tables'),
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () => context.read<TableBloc>().add(LoadTables(widget.branchId)),
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => context.read<AuthBloc>().add(LogoutRequested()),
          ),
        ],
      ),
      body: BlocBuilder<TableBloc, TableState>(
        builder: (context, state) {
          if (state is TableLoading) {
            return const Center(child: CircularProgressIndicator());
          }
          if (state is TableError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(state.message, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: () => context.read<TableBloc>().add(LoadTables(widget.branchId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            );
          }
          if (state is TableLoaded) {
            if (state.tables.isEmpty) {
              return const Center(child: Text('No tables found for this branch.'));
            }
            return GridView.builder(
              padding: const EdgeInsets.all(16),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                crossAxisSpacing: 12,
                mainAxisSpacing: 12,
                childAspectRatio: 1.1,
              ),
              itemCount: state.tables.length,
              itemBuilder: (context, index) {
                final table = state.tables[index];
                return TableCard(
                  table: table,
                  onTap: () {
                    context.read<OrderCartBloc>().add(
                          SetActiveTable(
                            branchId: widget.branchId,
                            tableId: table.id,
                            tableNumber: table.tableNumber,
                          ),
                        );
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => MenuOrderScreen(
                          tableId: table.id,
                          tableNumber: table.tableNumber,
                          branchId: widget.branchId,
                        ),
                      ),
                    );
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
