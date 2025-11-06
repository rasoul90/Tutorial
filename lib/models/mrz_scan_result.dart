import 'dart:ui';

import 'mrz_data.dart';

class MrzScanResult {
  MrzScanResult({
    required this.data,
    required this.imageSize,
    required this.boundingBox,
  });

  final MrzData data;
  final Size imageSize;
  final Rect boundingBox;
}
