import 'dart:io';
import 'dart:typed_data';
import 'dart:ui' as ui;

import 'package:camera/camera.dart';
import 'package:flutter/material.dart';
import 'package:google_mlkit_commons/google_mlkit_commons.dart';
import 'package:google_mlkit_text_recognition/google_mlkit_text_recognition.dart';

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
  ui.Rect? _detectedBoundingBox;
  bool _isStreaming = false;
  bool _isProcessingFrame = false;
  bool _autoCapturePending = false;
  int _stableDetections = 0;
  DateTime? _lastFrameProcessed;

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
      imageFormatGroup: ImageFormatGroup.yuv420,
    );

    final initializeFuture = controller.initialize().then((_) {
      return _startImageStream();
    });

    setState(() {
      _controller = controller;
      _initializeControllerFuture = initializeFuture;
    });
  }

  Future<void> _startImageStream() async {
    final controller = _controller;
    if (controller == null || _isStreaming) {
      return;
    }

    try {
      await controller.startImageStream(_processCameraImage);
      _isStreaming = true;
    } on CameraException {
      _isStreaming = false;
    }
  }

  Future<void> _stopImageStream() async {
    final controller = _controller;
    if (controller == null || !_isStreaming) {
      return;
    }

    try {
      await controller.stopImageStream();
    } on CameraException {
      // ignore
    } finally {
      _isStreaming = false;
    }
  }

  Future<void> _processCameraImage(CameraImage image) async {
    if (_isProcessingFrame || _isProcessing) {
      return;
    }

    final now = DateTime.now();
    if (_lastFrameProcessed != null &&
        now.difference(_lastFrameProcessed!).inMilliseconds < 400) {
      return;
    }
    _lastFrameProcessed = now;

    _isProcessingFrame = true;
    try {
      final controller = _controller;
      if (controller == null) {
        return;
      }

      final rotation = controller.description.sensorOrientation;
      final inputImage = _buildInputImage(image, rotation);
      final Size rotatedSize = rotation == 90 || rotation == 270
          ? Size(image.height.toDouble(), image.width.toDouble())
          : Size(image.width.toDouble(), image.height.toDouble());

      final detection =
          await _scanner.detectMrz(inputImage, rotatedSize);
      if (!mounted) {
        return;
      }

      if (detection != null) {
        final left = (detection.boundingBox.left / detection.imageSize.width)
            .clamp(0.0, 1.0)
            .toDouble();
        final top = (detection.boundingBox.top / detection.imageSize.height)
            .clamp(0.0, 1.0)
            .toDouble();
        final right = (detection.boundingBox.right / detection.imageSize.width)
            .clamp(0.0, 1.0)
            .toDouble();
        final bottom =
            (detection.boundingBox.bottom / detection.imageSize.height)
                .clamp(0.0, 1.0)
                .toDouble();

        final normalized = ui.Rect.fromLTRB(left, top, right, bottom);

        setState(() {
          _detectedBoundingBox = normalized;
        });
        _stableDetections = (_stableDetections + 1).clamp(0, 4);

        if (_stableDetections >= 2 && !_autoCapturePending) {
          _autoCapturePending = true;
          _captureAndProcess(autoTriggered: true);
        }
      } else {
        _stableDetections = 0;
        if (_detectedBoundingBox != null) {
          setState(() {
            _detectedBoundingBox = null;
          });
        }
      }
    } catch (_) {
      // ignore frame errors
    } finally {
      _isProcessingFrame = false;
    }
  }

  Uint8List _concatenatePlanes(List<Plane> planes) {
    final totalLength = planes.fold<int>(0, (sum, plane) => sum + plane.bytes.length);
    final bytes = Uint8List(totalLength);
    var offset = 0;
    for (final plane in planes) {
      bytes.setRange(offset, offset + plane.bytes.length, plane.bytes);
      offset += plane.bytes.length;
    }
    return bytes;
  }

  InputImage _buildInputImage(CameraImage image, int rotation) {
    final bytes = _concatenatePlanes(image.planes);

    final Size imageSize = Size(
      image.width.toDouble(),
      image.height.toDouble(),
    );

    final imageRotation = InputImageRotationValue.fromRawValue(rotation) ??
        InputImageRotation.rotation0deg;

    final inputImageFormat =
        InputImageFormatValue.fromRawValue(image.format.raw) ??
            InputImageFormat.nv21;

    final planeData = image.planes
        .map(
          (plane) => InputImagePlaneMetadata(
            bytesPerRow: plane.bytesPerRow,
            height: plane.height,
            width: plane.width,
          ),
        )
        .toList();

    return InputImage.fromBytes(
      bytes: bytes,
      metadata: InputImageMetadata(
        size: imageSize,
        rotation: imageRotation,
        format: inputImageFormat,
        planeData: planeData,
      ),
    );
  }

  Future<void> _captureAndProcess({bool autoTriggered = false}) async {
    final controller = _controller;
    if (controller == null) {
      if (autoTriggered) {
        _autoCapturePending = false;
      }
      return;
    }
    if (_isProcessing) {
      if (autoTriggered) {
        _autoCapturePending = false;
      }
      return;
    }

    try {
      await _initializeControllerFuture;
      setState(() {
        _isProcessing = true;
        _errorMessage = null;
      });

      await _stopImageStream();

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

      if (mounted) {
        _stableDetections = 0;
        _detectedBoundingBox = null;
      }
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
        if (autoTriggered) {
          _autoCapturePending = false;
        }
        if (!_isStreaming) {
          await _startImageStream();
        }
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
                                    MrzCameraGuides(
                                      detectedMrz: _detectedBoundingBox,
                                    ),
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
