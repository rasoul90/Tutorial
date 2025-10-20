import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:provider/provider.dart';

import '../services/identity_controller.dart';

class MrzScanScreen extends StatefulWidget {
  const MrzScanScreen({super.key});

  @override
  State<MrzScanScreen> createState() => _MrzScanScreenState();
}

class _MrzScanScreenState extends State<MrzScanScreen> {
  bool _isProcessing = false;

  @override
  Widget build(BuildContext context) {
    final controller = context.read<IdentityController>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('مسح MRZ'),
      ),
      body: Column(
        children: [
          Expanded(
            child: MobileScanner(
              allowDuplicates: false,
              onDetect: (capture) async {
                if (_isProcessing) return;
                final barcodes = capture.barcodes;
                if (barcodes.isEmpty) return;
                final value = barcodes.first.rawValue;
                if (value == null || value.length < 80) return;
                setState(() {
                  _isProcessing = true;
                });
                await controller.parseMrz(value);
                if (mounted) {
                  Navigator.of(context).pop();
                }
              },
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: const [
                Text(
                  'وجّه الكاميرا نحو منطقة القراءة الآلية (MRZ) في أسفل البطاقة.'
                  '\nتأكد من وجود إضاءة جيدة ومن وضوح الحروف.',
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
