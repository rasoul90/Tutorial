import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../models/document_type.dart';
import '../models/mrz_data.dart';
import '../services/nfc_reader.dart';

class ResultScreen extends StatelessWidget {
  const ResultScreen({
    super.key,
    required this.documentType,
    required this.mrzData,
    required this.readResult,
  });

  final DocumentType documentType;
  final MrzData mrzData;
  final NfcReadResult readResult;

  @override
  Widget build(BuildContext context) {
    final info = readResult.personalInfo;
    final doc = readResult.documentInfo;
    final chip = readResult.chipInfo;
    final formatter = DateFormat('yyyy-MM-dd');

    return Scaffold(
      appBar: AppBar(
        title: const Text('نتائج القراءة'),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'تمت قراءة ${documentType.displayName} بنجاح',
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 16),
              if (readResult.portrait != null)
                Center(
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(12),
                    child: Image.memory(
                      readResult.portrait!,
                      width: 180,
                      height: 220,
                      fit: BoxFit.cover,
                    ),
                  ),
                ),
              const SizedBox(height: 24),
              _InfoSection(
                title: 'البيانات الشخصية',
                entries: [
                  _InfoEntry('الاسم الكامل', info.fullName),
                  if (info.secondaryName.isNotEmpty)
                    _InfoEntry('أسماء أخرى', info.secondaryName),
                  _InfoEntry('الجنس', info.gender),
                  _InfoEntry('تاريخ الولادة', formatter.format(info.birthDate)),
                  if (info.birthPlace?.isNotEmpty ?? false)
                    _InfoEntry('مكان الولادة', info.birthPlace!),
                  _InfoEntry('الجنسية', info.nationality),
                  if (info.personalNumber?.isNotEmpty ?? false)
                    _InfoEntry('الرقم الشخصي', info.personalNumber!),
                ],
              ),
              const SizedBox(height: 24),
              _InfoSection(
                title: 'بيانات الوثيقة',
                entries: [
                  _InfoEntry('رقم الوثيقة', doc.documentNumber),
                  _InfoEntry('الدولة المصدرة', doc.issuingState),
                  _InfoEntry('نوع MRZ', doc.documentType),
                  if (doc.issueDate != null)
                    _InfoEntry('تاريخ الإصدار', formatter.format(doc.issueDate!)),
                  _InfoEntry('تاريخ الانتهاء', formatter.format(doc.expiryDate)),
                ],
              ),
              const SizedBox(height: 24),
              _InfoSection(
                title: 'نص MRZ من الشريحة',
                child: Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.surfaceVariant,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      for (final line in readResult.mrzLinesFromChip)
                        Text(
                          line,
                          style: const TextStyle(fontFamily: 'monospace'),
                        ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 24),
              _InfoSection(
                title: 'معلومات الشريحة',
                entries: [
                  _InfoEntry('إصدار LDS', chip.ldsVersion),
                  _InfoEntry('مجموعات البيانات', chip.dataGroups.join(', ')),
                  _InfoEntry('ميزات الأمان', chip.features.join(', ')),
                  _InfoEntry('نوع التحكم بالدخول', chip.accessControl),
                  _InfoEntry('نوع التاج', chip.tagTech),
                  _InfoEntry('المعيار المدعوم', chip.isoStandard),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _InfoSection extends StatelessWidget {
  const _InfoSection({
    required this.title,
    this.entries,
    this.child,
  });

  final String title;
  final List<_InfoEntry>? entries;
  final Widget? child;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 8),
        if (entries != null)
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surfaceVariant,
              borderRadius: BorderRadius.circular(12),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: entries!
                  .map(
                    (entry) => Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: RichText(
                        text: TextSpan(
                          style: Theme.of(context).textTheme.bodyLarge,
                          children: [
                            TextSpan(
                              text: '${entry.label}: ',
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            TextSpan(text: entry.value),
                          ],
                        ),
                      ),
                    ),
                  )
                  .toList(),
            ),
          ),
        if (child != null) child!,
      ],
    );
  }
}

class _InfoEntry {
  _InfoEntry(this.label, this.value);

  final String label;
  final String value;
}
