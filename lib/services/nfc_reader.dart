import 'dart:math';
import 'dart:typed_data';

import 'package:flutter_nfc_kit/flutter_nfc_kit.dart';
import 'package:pointycastle/api.dart' as pc;
import 'package:pointycastle/block/cbc.dart';
import 'package:pointycastle/block/desede.dart';
import 'package:pointycastle/digests/sha1.dart';
import 'package:pointycastle/macs/cbc_block_cipher_mac.dart';
import 'package:pointycastle/paddings/iso7816d4.dart';

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
    final tag = await FlutterNfcKit.poll(
      timeout: const Duration(seconds: 30),
      androidMultipleTagMessage: 'يرجى تقريب البطاقة إلى ظهر الهاتف',
      iosMultipleTagMessage: 'ضع البطاقة على ظهر الهاتف',
    );

    try {
      onProgress(0.05);
      final session = await _BacSession.connect(tag, mrzData);
      onProgress(0.2);

      final sm = session.secureMessaging;

      final efCom = await sm.readEfCom();
      final cardAccess = await sm.readCardAccess();
      onProgress(0.35);

      final availableGroups = efCom.dataGroups;

      final dg1 = await sm.readDataGroup(0x0101);
      onProgress(0.45);

      Uint8List? dg2;
      if (availableGroups.contains(2)) {
        dg2 = await sm.readDataGroup(0x0102);
      }
      onProgress(0.6);

      Uint8List? dg11;
      if (availableGroups.contains(11)) {
        dg11 = await sm.readDataGroup(0x010B);
      }

      Uint8List? dg12;
      if (availableGroups.contains(12)) {
        dg12 = await sm.readDataGroup(0x010C);
      }

      Uint8List? dg13;
      if (availableGroups.contains(13)) {
        dg13 = await sm.readDataGroup(0x010D);
      }

      Uint8List? dg14;
      if (availableGroups.contains(14)) {
        dg14 = await sm.readDataGroup(0x010E);
      }
      onProgress(0.8);

      final parser = _DataGroupParser(
        mrzSource: dg1,
        dg11: dg11,
        dg12: dg12,
        dg13: dg13,
        dg14: dg14,
      );

      final personalInfo = parser.parsePersonalInfo(mrzData);
      final documentInfo = parser.parseDocumentInfo(mrzData);
      final parsedMrz = parser.parseMrzLines();
      final mrzLinesFromChip = parsedMrz.isEmpty ? mrzData.lines : parsedMrz;
      final chipInfo = efCom.toChipInfo(
        accessControl: session.accessType,
        tag: tag.type,
        dg14: dg14,
        cardAccess: cardAccess,
      );

      onProgress(0.9);

      Uint8List? portraitBytes;
      if (dg2 != null && dg2.isNotEmpty) {
        portraitBytes = parser.extractPortrait(dg2);
      }

      onProgress(1.0);

      return NfcReadResult(
        personalInfo: personalInfo,
        documentInfo: documentInfo,
        mrzLinesFromChip: mrzLinesFromChip,
        chipInfo: chipInfo,
        portrait: portraitBytes,
      );
    } finally {
      await FlutterNfcKit.finish(
        iosAlertMessage: 'انتهت عملية القراءة',
      );
    }
  }
}

class _BacSession {
  _BacSession(this.secureMessaging, this.accessType);

  final _SecureMessaging secureMessaging;
  final String accessType;

  static Future<_BacSession> connect(NFCTag tag, MrzData mrzData) async {
    final selector = _ApduTransport();
    await selector.selectPassportApplet();

    try {
      final bac = await _BacAuthenticator.perform(
        transport: selector,
        mrzData: mrzData,
      );
      return _BacSession(bac, 'BAC (MRZ)');
    } on _ApduException {
      rethrow;
    }
  }
}

class _ApduTransport {
  Future<void> selectPassportApplet() async {
    final response = await transceive(
      _hexToBytes('00A4040C07A0000002471001'),
    );
    final rapdu = _Rapdu.fromBytes(response);
    _ensureStatus(rapdu, 0x9000);
  }

  Future<Uint8List> transceive(Uint8List apdu) async {
    final responseHex = await FlutterNfcKit.transceive(_bytesToHex(apdu));
    return _hexToBytes(responseHex);
  }
}

class _BacAuthenticator {
  static Future<_SecureMessaging> perform({
    required _ApduTransport transport,
    required MrzData mrzData,
  }) async {
    final mrzInfo = _composeMrzInfo(mrzData);
    final sha = SHA1Digest();
    final kSeed = Uint8List(sha.digestSize);
    sha.update(mrzInfo, 0, mrzInfo.length);
    sha.doFinal(kSeed, 0);

    final cEnc = Uint8List.fromList([0x00, 0x00, 0x00, 0x01]);
    final cMac = Uint8List.fromList([0x00, 0x00, 0x00, 0x02]);

    final kEnc = _deriveKey(kSeed.sublist(0, 16), cEnc);
    final kMac = _deriveKey(kSeed.sublist(0, 16), cMac);

    final rndIccResponse = await transport.transceive(
      _hexToBytes('0084000008'),
    );
    final rapdu = _Rapdu.fromBytes(rndIccResponse);
    _ensureStatus(rapdu, 0x9000);
    final rndIcc = rapdu.data;
    if (rndIcc.length != 8) {
      throw const _ApduException('استجابة GET CHALLENGE غير صالحة');
    }

    final rndIfd = _randomBytes(8);
    final kIfd = _randomBytes(16);

    final toEncrypt = Uint8List.fromList([...rndIfd, ...rndIcc, ...kIfd]);
    final encData = _desEncrypt(toEncrypt, kEnc, Uint8List(8));
    final mac = _computeMac(kMac, Uint8List.fromList(encData));

    final commandData = Uint8List.fromList([...encData, ...mac]);
    final command = BytesBuilder()
      ..add([0x00, 0x82, 0x00, 0x00, commandData.length])
      ..add(commandData);

    final mutualResponse = await transport.transceive(command.toBytes());
    final mutualRapdu = _Rapdu.fromBytes(mutualResponse);
    _ensureStatus(mutualRapdu, 0x9000);

    final responseData = mutualRapdu.data;
    if (responseData.length != 32 + 8) {
      throw const _ApduException('استجابة المصادقة المتبادلة غير متوقعة');
    }

    final encResp = responseData.sublist(0, 32);
    final respMac = responseData.sublist(32);

    final calculatedMac = _computeMac(kMac, encResp);
    if (!_bytesEqual(calculatedMac, respMac)) {
      throw const _ApduException('فشل التحقق من سلامة الاستجابة');
    }

    final decrypted = _desDecrypt(encResp, kEnc, Uint8List(8));
    final rndIccPrime = decrypted.sublist(0, 8);
    final rndIfdPrime = decrypted.sublist(8, 16);
    final kIcc = decrypted.sublist(16, 32);

    if (!_bytesEqual(rndIfdPrime, rndIfd)) {
      throw const _ApduException('فشل التحقق من التحدي العشوائي');
    }

    final ssc = Uint8List.fromList([
      ...rndIccPrime.sublist(4, 8),
      ...rndIfd.sublist(4, 8),
    ]);

    final secure = _SecureMessaging(
      transport: transport,
      kEnc: kEnc,
      kMac: kMac,
      ssc: ssc,
    );

    return secure;
  }
}

class _SecureMessaging {
  _SecureMessaging({
    required _ApduTransport transport,
    required Uint8List kEnc,
    required Uint8List kMac,
    required Uint8List ssc,
  })  : _transport = transport,
        _kEnc = kEnc,
        _kMac = kMac,
        _ssc = Uint8List.fromList(ssc);

  final _ApduTransport _transport;
  final Uint8List _kEnc;
  final Uint8List _kMac;
  final Uint8List _ssc;

  Future<_Rapdu> _transmitProtected({
    required int ins,
    required int p1,
    required int p2,
    Uint8List? data,
    int? le,
  }) async {
    _incrementSsc(_ssc);

    final do87 = data != null && data.isNotEmpty ? _buildDo87(data, _kEnc, _ssc) : Uint8List(0);
    final do97 = le != null ? _encodeTlv(0x97, Uint8List.fromList([le])) : Uint8List(0);

    final m = BytesBuilder()
      ..add(_ssc)
      ..add([0x0C, ins, p1, p2])
      ..add(do87)
      ..add(do97);

    final mac = _computeMac(_kMac, m.toBytes());
    final do8e = _encodeTlv(0x8E, mac);

    final dataField = Uint8List.fromList([...do87, ...do97, ...do8e]);

    final header = [0x0C, ins, p1, p2];
    final apdu = BytesBuilder()..add(header);
    if (dataField.isNotEmpty) {
      if (dataField.length > 0xFF) {
        apdu.add([0x00]);
        apdu.add([(dataField.length >> 8) & 0xFF, dataField.length & 0xFF]);
      } else {
        apdu.add([dataField.length]);
      }
      apdu.add(dataField);
    } else {
      apdu.add([0x00]);
    }

    final response = await _transport.transceive(apdu.toBytes());
    final rapdu = _Rapdu.fromBytes(response);
    if (rapdu.sw == 0x6982 || rapdu.sw == 0x6985) {
      throw const _ApduException('تم رفض الأمر بواسطة الشريحة');
    }

    _incrementSsc(_ssc);

    final decrypted = _unprotectResponse(
      rapdu.data,
      rapdu.sw,
      _kEnc,
      _kMac,
      _ssc,
    );

    return decrypted;
  }

  Future<_EfCom> readEfCom() async {
    await _transmitProtected(
      ins: 0xA4,
      p1: 0x02,
      p2: 0x0C,
      data: Uint8List.fromList([0x01, 0x1E]),
    );
    final data = await readBinary();
    return _EfCom.parse(data);
  }

  Future<Uint8List> readDataGroup(int fileId) async {
    await _transmitProtected(
      ins: 0xA4,
      p1: 0x02,
      p2: 0x0C,
      data: Uint8List.fromList([(fileId >> 8) & 0xFF, fileId & 0xFF]),
    );
    return readBinary();
  }

  Future<Uint8List?> readCardAccess() async {
    try {
      await _transmitProtected(
        ins: 0xA4,
        p1: 0x02,
        p2: 0x0C,
        data: Uint8List.fromList([0x01, 0x1C]),
      );
      return readBinary();
    } on _ApduException {
      return null;
    }
  }

  Future<Uint8List> readBinary() async {
    final buffer = BytesBuilder();
    var offset = 0;
    while (true) {
      final rapdu = await _transmitProtected(
        ins: 0xB0,
        p1: (offset >> 8) & 0xFF,
        p2: offset & 0xFF,
        le: 0x00,
      );
      if (rapdu.sw != 0x9000) {
        throw _ApduException('استجابة قراءة غير متوقعة: ${rapdu.sw.toRadixString(16)}');
      }
      buffer.add(rapdu.data);
      offset += rapdu.data.length;
      if (rapdu.data.length < 0xFF) {
        break;
      }
    }
    return buffer.toBytes();
  }
}

class _EfCom {
  _EfCom({
    required this.ldsVersion,
    required this.unicodeVersion,
    required this.dataGroups,
  });

  final String ldsVersion;
  final String unicodeVersion;
  final List<int> dataGroups;

  static _EfCom parse(Uint8List data) {
    final tlvs = _BerTlv.decode(data);
    final outer = tlvs.firstWhere((tlv) => tlv.tag == 0x60);
    final children = _BerTlv.decode(outer.value);
    String ldsVersion = 'غير محدد';
    String unicodeVersion = '';
    final dataGroups = <int>[];

    for (final tlv in children) {
      switch (tlv.tag) {
        case 0x5F01:
          ldsVersion = String.fromCharCodes(tlv.value);
          break;
        case 0x5F36:
          unicodeVersion = String.fromCharCodes(tlv.value);
          break;
        case 0x5C:
          for (var i = 0; i < tlv.value.length; i += 2) {
            dataGroups.add(tlv.value[i + 1]);
          }
          break;
      }
    }
    return _EfCom(
      ldsVersion: ldsVersion,
      unicodeVersion: unicodeVersion,
      dataGroups: dataGroups,
    );
  }

  ChipInfo toChipInfo({
    required String accessControl,
    required String tag,
    Uint8List? dg14,
    Uint8List? cardAccess,
  }) {
    final features = <String>{};
    if (dg14 != null && dg14.isNotEmpty) {
      features.add('EAC');
    }
    if (cardAccess != null) {
      final securityInfos = _BerTlv.decode(cardAccess);
      final paceInfo = securityInfos.where((tlv) => tlv.tag == 0x7F4C);
      for (final info in paceInfo) {
        final nested = _BerTlv.decode(info.value);
        if (nested.any((element) => element.tag == 0xA1)) {
          features.add('PACE');
        }
      }
    }
    features.add(accessControl);
    return ChipInfo(
      ldsVersion: ldsVersion,
      dataGroups: dataGroups,
      features: features.toList(),
      accessControl: accessControl,
      tagTech: tag,
      isoStandard: 'ISO 14443-4 (Type A)',
    );
  }
}

class _DataGroupParser {
  _DataGroupParser({
    required Uint8List? mrzSource,
    required this.dg11,
    required this.dg12,
    required this.dg13,
    required this.dg14,
  }) : mrzSource = mrzSource ?? Uint8List(0);

  final Uint8List mrzSource;
  final Uint8List? dg11;
  final Uint8List? dg12;
  final Uint8List? dg13;
  final Uint8List? dg14;

  List<String> parseMrzLines() {
    if (mrzSource.isEmpty) {
      return const [];
    }
    final mrzText = String.fromCharCodes(mrzSource).trim();
    final lines = mrzText.split(RegExp(r'\r?\n'));
    return lines.where((line) => line.isNotEmpty).toList();
  }

  PersonalInfo parsePersonalInfo(MrzData mrzData) {
    final birthPlace = _extractString(dg11, 0x5F11);
    final otherNames = _extractString(dg11, 0x5F0F);
    final personalNumber = mrzData.personalNumber.isEmpty
        ? _extractString(dg11, 0x5F10)
        : mrzData.personalNumber;

    return PersonalInfo(
      fullName: mrzData.fullName,
      primaryName: mrzData.primaryIdentifier,
      secondaryName: mrzData.secondaryIdentifier,
      gender: mrzData.sex,
      birthDate: mrzData.birthDate,
      birthPlace: birthPlace,
      nationality: mrzData.nationality,
      personalNumber: personalNumber?.isEmpty ?? true ? null : personalNumber,
    );
  }

  DocumentInfo parseDocumentInfo(MrzData mrzData) {
    final issueDateString = _extractString(dg12, 0x5F26);
    DateTime? issueDate;
    if (issueDateString != null && issueDateString.length >= 8) {
      issueDate = DateTime.parse(issueDateString.substring(0, 8));
    }

    return DocumentInfo(
      documentNumber: mrzData.documentNumber,
      issuingState: mrzData.issuingState,
      expiryDate: mrzData.expiryDate,
      issueDate: issueDate,
      documentType: mrzData.lines.length == 3 ? 'TD1' : 'TD3',
    );
  }

  Uint8List? extractPortrait(Uint8List dg2) {
    return _findValueRecursive(_BerTlv.decode(dg2), const {
      0x5F2E,
      0x5F40,
      0x7F60,
    });
  }

  String? _extractString(Uint8List? source, int tag) {
    if (source == null) {
      return null;
    }
    final value = _findValueRecursive(_BerTlv.decode(source), {tag});
    return value == null ? null : String.fromCharCodes(value).trim();
  }

  Uint8List? _findValueRecursive(List<_BerTlv> tlvs, Set<int> tags) {
    for (final tlv in tlvs) {
      if (tags.contains(tlv.tag)) {
        return tlv.value;
      }
      if ((tlv.tag & 0x20) == 0x20) {
        final nested = _BerTlv.decode(tlv.value);
        final found = _findValueRecursive(nested, tags);
        if (found != null) {
          return found;
        }
      }
    }
    return null;
  }
}

class _Rapdu {
  _Rapdu({required this.data, required this.sw});

  final Uint8List data;
  final int sw;

  factory _Rapdu.fromBytes(Uint8List response) {
    if (response.length < 2) {
      throw const _ApduException('استجابة غير مكتملة من الشريحة');
    }
    final sw1 = response[response.length - 2];
    final sw2 = response[response.length - 1];
    final sw = (sw1 << 8) | sw2;
    final data = response.sublist(0, response.length - 2);
    return _Rapdu(data: data, sw: sw);
  }
}

class _ApduException implements Exception {
  const _ApduException(this.message);
  final String message;
  @override
  String toString() => message;
}

void _ensureStatus(_Rapdu rapdu, int expectedSw) {
  if (rapdu.sw != expectedSw) {
    throw _ApduException('استجابة غير متوقعة: ${rapdu.sw.toRadixString(16)}');
  }
}

Uint8List _buildDo87(Uint8List data, Uint8List kEnc, Uint8List ssc) {
  final padded = _iso7816Padding(data);
  final enc = _desEncrypt(padded, kEnc, Uint8List.fromList(ssc));
  final value = Uint8List.fromList([0x01, ...enc]);
  return _encodeTlv(0x87, value);
}

_Rapdu _unprotectResponse(
  Uint8List responseData,
  int sw,
  Uint8List kEnc,
  Uint8List kMac,
  Uint8List ssc,
) {
  Uint8List do87 = Uint8List(0);
  Uint8List do99 = Uint8List(0);
  Uint8List do8e = Uint8List(0);

  var offset = 0;
  while (offset < responseData.length) {
    final tag = responseData[offset++];
    var length = responseData[offset++];
    if (length == 0x81) {
      length = responseData[offset++];
    }
    final value = responseData.sublist(offset, offset + length);
    offset += length;

    switch (tag) {
      case 0x87:
        do87 = value;
        break;
      case 0x99:
        do99 = value;
        break;
      case 0x8E:
        do8e = value;
        break;
    }
  }

  if (do99.isEmpty) {
    do99 = Uint8List.fromList([(sw >> 8) & 0xFF, sw & 0xFF]);
  }

  final macSource = BytesBuilder()
    ..add(ssc)
    ..add(do87.isNotEmpty ? _encodeTlv(0x87, do87) : Uint8List(0))
    ..add(_encodeTlv(0x99, do99));
  final expectedMac = _computeMac(kMac, macSource.toBytes());

  if (do8e.isEmpty || !_bytesEqual(expectedMac, do8e)) {
    throw const _ApduException('فشل التحقق من سلامة الاستجابة المحمية');
  }

  Uint8List plain = Uint8List(0);
  if (do87.isNotEmpty) {
    final encrypted = do87.sublist(1);
    plain = _desDecrypt(encrypted, kEnc, Uint8List.fromList(ssc));
    plain = _removeIso7816Padding(plain);
  }

  if (do99.isEmpty) {
    do99 = Uint8List.fromList([(sw >> 8) & 0xFF, sw & 0xFF]);
  }

  final swFromDo99 = (do99[0] << 8) | do99[1];
  return _Rapdu(data: plain, sw: swFromDo99);
}

Uint8List _desEncrypt(Uint8List input, Uint8List key, Uint8List iv) {
  final cipher = CBCBlockCipher(DESedeEngine())
      ..init(
        true,
        pc.ParametersWithIV(pc.KeyParameter(key), iv),
      );
  final blockSize = cipher.blockSize;
  final output = Uint8List(input.length);
  for (var offset = 0; offset < input.length; offset += blockSize) {
    cipher.processBlock(input, offset, output, offset);
  }
  return output;
}

Uint8List _desDecrypt(Uint8List input, Uint8List key, Uint8List iv) {
  final cipher = CBCBlockCipher(DESedeEngine())
      ..init(
        false,
        pc.ParametersWithIV(pc.KeyParameter(key), iv),
      );
  final blockSize = cipher.blockSize;
  final output = Uint8List(input.length);
  for (var offset = 0; offset < input.length; offset += blockSize) {
    cipher.processBlock(input, offset, output, offset);
  }
  return output;
}

Uint8List _computeMac(Uint8List key, Uint8List message) {
  final mac = CBCBlockCipherMac(
    DESedeEngine(),
    blockSize: 8,
    padding: Iso7816d4Padding(),
  );
  mac.init(pc.KeyParameter(key));
  mac.update(message, 0, message.length);
  final out = Uint8List(mac.macSize);
  mac.doFinal(out, 0);
  return out;
}

Uint8List _deriveKey(Uint8List seed, Uint8List c) {
  final sha = SHA1Digest();
  final input = Uint8List.fromList([...seed, ...c]);
  final hash = Uint8List(sha.digestSize);
  sha.update(input, 0, input.length);
  sha.doFinal(hash, 0);
  return _adjustParity(hash.sublist(0, 16));
}

Uint8List _adjustParity(Uint8List key) {
  final adjusted = Uint8List.fromList(key);
  for (var i = 0; i < adjusted.length; i++) {
    var b = adjusted[i];
    var parity = 0;
    for (var bit = 1; bit < 8; bit++) {
      parity ^= (b >> bit) & 0x01;
    }
    b = (b & 0xFE) | (parity ^ 0x01);
    adjusted[i] = b;
  }
  return adjusted;
}

Uint8List _composeMrzInfo(MrzData data) {
  final document = data.documentNumber.padRight(9, '<');
  final personal = data.personalNumber.padRight(14, '<');
  final mrzInformation = StringBuffer()
    ..write(document)
    ..write(data.documentNumberCheckDigit)
    ..write(data.birthDateRaw)
    ..write(data.birthDateCheckDigit)
    ..write(data.expiryDateRaw)
    ..write(data.expiryDateCheckDigit)
    ..write(personal)
    ..write(data.personalNumberCheckDigit);
  final mrzString = mrzInformation.toString();
  return Uint8List.fromList(mrzString.codeUnits);
}

Uint8List _randomBytes(int length) {
  final random = Random.secure();
  return Uint8List.fromList(List<int>.generate(length, (_) => random.nextInt(256)));
}

Uint8List _iso7816Padding(Uint8List data) {
  final blockSize = 8;
  final padLength = blockSize - (data.length % blockSize);
  final padded = Uint8List(data.length + padLength);
  padded.setAll(0, data);
  padded[data.length] = 0x80;
  return padded;
}

Uint8List _removeIso7816Padding(Uint8List data) {
  var index = data.length - 1;
  while (index >= 0 && data[index] == 0x00) {
    index--;
  }
  if (index >= 0 && data[index] == 0x80) {
    return data.sublist(0, index);
  }
  return data;
}

void _incrementSsc(Uint8List ssc) {
  for (var i = ssc.length - 1; i >= 0; i--) {
    ssc[i] = (ssc[i] + 1) & 0xFF;
    if (ssc[i] != 0) {
      break;
    }
  }
}

bool _bytesEqual(Uint8List a, Uint8List b) {
  if (a.length != b.length) return false;
  for (var i = 0; i < a.length; i++) {
    if (a[i] != b[i]) {
      return false;
    }
  }
  return true;
}

Uint8List _hexToBytes(String hex) {
  final cleanHex = hex.replaceAll(' ', '');
  final length = cleanHex.length;
  final bytes = Uint8List(length ~/ 2);
  for (var i = 0; i < length; i += 2) {
    bytes[i ~/ 2] = int.parse(cleanHex.substring(i, i + 2), radix: 16);
  }
  return bytes;
}

String _bytesToHex(Uint8List bytes) {
  final buffer = StringBuffer();
  for (final byte in bytes) {
    buffer.write(byte.toRadixString(16).padLeft(2, '0').toUpperCase());
  }
  return buffer.toString();
}

Uint8List _encodeTlv(int tag, Uint8List value) {
  final tagBytes = <int>[];
  if (tag > 0xFF) {
    tagBytes.add((tag >> 8) & 0xFF);
    tagBytes.add(tag & 0xFF);
  } else {
    tagBytes.add(tag & 0xFF);
  }
  final lengthBytes = _encodeLength(value.length);
  return Uint8List.fromList([...tagBytes, ...lengthBytes, ...value]);
}

Uint8List _encodeLength(int length) {
  if (length <= 0x7F) {
    return Uint8List.fromList([length]);
  }
  final bytes = <int>[];
  var temp = length;
  while (temp > 0) {
    bytes.insert(0, temp & 0xFF);
    temp >>= 8;
  }
  return Uint8List.fromList([0x80 | bytes.length, ...bytes]);
}

class _BerTlv {
  _BerTlv({required this.tag, required this.value});

  final int tag;
  final Uint8List value;

  static List<_BerTlv> decode(Uint8List input) {
    final result = <_BerTlv>[];
    var offset = 0;
    while (offset < input.length) {
      var tag = input[offset++];
      if ((tag & 0x1F) == 0x1F) {
        tag = (tag << 8) | input[offset++];
      }
      var length = input[offset++];
      if (length > 0x80) {
        final lengthBytes = length - 0x80;
        length = 0;
        for (var i = 0; i < lengthBytes; i++) {
          length = (length << 8) | input[offset++];
        }
      }
      final value = input.sublist(offset, offset + length);
      offset += length;
      result.add(_BerTlv(tag: tag, value: value));
    }
    return result;
  }
}
