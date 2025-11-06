import 'package:flutter/material.dart';

class MrzCameraGuides extends StatelessWidget {
  const MrzCameraGuides({super.key});

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: CustomPaint(
        painter: _MrzGuidePainter(
          color: Colors.white.withOpacity(0.9),
        ),
      ),
    );
  }
}

class _MrzGuidePainter extends CustomPainter {
  _MrzGuidePainter({required this.color});

  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = 3;

    final width = size.width * 0.9;
    final height = size.height * 0.25;
    final left = (size.width - width) / 2;
    final top = (size.height - height) * 0.7;
    final rect = Rect.fromLTWH(left, top, width, height);

    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(16));
    canvas.drawRRect(rrect, paint);
  }

  @override
  bool shouldRepaint(covariant _MrzGuidePainter oldDelegate) => false;
}
