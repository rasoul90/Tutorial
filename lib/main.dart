import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'screens/home_screen.dart';
import 'services/identity_controller.dart';

void main() {
  runApp(const IraqiNationalIdReaderApp());
}

class IraqiNationalIdReaderApp extends StatelessWidget {
  const IraqiNationalIdReaderApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (_) => IdentityController(),
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        title: 'Iraqi National ID Reader',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF174D73)),
          useMaterial3: true,
          fontFamily: 'Roboto',
        ),
        home: const HomeScreen(),
      ),
    );
  }
}
