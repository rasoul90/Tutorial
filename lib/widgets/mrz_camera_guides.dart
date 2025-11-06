import 'dart:ui' as ui;

import 'package:flutter/material.dart';

class MrzCameraGuides extends StatelessWidget {
  const MrzCameraGuides({super.key, this.detectedMrz});

  final ui.Rect? detectedMrz;

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: LayoutBuilder(
        builder: (context, constraints) {
          final size = Size(constraints.maxWidth, constraints.maxHeight);
          return SizedBox.expand(
            child: CustomPaint(
              size: size,
              painter: _MrzGuidePainter(
                detectedMrz: detectedMrz,
              ),
            ),
          );
        },
      ),
    );
  }
}

class _MrzGuidePainter extends CustomPainter {
  _MrzGuidePainter({required this.detectedMrz});

  final ui.Rect? detectedMrz;

  @override
  void paint(Canvas canvas, Size size) {
    _drawDimmedBackground(canvas, size);
    _drawCardOutline(canvas, size);
    _drawMrzZone(canvas, size);
    _drawDetection(canvas, size);
  }

  @override
  bool shouldRepaint(covariant _MrzGuidePainter oldDelegate) {
    return oldDelegate.detectedMrz != detectedMrz;
  }

  void _drawDimmedBackground(Canvas canvas, Size size) {
    final cardRect = _cardRect(size);
    final overlayPaint = Paint()..color = Colors.black.withOpacity(0.55);

    canvas.saveLayer(null, Paint());
    canvas.drawRect(Offset.zero & size, overlayPaint);
    canvas.drawRRect(
      RRect.fromRectAndRadius(cardRect, const Radius.circular(28)),
      Paint()..blendMode = BlendMode.clear,
    );
    canvas.restore();
  }

  void _drawCardOutline(Canvas canvas, Size size) {
    final cardRect = _cardRect(size);
    final borderPaint = Paint()
      ..color = Colors.white.withOpacity(0.9)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 3;

    canvas.drawRRect(
      RRect.fromRectAndRadius(cardRect, const Radius.circular(28)),
      borderPaint,
    );

    final headerRect = Rect.fromLTWH(
      cardRect.left + 16,
      cardRect.top + 16,
      cardRect.width - 32,
      (cardRect.height * 0.24).clamp(40, cardRect.height / 2),
    );

    final headerPaint = Paint()
      ..color = Colors.white.withOpacity(0.2)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;

    canvas.drawRRect(
      RRect.fromRectAndRadius(headerRect, const Radius.circular(20)),
      headerPaint,
    );
  }

  void _drawMrzZone(Canvas canvas, Size size) {
    final cardRect = _cardRect(size);
    final horizontalMargin = cardRect.width * 0.08;
    final mrzHeight = cardRect.height * 0.28;

    final mrzRect = Rect.fromLTWH(
      cardRect.left + horizontalMargin,
      cardRect.bottom - mrzHeight - horizontalMargin,
      cardRect.width - (horizontalMargin * 2),
      mrzHeight,
    );

    final mrzBackground = Paint()
      ..color = Colors.white.withOpacity(0.25)
      ..style = PaintingStyle.fill;

    canvas.drawRRect(
      RRect.fromRectAndRadius(mrzRect, const Radius.circular(18)),
      mrzBackground,
    );

    final linePaint = Paint()
      ..color = Colors.white.withOpacity(0.7)
      ..strokeWidth = 1.4;

    final lineCount = 3;
    for (var i = 1; i <= lineCount; i++) {
      final dy = mrzRect.top + (mrzRect.height / (lineCount + 1)) * i;
      canvas.drawLine(
        Offset(mrzRect.left + 12, dy),
        Offset(mrzRect.right - 12, dy),
        linePaint,
      );
    }
  }

  void _drawDetection(Canvas canvas, Size size) {
    final detection = detectedMrz;
    if (detection == null) {
      return;
    }

    final highlightRect = Rect.fromLTRB(
      detection.left * size.width,
      detection.top * size.height,
      detection.right * size.width,
      detection.bottom * size.height,
    );

    final paint = Paint()
      ..color = Colors.greenAccent
      ..style = PaintingStyle.stroke
      ..strokeWidth = 4;

    canvas.drawRRect(
      RRect.fromRectAndRadius(highlightRect.inflate(8), const Radius.circular(24)),
      paint,
    );
  }

  Rect _cardRect(Size size) {
    final aspectRatio = 85.6 / 54.0; // ID-1 ratio
    var cardWidth = size.width * 0.72;
    var cardHeight = cardWidth / aspectRatio;
    final maxHeight = size.height * 0.8;

    if (cardHeight > maxHeight) {
      cardHeight = maxHeight;
      cardWidth = cardHeight * aspectRatio;
    }

    final top = (size.height - cardHeight) * 0.35;
    final left = (size.width - cardWidth) / 2;

    return Rect.fromLTWH(left, top, cardWidth, cardHeight);
  }
}
