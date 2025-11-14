import 'dart:io';
import 'dart:ui';

import 'package:flutter/material.dart';

import '../models/document_type.dart';
import '../models/mrz_scan_result.dart';
import 'nfc_instructions_screen.dart';

class MrzReviewScreen extends StatelessWidget {
  const MrzReviewScreen({
    super.key,
    required this.documentType,
    required this.capturePath,
    required this.scanResult,
  });

  final DocumentType documentType;
  final String capturePath;
  final MrzScanResult scanResult;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('تأكيد منطقة MRZ'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Expanded(
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(16),
                  child: Container(
                    color: Colors.black,
                    child: FittedBox(
                      fit: BoxFit.contain,
                      child: SizedBox(
                        width: scanResult.imageSize.width,
                        height: scanResult.imageSize.height,
                        child: Stack(
                          fit: StackFit.expand,
                          children: [
                            Image.file(
                              File(capturePath),
                              fit: BoxFit.cover,
                            ),
                            Positioned.fill(
                              child: CustomPaint(
                                painter: _MrzBoundingPainter(
                                  boundingBox: scanResult.boundingBox,
                                  imageSize: scanResult.imageSize,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Text(
                'تم العثور على النص التالي:',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surfaceVariant,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    for (final line in scanResult.data.lines)
                      Text(
                        line,
                        style: const TextStyle(fontFeatures: [FontFeature.tabularFigures()]),
                      ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => NfcInstructionsScreen(
                        documentType: documentType,
                        mrzData: scanResult.data,
                      ),
                    ),
                  );
                },
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  child: Text('متابعة إلى قراءة NFC'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _MrzBoundingPainter extends CustomPainter {
  _MrzBoundingPainter({required this.boundingBox, required this.imageSize});

  final Rect boundingBox;
  final Size imageSize;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = Colors.greenAccent.withOpacity(0.8)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 6;

    final scaleX = size.width / imageSize.width;
    final scaleY = size.height / imageSize.height;

    final rect = Rect.fromLTRB(
      boundingBox.left * scaleX,
      boundingBox.top * scaleY,
      boundingBox.right * scaleX,
      boundingBox.bottom * scaleY,
    );

    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(18)),
      paint,
    );
  }

  @override
  bool shouldRepaint(covariant _MrzBoundingPainter oldDelegate) {
    return oldDelegate.boundingBox != boundingBox ||
        oldDelegate.imageSize != imageSize;
  }
}
