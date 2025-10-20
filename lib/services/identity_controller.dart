import 'package:flutter/foundation.dart';

import '../models/identity_document.dart';
import 'mrz_service.dart';
import 'nfc_service.dart';

class IdentityController extends ChangeNotifier {
  IdentityController({
    MrzService? mrzService,
    NfcService? nfcService,
  })  : _mrzService = mrzService ?? MrzService(),
        _nfcService = nfcService ?? NfcService();

  final MrzService _mrzService;
  final NfcService _nfcService;

  IdentityDocument? _document;
  bool _isLoading = false;
  String? _error;

  IdentityDocument? get document => _document;
  bool get isLoading => _isLoading;
  String? get error => _error;

  void reset() {
    _document = null;
    _error = null;
    notifyListeners();
  }

  Future<void> parseMrz(String rawMrz) async {
    _setLoading(true);
    try {
      _document = _mrzService.parse(rawMrz);
      _error = null;
    } on FormatException catch (error) {
      _error = error.message;
      _document = null;
    } catch (error) {
      _error = error.toString();
      _document = null;
    } finally {
      _setLoading(false);
    }
  }

  Future<void> readNfc() async {
    if (_document == null) {
      _error = 'قم بقراءة ال MRZ اولاً.';
      notifyListeners();
      return;
    }

    _setLoading(true);
    try {
      _document = await _nfcService.readChip(document: _document!);
      _error = null;
    } on NfcUnavailableException catch (error) {
      _error = error.message;
    } on NfcTimeoutException catch (error) {
      _error = error.message;
    } on NfcReadException catch (error) {
      _error = error.message;
    } catch (error) {
      _error = error.toString();
    } finally {
      _setLoading(false);
    }
  }

  void _setLoading(bool value) {
    _isLoading = value;
    notifyListeners();
  }
}
