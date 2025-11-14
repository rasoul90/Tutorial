import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flutter/material.dart';

class RotatingMrzOverlayV2 extends StatelessWidget {
  const RotatingMrzOverlayV2({
    super.key,
    this.detectedRect,
    this.rotationDegrees = 0,
    this.alignment = Alignment.center,
    this.heightRelative = 0.8,
    this.guideLinesVertical = false,
  });

  final ui.Rect? detectedRect;
  final double rotationDegrees;
  final Alignment alignment;
  final double heightRelative;
  final bool guideLinesVertical;

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: LayoutBuilder(
        builder: (context, constraints) {
          final size = Size(constraints.maxWidth, constraints.maxHeight);
          return CustomPaint(
            size: size,
            painter: _RotatingMrzOverlayPainter(
              detectedRect: detectedRect,
              rotationRadians: rotationDegrees * math.pi / 180,
              alignment: alignment,
              heightRelative: heightRelative,
              guideLinesVertical: guideLinesVertical,
            ),
          );
        },
      ),
    );
  }
}

class _RotatingMrzOverlayPainter extends CustomPainter {
  _RotatingMrzOverlayPainter({
    required this.detectedRect,
    required this.rotationRadians,
    required this.alignment,
    required this.heightRelative,
    required this.guideLinesVertical,
  });

  final ui.Rect? detectedRect;
  final double rotationRadians;
  final Alignment alignment;
  final double heightRelative;
  final bool guideLinesVertical;

  static const double _cardCornerRadius = 24;
  static const double _id1Aspect = 85.6 / 54.0;

  @override
  void paint(Canvas canvas, Size size) {
    final cardRect = _resolveCardRect(size);

    _drawDimmedBackground(canvas, size, cardRect);

    canvas.save();
    if (detectedRect == null) {
      _applyRotation(canvas, cardRect.center);
    }
    _drawCardOutline(canvas, cardRect);
    _drawGuidelines(canvas, cardRect);
    canvas.restore();
  }

  @override
  bool shouldRepaint(covariant _RotatingMrzOverlayPainter oldDelegate) {
    return oldDelegate.detectedRect != detectedRect ||
        oldDelegate.rotationRadians != rotationRadians ||
        oldDelegate.heightRelative != heightRelative ||
        oldDelegate.alignment != alignment ||
        oldDelegate.guideLinesVertical != guideLinesVertical;
  }

  void _drawDimmedBackground(Canvas canvas, Size size, ui.Rect cardRect) {
    final overlayPaint = Paint()..color = Colors.black.withOpacity(0.55);
    final cutoutPaint = Paint()..blendMode = BlendMode.clear;

    canvas.saveLayer(null, Paint());
    canvas.drawRect(Offset.zero & size, overlayPaint);

    canvas.save();
    if (detectedRect == null) {
      _applyRotation(canvas, cardRect.center);
    }
    canvas.drawRRect(
      RRect.fromRectAndRadius(cardRect, const Radius.circular(_cardCornerRadius)),
      cutoutPaint,
    );
    canvas.restore();

    canvas.restore();
  }

  void _drawCardOutline(Canvas canvas, ui.Rect cardRect) {
    final borderColor = detectedRect != null
        ? Colors.greenAccent
        : Colors.white.withOpacity(0.9);
    final borderPaint = Paint()
      ..color = borderColor
      ..style = PaintingStyle.stroke
      ..strokeWidth = detectedRect != null ? 4 : 3;

    final fillPaint = Paint()
      ..color = Colors.white.withOpacity(detectedRect != null ? 0.08 : 0.12);

    canvas.drawRRect(
      RRect.fromRectAndRadius(cardRect, const Radius.circular(_cardCornerRadius)),
      fillPaint,
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(cardRect, const Radius.circular(_cardCornerRadius)),
      borderPaint,
    );

    final mrzBandHeight = cardRect.height * 0.28;
    final mrzBand = Rect.fromLTWH(
      cardRect.left + cardRect.width * 0.08,
      cardRect.bottom - mrzBandHeight - cardRect.width * 0.08,
      cardRect.width * 0.84,
      mrzBandHeight,
    );

    final mrzPaint = Paint()
      ..color = Colors.white.withOpacity(0.18)
      ..style = PaintingStyle.fill;
    canvas.drawRRect(
      RRect.fromRectAndRadius(mrzBand, const Radius.circular(_cardCornerRadius * 0.55)),
      mrzPaint,
    );
  }

  void _drawGuidelines(Canvas canvas, ui.Rect cardRect) {
    final paint = Paint()
      ..color = Colors.white.withOpacity(0.35)
      ..strokeWidth = 1.4;

    if (guideLinesVertical && detectedRect == null) {
      final divisions = 3;
      final step = cardRect.width / divisions;
      for (var i = 1; i < divisions; i++) {
        final dx = cardRect.left + (step * i);
        canvas.drawLine(
          Offset(dx, cardRect.top + cardRect.height * 0.12),
          Offset(dx, cardRect.bottom - cardRect.height * 0.12),
          paint,
        );
      }
    }

    if (detectedRect == null) {
      final headerHeight = cardRect.height * 0.22;
      canvas.drawLine(
        Offset(cardRect.left + 16, cardRect.top + headerHeight),
        Offset(cardRect.right - 16, cardRect.top + headerHeight),
        paint,
      );
    }
  }

  ui.Rect _resolveCardRect(Size size) {
    final detection = detectedRect;
    if (detection != null && !detection.isEmpty) {
      final left = detection.left.clamp(0.0, 1.0) * size.width;
      final top = detection.top.clamp(0.0, 1.0) * size.height;
      final right = detection.right.clamp(0.0, 1.0) * size.width;
      final bottom = detection.bottom.clamp(0.0, 1.0) * size.height;
      return ui.Rect.fromLTRB(left, top, right, bottom);
    }

    final overlayHeight = (size.height * heightRelative).clamp(0.0, size.height);
    double overlayWidth = overlayHeight * _id1Aspect;
    if (overlayWidth > size.width) {
      overlayWidth = size.width;
    }
    final overlaySize = Size(overlayWidth, overlayHeight);
    final baseRect = alignment.inscribe(overlaySize, Offset.zero & size);
    return ui.Rect.fromLTWH(baseRect.left, baseRect.top, baseRect.width, baseRect.height);
  }

  void _applyRotation(Canvas canvas, Offset center) {
    if (rotationRadians == 0) {
      return;
    }
    canvas.translate(center.dx, center.dy);
    canvas.rotate(rotationRadians);
    canvas.translate(-center.dx, -center.dy);
  }
}
