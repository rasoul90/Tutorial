import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../services/identity_controller.dart';
import '../widgets/identity_summary.dart';
import 'mrz_scan_screen.dart';
import 'nfc_read_screen.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final controller = context.watch<IdentityController>();
    final document = controller.document;

    return Scaffold(
      appBar: AppBar(
        title: const Text('قارئ الهوية الوطنية العراقية'),
        actions: [
          IconButton(
            onPressed: controller.isLoading ? null : controller.reset,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              child: ListTile(
                leading: const Icon(Icons.qr_code_scanner),
                title: const Text('الخطوة ١: قراءة ال MRZ'),
                subtitle: const Text('قم بمسح منطقة القراءة الآلية من البطاقة.'),
                trailing: const Icon(Icons.chevron_right),
                onTap: controller.isLoading
                    ? null
                    : () => Navigator.of(context).push(
                          MaterialPageRoute(
                            builder: (_) => const MrzScanScreen(),
                          ),
                        ),
              ),
            ),
            const SizedBox(height: 12),
            Card(
              child: ListTile(
                leading: const Icon(Icons.nfc),
                title: const Text('الخطوة ٢: قراءة بيانات NFC'),
                subtitle: const Text('بعد قراءة MRZ ضع البطاقة خلف الهاتف.'),
                trailing: const Icon(Icons.chevron_right),
                onTap: controller.isLoading
                    ? null
                    : () => Navigator.of(context).push(
                          MaterialPageRoute(
                            builder: (_) => const NfcReadScreen(),
                          ),
                        ),
              ),
            ),
            const SizedBox(height: 16),
            if (controller.isLoading) ...[
              const LinearProgressIndicator(),
              const SizedBox(height: 12),
            ],
            if (controller.error != null) ...[
              Text(
                controller.error!,
                style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.error),
              ),
              const SizedBox(height: 12),
            ],
            Expanded(
              child: document == null
                  ? const Center(
                      child: Text('لم يتم قراءة أي بطاقة بعد.'),
                    )
                  : SingleChildScrollView(
                      child: IdentitySummary(document: document),
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
