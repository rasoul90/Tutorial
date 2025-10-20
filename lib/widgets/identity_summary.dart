import 'package:flutter/material.dart';

import '../models/identity_document.dart';

class IdentitySummary extends StatelessWidget {
  const IdentitySummary({super.key, required this.document});

  final IdentityDocument document;

  @override
  Widget build(BuildContext context) {
    final chipData = document.chipData;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _buildTile('الاسم الكامل (إنجليزي)', document.fullNameEnglish),
        if (document.fullNameArabic.isNotEmpty)
          _buildTile('الاسم الكامل (عربي)', document.fullNameArabic),
        _buildTile('رقم الوثيقة', document.documentNumber),
        _buildTile('الرقم الوطني', document.nationalNumber),
        _buildTile(
          'تاريخ الميلاد',
          _formatDate(document.dateOfBirth),
        ),
        _buildTile('الجنس', document.gender),
        _buildTile(
          'تاريخ انتهاء الصلاحية',
          _formatDate(document.expiryDate),
        ),
        _buildTile('مكان الميلاد', document.placeOfBirth),
        _buildTile('جهة الإصدار', document.cardIssuer),
        _buildTile('MRZ', document.mrz),
        if (chipData != null) ...[
          const SizedBox(height: 16),
          const Text(
            'بيانات الشريحة (NFC)',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          for (final entry in chipData.entries)
            _buildTile(entry.key, '${entry.value}'),
        ],
      ],
    );
  }

  Widget _buildTile(String title, String value) {
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 6),
      child: ListTile(
        title: Text(title),
        subtitle: Text(value.isEmpty ? 'غير متوفر' : value),
      ),
    );
  }

  String _formatDate(DateTime value) {
    return '${value.year.toString().padLeft(4, '0')}-${value.month.toString().padLeft(2, '0')}-${value.day.toString().padLeft(2, '0')}';
  }
}
