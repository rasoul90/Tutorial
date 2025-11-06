import 'package:flutter/material.dart';

import '../models/document_type.dart';
import '../models/mrz_data.dart';
import 'nfc_reading_screen.dart';

class NfcInstructionsScreen extends StatelessWidget {
  const NfcInstructionsScreen({
    super.key,
    required this.documentType,
    required this.mrzData,
  });

  final DocumentType documentType;
  final MrzData mrzData;

  @override
  Widget build(BuildContext context) {
    final bulletPoints = [
      'فعّل خاصية NFC على هاتفك.',
      'ضع ${documentType.displayName} في منتصف ظهر الهاتف بحيث يلامس الشريحة منطقة الاستشعار.',
      'ابقَ ثابتاً حتى تكتمل عملية القراءة تماماً.',
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('الاستعداد لقراءة NFC'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'سيتم استخدام بيانات MRZ التالية لبدء الاتصال الآمن مع الشريحة:',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
              const SizedBox(height: 12),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surfaceVariant,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    for (final line in mrzData.lines)
                      Text(
                        line,
                        style: const TextStyle(fontFamily: 'monospace'),
                      ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              Text(
                'إرشادات القراءة:',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              ...bulletPoints.map(
                (line) => Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('• '),
                      Expanded(child: Text(line)),
                    ],
                  ),
                ),
              ),
              const Spacer(),
              FilledButton(
                onPressed: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => NfcReadingScreen(
                        documentType: documentType,
                        mrzData: mrzData,
                      ),
                    ),
                  );
                },
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  child: Text('بدء القراءة'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
