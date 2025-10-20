import 'dart:typed_data';

import 'package:nfc_manager/nfc_manager.dart';

import '../models/identity_document.dart';

class NfcService {
  Future<IdentityDocument> readChip({
    required IdentityDocument document,
    Duration timeout = const Duration(seconds: 20),
  }) async {
    final isAvailable = await NfcManager.instance.isAvailable();
    if (!isAvailable) {
      throw const NfcUnavailableException('NFC is not available on this device.');
    }

    IdentityDocument? updatedDocument;

    await NfcManager.instance.startSession(
      alertMessage: 'ضع الهوية الوطنية على ظهر الهاتف.',
      onDiscovered: (NfcTag tag) async {
        final basic = tag.data['nfca'] as Map<String, dynamic>?;
        if (basic == null) {
          throw const NfcReadException('بطاقة الهوية غير مدعومة.');
        }
        final identifier = basic['identifier'] as Uint8List?;
        final techData = basic['data'] as Map<String, dynamic>?;
        final cardInfo = <String, dynamic>{
          'chipSerial': identifier != null ? _bytesToHex(identifier) : 'غير معروف',
          'atqa': techData?['atqa'],
          'sak': techData?['sak'],
        };
        updatedDocument = document.copyWith(chipData: cardInfo);
        await NfcManager.instance.stopSession();
      },
    );

    final result = await Future.any<IdentityDocument?>([
      Future<IdentityDocument?>.delayed(timeout, () => null),
      Future<IdentityDocument?>.value(() async => updatedDocument).then((future) => future),
    ]);

    if (result == null) {
      throw const NfcTimeoutException('انتهى وقت الانتظار دون قراءة البطاقة.');
    }

    return result;
  }

  String _bytesToHex(Uint8List bytes) {
    final buffer = StringBuffer();
    for (final byte in bytes) {
      buffer.write(byte.toRadixString(16).padLeft(2, '0'));
    }
    return buffer.toString().toUpperCase();
  }
}

class NfcUnavailableException implements Exception {
  const NfcUnavailableException(this.message);
  final String message;
}

class NfcReadException implements Exception {
  const NfcReadException(this.message);
  final String message;
}

class NfcTimeoutException implements Exception {
  const NfcTimeoutException(this.message);
  final String message;
}
