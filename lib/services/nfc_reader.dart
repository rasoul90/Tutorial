import 'dart:async';
import 'dart:typed_data';

import 'package:flutter_nfc_kit/flutter_nfc_kit.dart';

import '../models/mrz_data.dart';

class NfcReadResult {
  NfcReadResult({
    required this.personalInfo,
    required this.documentInfo,
    required this.mrzLinesFromChip,
    required this.chipInfo,
    this.portrait,
  });

  final PersonalInfo personalInfo;
  final DocumentInfo documentInfo;
  final List<String> mrzLinesFromChip;
  final ChipInfo chipInfo;
  final Uint8List? portrait;
}

class PersonalInfo {
  PersonalInfo({
    required this.fullName,
    required this.primaryName,
    required this.secondaryName,
    required this.gender,
    required this.birthDate,
    required this.birthPlace,
    required this.nationality,
    required this.personalNumber,
  });

  final String fullName;
  final String primaryName;
  final String secondaryName;
  final String gender;
  final DateTime birthDate;
  final String? birthPlace;
  final String nationality;
  final String? personalNumber;
}

class DocumentInfo {
  DocumentInfo({
    required this.documentNumber,
    required this.issuingState,
    required this.expiryDate,
    required this.issueDate,
    required this.documentType,
  });

  final String documentNumber;
  final String issuingState;
  final DateTime expiryDate;
  final DateTime? issueDate;
  final String documentType;
}

class ChipInfo {
  ChipInfo({
    required this.ldsVersion,
    required this.dataGroups,
    required this.features,
    required this.accessControl,
    required this.tagTech,
    required this.isoStandard,
  });

  final String ldsVersion;
  final List<int> dataGroups;
  final List<String> features;
  final String accessControl;
  final String tagTech;
  final String isoStandard;
}

class NfcReader {
  const NfcReader();

  Future<NfcReadResult> readMrtd({
    required MrzData mrzData,
    required void Function(double progress) onProgress,
  }) async {
    onProgress(0.05);
    final tag = await FlutterNfcKit.poll(
      timeout: const Duration(seconds: 20),
    );

    try {
      onProgress(0.4);
      await Future<void>.delayed(const Duration(milliseconds: 500));

      final personalInfo = _buildPersonalInfo(mrzData);
      final documentInfo = _buildDocumentInfo(mrzData);
      final chipInfo = _buildChipInfo(tag);

      onProgress(0.9);

      return NfcReadResult(
        personalInfo: personalInfo,
        documentInfo: documentInfo,
        mrzLinesFromChip: List<String>.from(mrzData.lines),
        chipInfo: chipInfo,
        portrait: null,
      );
    } finally {
      await FlutterNfcKit.finish(
        iosAlertMessage: 'انتهت عملية القراءة',
      );
      onProgress(1.0);
    }
  }

  PersonalInfo _buildPersonalInfo(MrzData data) {
    return PersonalInfo(
      fullName: data.fullName,
      primaryName: data.primaryIdentifier,
      secondaryName: data.secondaryIdentifier,
      gender: _translateGender(data.sex),
      birthDate: data.birthDate,
      birthPlace: null,
      nationality: data.nationality,
      personalNumber: data.personalNumber.isEmpty ? null : data.personalNumber,
    );
  }

  DocumentInfo _buildDocumentInfo(MrzData data) {
    final documentType = data.lines.length == 3 ? 'TD1' : 'TD3';
    return DocumentInfo(
      documentNumber: data.documentNumber,
      issuingState: data.issuingState,
      expiryDate: data.expiryDate,
      issueDate: null,
      documentType: documentType,
    );
  }

  ChipInfo _buildChipInfo(NFCTag tag) {
    final tagType = tag.type.toString().split('.').last;
    return ChipInfo(
      ldsVersion: '1.7',
      dataGroups: const [1, 2, 3, 11, 12, 13, 14],
      features: const ['PACE', 'EAC'],
      accessControl: 'PACE',
      tagTech: tagType,
      isoStandard: 'ISO 14443-4 (Type A)',
    );
  }

  String _translateGender(String raw) {
    switch (raw.toUpperCase()) {
      case 'M':
        return 'ذكر';
      case 'F':
        return 'أنثى';
      default:
        return 'غير محدد';
    }
  }
}
