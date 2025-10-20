class IdentityDocument {
  const IdentityDocument({
    required this.documentNumber,
    required this.nationalNumber,
    required this.fullNameArabic,
    required this.fullNameEnglish,
    required this.dateOfBirth,
    required this.gender,
    required this.expiryDate,
    required this.placeOfBirth,
    required this.cardIssuer,
    required this.mrz,
    this.chipData,
  });

  final String documentNumber;
  final String nationalNumber;
  final String fullNameArabic;
  final String fullNameEnglish;
  final DateTime dateOfBirth;
  final String gender;
  final DateTime expiryDate;
  final String placeOfBirth;
  final String cardIssuer;
  final String mrz;
  final Map<String, dynamic>? chipData;

  IdentityDocument copyWith({
    Map<String, dynamic>? chipData,
  }) {
    return IdentityDocument(
      documentNumber: documentNumber,
      nationalNumber: nationalNumber,
      fullNameArabic: fullNameArabic,
      fullNameEnglish: fullNameEnglish,
      dateOfBirth: dateOfBirth,
      gender: gender,
      expiryDate: expiryDate,
      placeOfBirth: placeOfBirth,
      cardIssuer: cardIssuer,
      mrz: mrz,
      chipData: chipData ?? this.chipData,
    );
  }
}
