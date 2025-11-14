import 'package:camera/camera.dart';
import 'package:flutter/material.dart';

import '../models/document_type.dart';
import 'mrz_instructions_screen.dart';

class WelcomeScreen extends StatelessWidget {
  const WelcomeScreen({super.key, required this.availableCameras});

  final List<CameraDescription> availableCameras;

  void _openMrzInstructions(BuildContext context, DocumentType type) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => MrzInstructionsScreen(
          documentType: type,
          availableCameras: availableCameras,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surfaceVariant,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 48),
              Text(
                'مرحباً بك في قارئ الهوية الذكية',
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              Text(
                'اختر نوع الوثيقة التي ترغب في قراءتها لبدء الإرشادات خطوة بخطوة.',
                style: Theme.of(context).textTheme.bodyLarge,
                textAlign: TextAlign.center,
              ),
              const Spacer(),
              FilledButton(
                onPressed: () => _openMrzInstructions(context, DocumentType.passport),
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20),
                  child: Text('جواز السفر'),
                ),
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => _openMrzInstructions(context, DocumentType.nationalId),
                child: const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20),
                  child: Text('البطاقة الوطنية'),
                ),
              ),
              const Spacer(),
            ],
          ),
        ),
      ),
    );
  }
}
