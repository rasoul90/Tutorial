import 'package:camera/camera.dart';
import 'package:flutter/material.dart';

import '../models/document_type.dart';
import 'mrz_capture_screen.dart';

class MrzInstructionsScreen extends StatelessWidget {
  const MrzInstructionsScreen({
    super.key,
    required this.documentType,
    required this.availableCameras,
  });

  final DocumentType documentType;
  final List<CameraDescription> availableCameras;

  void _openCamera(BuildContext context) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => MrzCaptureScreen(
          documentType: documentType,
          availableCameras: availableCameras,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final instructions = [
      'ضع ${documentType == DocumentType.passport ? 'جواز السفر' : 'البطاقة الوطنية'} على سطح مستوٍ ومضيء.',
      'قم بمحاذاة منطقة MRZ داخل الإطار الأبيض بشكل كامل.',
      'تأكد من خلو الزجاج أو الحافظة من الانعكاسات قدر الإمكان.',
    ];

    return Scaffold(
      appBar: AppBar(
        title: Text('${documentType.displayName}'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'التقط صورة واضحة لمنطقة MRZ',
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 12),
              ...instructions.map(
                (line) => Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('• '),
                      Expanded(
                        child: Text(
                          line,
                          style: Theme.of(context).textTheme.bodyLarge,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              const Spacer(),
              FilledButton.icon(
                onPressed: () => _openCamera(context),
                icon: const Icon(Icons.camera_alt_outlined),
                label: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  child: Text('التالي'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
