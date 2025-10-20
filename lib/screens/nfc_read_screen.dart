import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../services/identity_controller.dart';

class NfcReadScreen extends StatelessWidget {
  const NfcReadScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<IdentityController>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('قراءة NFC'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Icon(Icons.nfc, size: 96),
            const SizedBox(height: 24),
            const Text(
              'ضع البطاقة الوطنية خلف الهاتف وانتظر حتى يتم الحصول على بيانات الشريحة.',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            if (controller.isLoading)
              const LinearProgressIndicator()
            else
              ElevatedButton.icon(
                onPressed: controller.readNfc,
                icon: const Icon(Icons.play_arrow),
                label: const Text('ابدأ القراءة'),
              ),
            const SizedBox(height: 16),
            if (controller.error != null)
              Text(
                controller.error!,
                textAlign: TextAlign.center,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
          ],
        ),
      ),
    );
  }
}
