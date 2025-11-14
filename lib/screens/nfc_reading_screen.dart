import 'package:flutter/material.dart';

import '../models/document_type.dart';
import '../models/mrz_data.dart';
import '../services/nfc_reader.dart';
import 'result_screen.dart';

class NfcReadingScreen extends StatefulWidget {
  const NfcReadingScreen({
    super.key,
    required this.documentType,
    required this.mrzData,
  });

  final DocumentType documentType;
  final MrzData mrzData;

  @override
  State<NfcReadingScreen> createState() => _NfcReadingScreenState();
}

class _NfcReadingScreenState extends State<NfcReadingScreen> {
  final NfcReader _reader = const NfcReader();
  double _progress = 0;
  bool _isReading = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _startReading();
  }

  Future<void> _startReading() async {
    setState(() {
      _isReading = true;
      _error = null;
    });

    try {
      final result = await _reader.readMrtd(
        mrzData: widget.mrzData,
        onProgress: (progress) {
          setState(() {
            _progress = progress;
          });
        },
      );

      if (!mounted) return;

      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (_) => ResultScreen(
            documentType: widget.documentType,
            mrzData: widget.mrzData,
            readResult: result,
          ),
        ),
      );
    } catch (error) {
      setState(() {
        _error = error.toString();
      });
    } finally {
      setState(() {
        _isReading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('قراءة البطاقة عبر NFC'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              const SizedBox(height: 24),
              const Icon(Icons.nfc, size: 64),
              const SizedBox(height: 16),
              Text(
                'أبقِ ${widget.documentType.displayName} ملاصقة لهاتفك حتى اكتمال المؤشر.',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodyLarge,
              ),
              const SizedBox(height: 32),
              LinearProgressIndicator(value: _isReading ? (_progress.clamp(0.0, 1.0)) : null),
              const SizedBox(height: 16),
              Text(
                _isReading
                    ? 'جاري الاتصال (${(_progress * 100).toStringAsFixed(0)}%)'
                    : _error == null
                        ? 'انتهت المحاولة'
                        : 'تعذر القراءة',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              if (_error != null) ...[
                const SizedBox(height: 12),
                Text(
                  _error!,
                  textAlign: TextAlign.center,
                  style: TextStyle(color: Theme.of(context).colorScheme.error),
                ),
                const SizedBox(height: 12),
                FilledButton(
                  onPressed: _startReading,
                  child: const Padding(
                    padding: EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                    child: Text('إعادة المحاولة'),
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
