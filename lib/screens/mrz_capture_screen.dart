import 'dart:io';

import 'package:camera/camera.dart';
import 'package:flutter/material.dart';

import '../models/document_type.dart';
import '../models/mrz_scan_result.dart';
import '../services/mrz_scanner.dart';
import '../widgets/mrz_camera_guides.dart';
import 'mrz_review_screen.dart';

class MrzCaptureScreen extends StatefulWidget {
  const MrzCaptureScreen({
    super.key,
    required this.documentType,
    required this.availableCameras,
  });

  final DocumentType documentType;
  final List<CameraDescription> availableCameras;

  @override
  State<MrzCaptureScreen> createState() => _MrzCaptureScreenState();
}

class _MrzCaptureScreenState extends State<MrzCaptureScreen>
    with WidgetsBindingObserver {
  CameraController? _controller;
  Future<void>? _initializeControllerFuture;
  bool _isProcessing = false;
  String? _errorMessage;
  late final MrzScannerService _scanner;

  CameraDescription? _selectedCamera;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _scanner = MrzScannerService();
    _initializeCamera();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _controller?.dispose();
    _scanner.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    final controller = _controller;
    if (controller == null || !controller.value.isInitialized) {
      return;
    }

    if (state == AppLifecycleState.inactive) {
      controller.dispose();
    } else if (state == AppLifecycleState.resumed) {
      _initializeCamera(selected: _selectedCamera);
    }
  }

  void _initializeCamera({CameraDescription? selected}) {
    final camera = selected ??
        widget.availableCameras.firstWhere(
          (camera) => camera.lensDirection == CameraLensDirection.back,
          orElse: () => widget.availableCameras.isNotEmpty
              ? widget.availableCameras.first
              : throw StateError('لا توجد كاميرات متاحة على هذا الجهاز.'),
        );

    _selectedCamera = camera;
    final controller = CameraController(
      camera,
      ResolutionPreset.high,
      enableAudio: false,
      imageFormatGroup: ImageFormatGroup.jpeg,
    );

    setState(() {
      _controller = controller;
      _initializeControllerFuture = controller.initialize();
    });
  }

  Future<void> _captureAndProcess() async {
    final controller = _controller;
    if (controller == null) {
      return;
    }
    if (_isProcessing) {
      return;
    }

    try {
      await _initializeControllerFuture;
      setState(() {
        _isProcessing = true;
        _errorMessage = null;
      });

      final capture = await controller.takePicture();
      final result = await _scanner.scanImage(File(capture.path));

      if (!mounted) return;

      await Navigator.of(context).push(
        MaterialPageRoute(
          builder: (_) => MrzReviewScreen(
            documentType: widget.documentType,
            capturePath: capture.path,
            scanResult: result,
          ),
        ),
      );
    } on CameraException catch (error) {
      setState(() {
        _errorMessage = 'تعذر فتح الكاميرا: ${error.description ?? error.code}';
      });
    } on FormatException catch (error) {
      setState(() {
        _errorMessage = error.message;
      });
    } catch (error) {
      setState(() {
        _errorMessage = 'حدث خطأ غير متوقع: $error';
      });
    } finally {
      if (mounted) {
        setState(() {
          _isProcessing = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final controller = _controller;

    return Scaffold(
      appBar: AppBar(
        title: const Text('التقاط منطقة MRZ'),
      ),
      body: SafeArea(
        child: controller == null
            ? const Center(child: Text('لا يمكن تهيئة الكاميرا.'))
            : FutureBuilder<void>(
                future: _initializeControllerFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(child: CircularProgressIndicator());
                  }
                  if (snapshot.hasError) {
                    return Center(
                      child: Text('تعذر تشغيل الكاميرا: ${snapshot.error}'),
                    );
                  }

                  final previewSize = controller.value.previewSize;
                  final aspectRatio = previewSize != null
                      ? previewSize.height / previewSize.width
                      : 1 / controller.value.aspectRatio;

                  return Column(
                    children: [
                      Expanded(
                        child: Stack(
                          children: [
                            Center(
                              child: AspectRatio(
                                aspectRatio: aspectRatio,
                                child: Stack(
                                  fit: StackFit.expand,
                                  children: [
                                    CameraPreview(controller),
                                    const MrzCameraGuides(),
                                  ],
                                ),
                              ),
                            ),
                            if (_isProcessing)
                              Container(
                                color: Colors.black45,
                                child: const Center(
                                  child: CircularProgressIndicator(),
                                ),
                              ),
                          ],
                        ),
                      ),
                      if (_errorMessage != null)
                        Padding(
                          padding: const EdgeInsets.all(16),
                          child: Text(
                            _errorMessage!,
                            style: TextStyle(color: Theme.of(context).colorScheme.error),
                          ),
                        ),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
                        child: FilledButton.icon(
                          onPressed: _isProcessing ? null : _captureAndProcess,
                          icon: const Icon(Icons.camera_alt_outlined),
                          label: const Padding(
                            padding: EdgeInsets.symmetric(vertical: 12),
                            child: Text('التقاط'),
                          ),
                        ),
                      ),
                    ],
                  );
                },
              ),
      ),
    );
  }
}
