import 'dart:io';
import 'dart:typed_data';
import 'dart:ui' show Image, Rect, Size, decodeImageFromList;

import 'package:google_mlkit_text_recognition/google_mlkit_text_recognition.dart';
import 'package:mrz_parser/mrz_parser.dart';

import '../models/mrz_data.dart';
import '../models/mrz_scan_result.dart';

class MrzScannerService {
  MrzScannerService()
      : _recognizer = TextRecognizer(script: TextRecognitionScript.latin);

  final TextRecognizer _recognizer;

  Future<MrzScanResult> scanImage(File imageFile) async {
    final inputImage = InputImage.fromFile(imageFile);
    final recognized = await _recognizer.processImage(inputImage);

    final mrzExtraction = _extractMrzLines(recognized);
    if (mrzExtraction == null) {
      throw const FormatException('تعذر العثور على MRZ بشكل واضح. حاول مجدداً.');
    }

    final mrzText = mrzExtraction.lines.join('\n');
    final parsed = MRZParser().parse(mrzText);
    final data = MrzData.fromResult(parsed, mrzExtraction.lines);

    final imageBytes = await imageFile.readAsBytes();
    final Image uiImage = await decodeImageFromList(Uint8List.fromList(imageBytes));
    final boundingBox = mrzExtraction.boundingBox;

    return MrzScanResult(
      data: data,
      imageSize: Size(uiImage.width.toDouble(), uiImage.height.toDouble()),
      boundingBox: boundingBox,
    );
  }

  Future<void> dispose() => _recognizer.close();

  _MrzExtraction? _extractMrzLines(RecognizedText recognized) {
    final candidateLines = <_RecognizedLine>[];

    for (final block in recognized.blocks) {
      for (final line in block.lines) {
        final cleaned = line.text
            .replaceAll(' ', '')
            .replaceAll('\u200f', '')
            .replaceAll('\u202d', '')
            .replaceAll('\u202c', '')
            .toUpperCase();
        if (cleaned.isEmpty) {
          continue;
        }
        if (!RegExp(r'^[A-Z0-9<]+$').hasMatch(cleaned)) {
          continue;
        }
        candidateLines.add(
          _RecognizedLine(
            text: cleaned,
            boundingBox: line.boundingBox,
          ),
        );
      }
    }

    if (candidateLines.length < 2) {
      return null;
    }

    _RecognizedLine padLine(_RecognizedLine line, int targetLength) {
      final padded = line.text.padRight(targetLength, '<').substring(0, targetLength);
      return _RecognizedLine(text: padded, boundingBox: line.boundingBox);
    }

    if (candidateLines.length >= 3) {
      for (var i = 0; i <= candidateLines.length - 3; i++) {
        final slice = candidateLines.sublist(i, i + 3);
        if (slice.every((line) => line.text.length >= 30 && line.text.length <= 36)) {
          final normalized = slice.map((line) => padLine(line, 30)).toList();
          return _MrzExtraction(
            lines: normalized.map((line) => line.text).toList(),
            boundingBox: _mergeBoundingBoxes(normalized.map((e) => e.boundingBox)),
          );
        }
      }
    }

    for (var i = 0; i <= candidateLines.length - 2; i++) {
      final slice = candidateLines.sublist(i, i + 2);
      if (slice.every((line) => line.text.length >= 36)) {
        final normalized = slice.map((line) => padLine(line, 44)).toList();
        return _MrzExtraction(
          lines: normalized.map((line) => line.text).toList(),
          boundingBox: _mergeBoundingBoxes(normalized.map((e) => e.boundingBox)),
        );
      }
    }

    return null;
  }

  Rect _mergeBoundingBoxes(Iterable<Rect> boxes) {
    Rect? merged;
    for (final box in boxes) {
      if (merged == null) {
        merged = box;
      } else {
        merged = merged.expandToInclude(box);
      }
    }
    return merged ?? Rect.zero;
  }
}

class _RecognizedLine {
  _RecognizedLine({required this.text, required this.boundingBox});

  final String text;
  final Rect boundingBox;
}

class _MrzExtraction {
  _MrzExtraction({required this.lines, required this.boundingBox});

  final List<String> lines;
  final Rect boundingBox;
}
