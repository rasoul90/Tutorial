enum DocumentType {
  passport,
  nationalId,
}

extension DocumentTypeText on DocumentType {
  String get displayName {
    switch (this) {
      case DocumentType.passport:
        return 'جواز السفر';
      case DocumentType.nationalId:
        return 'البطاقة الوطنية';
    }
  }
}
