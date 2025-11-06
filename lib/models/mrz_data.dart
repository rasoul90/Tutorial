import 'package:intl/intl.dart';
import 'package:mrz_parser/mrz_parser.dart';

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

  factory MrzData.fromResult(MRZResult result, List<String> lines) {
    DateTime parseDate(String value) {
      final parsed = result.toDate(value);
      if (parsed == null) {
        throw FormatException('لا يمكن تحليل التاريخ: $value');
      }
      return parsed;
    }

    return MrzData(
      lines: lines,
      documentNumber: result.documentNumber,
      documentNumberCheckDigit: result.documentNumberCheckDigit,
      nationality: result.nationality,
      issuingState: result.countryCode,
      birthDate: parseDate(result.birthDate),
      birthDateRaw: result.birthDate,
      birthDateCheckDigit: result.birthDateCheckDigit,
      expiryDate: parseDate(result.expiryDate),
      expiryDateRaw: result.expiryDate,
      expiryDateCheckDigit: result.expiryDateCheckDigit,
      sex: result.sex,
      primaryIdentifier: result.surnames,
      secondaryIdentifier: result.givenNames,
      personalNumber: result.personalNumber,
      personalNumberCheckDigit: result.personalNumberCheckDigit,
      compositeCheckDigit: result.compositeCheckDigit,
    );
  }
}
