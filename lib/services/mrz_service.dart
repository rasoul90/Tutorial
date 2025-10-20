import 'package:mrz_parser/mrz_parser.dart';

import '../models/identity_document.dart';

class MrzService {
  IdentityDocument parse(String rawMrz) {
    final lines = rawMrz.split(RegExp(r'\s+')).where((line) => line.isNotEmpty).toList();
    if (lines.length < 2) {
      throw const FormatException('Invalid MRZ: expected at least two lines.');
    }
    final mrz = MRZParser.parse(lines.join('\n'));
    final fullName = _buildNames(mrz);
    return IdentityDocument(
      documentNumber: mrz.documentNumber,
      nationalNumber: mrz.optionalData1 ?? '',
      fullNameArabic: '',
      fullNameEnglish: fullName,
      dateOfBirth: mrz.birthDate,
      gender: mrz.gender,
      expiryDate: mrz.expiryDate,
      placeOfBirth: mrz.personalNumber2 ?? '',
      cardIssuer: mrz.issuingCountry,
      mrz: lines.join('\n'),
    );
  }

  String _buildNames(MRZResult mrz) {
    final components = <String>[
      if (mrz.givenNames?.isNotEmpty ?? false) mrz.givenNames!,
      if (mrz.surname?.isNotEmpty ?? false) mrz.surname!,
    ];
    return components.join(' ').trim();
  }
}
