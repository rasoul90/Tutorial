import 'package:intl/intl.dart';

class MrzData {
  MrzData({
    required this.lines,
    required this.documentNumber,
    required this.documentNumberCheckDigit,
    required this.nationality,
    required this.issuingState,
    required this.birthDate,
    required this.birthDateRaw,
    required this.birthDateCheckDigit,
    required this.expiryDate,
    required this.expiryDateRaw,
    required this.expiryDateCheckDigit,
    required this.sex,
    required this.primaryIdentifier,
    required this.secondaryIdentifier,
    required this.personalNumber,
    required this.personalNumberCheckDigit,
    required this.compositeCheckDigit,
  });

  final List<String> lines;
  final String documentNumber;
  final String documentNumberCheckDigit;
  final String nationality;
  final String issuingState;
  final DateTime birthDate;
  final String birthDateRaw;
  final String birthDateCheckDigit;
  final DateTime expiryDate;
  final String expiryDateRaw;
  final String expiryDateCheckDigit;
  final String sex;
  final String primaryIdentifier;
  final String secondaryIdentifier;
  final String personalNumber;
  final String personalNumberCheckDigit;
  final String compositeCheckDigit;

  String get fullName => '$primaryIdentifier $secondaryIdentifier'.trim();

  String get formattedBirthDate => _formatDate(birthDate);

  String get formattedExpiryDate => _formatDate(expiryDate);

  static String _formatDate(DateTime date) {
    return DateFormat('yyyy-MM-dd').format(date);
  }

  factory MrzData.fromLines(List<String> rawLines) {
    final cleanedLines = rawLines
        .map((line) => line.replaceAll(' ', '').replaceAll('\u200f', ''))
        .where((line) => line.isNotEmpty)
        .toList(growable: false);

    if (cleanedLines.length == 3 && cleanedLines[0].length <= 36) {
      return _parseTd1(cleanedLines);
    }

    if (cleanedLines.length >= 2) {
      return _parseTd3(cleanedLines.take(2).toList(growable: false));
    }

    throw const FormatException('تنسيق MRZ غير مدعوم أو غير مكتمل');
  }

  static MrzData _parseTd1(List<String> lines) {
    final line1 = _pad(lines[0], 30);
    final line2 = _pad(lines[1], 30);
    final line3 = _pad(lines[2], 30);

    final issuingState = _cleanLetters(line1.substring(2, 5));
    final documentNumber = _cleanLetters(line1.substring(5, 14));
    final documentNumberCheckDigit = line1.substring(14, 15);
    final optional1 = line1.substring(15, 30);

    final birthDateRaw = line2.substring(0, 6);
    final birthDate = _parseDate(birthDateRaw);
    final birthDateCheckDigit = line2.substring(6, 7);
    final sex = _cleanLetters(line2.substring(7, 8)).isEmpty
        ? 'X'
        : _cleanLetters(line2.substring(7, 8));
    final expiryDateRaw = line2.substring(8, 14);
    final expiryDate = _parseDate(expiryDateRaw);
    final expiryDateCheckDigit = line2.substring(14, 15);
    final nationality = _cleanLetters(line2.substring(15, 18));
    final optional2 = line2.substring(18, 29);
    final personalNumber = _cleanLetters(optional1 + optional2);
    final personalNumberCheckDigit = line2.substring(29, 30);
    final compositeCheckDigit = personalNumberCheckDigit;

    final nameParts = _extractNames(line3.substring(0, 30));

    return MrzData(
      lines: List<String>.unmodifiable(lines),
      documentNumber: documentNumber,
      documentNumberCheckDigit: documentNumberCheckDigit,
      nationality: nationality.isEmpty ? issuingState : nationality,
      issuingState: issuingState,
      birthDate: birthDate,
      birthDateRaw: birthDateRaw,
      birthDateCheckDigit: birthDateCheckDigit,
      expiryDate: expiryDate,
      expiryDateRaw: expiryDateRaw,
      expiryDateCheckDigit: expiryDateCheckDigit,
      sex: sex,
      primaryIdentifier: nameParts.$1,
      secondaryIdentifier: nameParts.$2,
      personalNumber: personalNumber,
      personalNumberCheckDigit: personalNumberCheckDigit,
      compositeCheckDigit: compositeCheckDigit,
    );
  }

  static MrzData _parseTd3(List<String> lines) {
    final line1 = _pad(lines[0], 44);
    final line2 = _pad(lines[1], 44);

    final issuingState = _cleanLetters(line1.substring(2, 5));
    final names = _extractNames(line1.substring(5, 44));

    final documentNumber = _cleanLetters(line2.substring(0, 9));
    final documentNumberCheckDigit = line2.substring(9, 10);
    final nationality = _cleanLetters(line2.substring(10, 13));
    final birthDateRaw = line2.substring(13, 19);
    final birthDate = _parseDate(birthDateRaw);
    final birthDateCheckDigit = line2.substring(19, 20);
    final sexRaw = _cleanLetters(line2.substring(20, 21));
    final sex = sexRaw.isEmpty ? 'X' : sexRaw;
    final expiryDateRaw = line2.substring(21, 27);
    final expiryDate = _parseDate(expiryDateRaw);
    final expiryDateCheckDigit = line2.substring(27, 28);
    final personalNumber = _cleanLetters(line2.substring(28, 42));
    final personalNumberCheckDigit = line2.substring(42, 43);
    final compositeCheckDigit = line2.substring(43, 44);

    return MrzData(
      lines: List<String>.unmodifiable(lines),
      documentNumber: documentNumber,
      documentNumberCheckDigit: documentNumberCheckDigit,
      nationality: nationality.isEmpty ? issuingState : nationality,
      issuingState: issuingState,
      birthDate: birthDate,
      birthDateRaw: birthDateRaw,
      birthDateCheckDigit: birthDateCheckDigit,
      expiryDate: expiryDate,
      expiryDateRaw: expiryDateRaw,
      expiryDateCheckDigit: expiryDateCheckDigit,
      sex: sex,
      primaryIdentifier: names.$1,
      secondaryIdentifier: names.$2,
      personalNumber: personalNumber,
      personalNumberCheckDigit: personalNumberCheckDigit,
      compositeCheckDigit: compositeCheckDigit,
    );
  }

  static (String, String) _extractNames(String raw) {
    final cleaned = raw.split('<<');
    final primary = cleaned.isNotEmpty ? _cleanWhitespace(cleaned.first) : '';
    if (cleaned.length <= 1) {
      return (primary, '');
    }
    final secondary = _cleanWhitespace(cleaned.sublist(1).join(' '));
    return (primary, secondary);
  }

  static String _pad(String value, int length) {
    if (value.length >= length) {
      return value;
    }
    return value.padRight(length, '<');
  }

  static String _cleanLetters(String value) {
    return value.replaceAll('<', '').trim();
  }

  static String _cleanWhitespace(String value) {
    return value.replaceAll('<', ' ').replaceAll(RegExp(' +'), ' ').trim();
  }

  static DateTime _parseDate(String raw) {
    final normalized = raw.replaceAll('<', '');
    if (normalized.length != 6) {
      throw FormatException('قيمة تاريخ غير صالحة: $raw');
    }
    final year = int.parse(normalized.substring(0, 2));
    final month = int.parse(normalized.substring(2, 4));
    final day = int.parse(normalized.substring(4, 6));

    final now = DateTime.now();
    final currentYearTwoDigits = now.year % 100;
    final century = year <= currentYearTwoDigits ? 2000 : 1900;

    return DateTime(century + year, month, day);
  }
}
