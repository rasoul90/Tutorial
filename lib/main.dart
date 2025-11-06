import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

void main() {
  runApp(const ReadIdCloneApp());
}

class ReadIdCloneApp extends StatelessWidget {
  const ReadIdCloneApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'قارئ الهوية',
      debugShowCheckedModeBanner: false,
      locale: const Locale('ar'),
      supportedLocales: const [Locale('ar')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF005F73),
          brightness: Brightness.light,
        ),
        fontFamily: 'Roboto',
      ),
      home: const WelcomePage(),
    );
  }
}

class WelcomePage extends StatelessWidget {
  const WelcomePage({super.key});

  @override
  Widget build(BuildContext context) {
    final spacing = MediaQuery.of(context).size.height * 0.04;
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        backgroundColor: Theme.of(context).colorScheme.surface,
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Spacer(),
                Icon(
                  Icons.verified_user,
                  size: MediaQuery.of(context).size.width * 0.3,
                  color: Theme.of(context).colorScheme.primary,
                ),
                SizedBox(height: spacing),
                Text(
                  'مرحباً بك في تطبيق التحقق من الهوية',
                  style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 12),
                Text(
                  'اختر نوع الوثيقة التي ترغب في مسحها وقراءة بياناتها عبر تقنية NFC.',
                  style: Theme.of(context).textTheme.bodyLarge,
                  textAlign: TextAlign.center,
                ),
                const Spacer(),
                ElevatedButton(
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => const PassportPlaceholderPage(),
                      ),
                    );
                  },
                  child: const Padding(
                    padding: EdgeInsets.symmetric(vertical: 16),
                    child: Text('جواز السفر'),
                  ),
                ),
                const SizedBox(height: 16),
                ElevatedButton(
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => const IdInstructionsPage(),
                      ),
                    );
                  },
                  child: const Padding(
                    padding: EdgeInsets.symmetric(vertical: 16),
                    child: Text('الهوية التعريفية (البطاقة الوطنية)'),
                  ),
                ),
                const Spacer(),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class PassportPlaceholderPage extends StatelessWidget {
  const PassportPlaceholderPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('مسح جواز السفر'),
        ),
        body: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'الميزة قيد التطوير',
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 16),
              Text(
                'حالياً يركز التطبيق على مسار البطاقة الوطنية. استخدم القائمة الرئيسية للعودة إلى الشاشة السابقة واختيار الهوية التعريفية.',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class IdInstructionsPage extends StatelessWidget {
  const IdInstructionsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('التقاط منطقة MRZ'),
        ),
        body: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'للحصول على أفضل قراءة، اتبع هذه الإرشادات قبل تصوير منطقة MRZ:',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 16),
              const InstructionBullet(
                text: 'ضع البطاقة الوطنية على سطح مستوٍ وخالٍ من اللمعان.',
              ),
              const InstructionBullet(
                text: 'تأكد من أن الإضاءة جيدة وأن منطقة MRZ واضحة بالكامل.',
              ),
              const InstructionBullet(
                text: 'أبعد أي عناصر قد تعيق الرؤية مثل الأصابع أو الظلال.',
              ),
              const InstructionBullet(
                text: 'أمسك الهاتف بثبات وتأكد من ظهور السطور الثلاثة كاملة داخل الإطار.',
              ),
              const Spacer(),
              SizedBox(
                width: double.infinity,
                child: FilledButton(
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => const MrzCameraPage(),
                      ),
                    );
                  },
                  child: const Padding(
                    padding: EdgeInsets.symmetric(vertical: 16),
                    child: Text('التالي'),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class InstructionBullet extends StatelessWidget {
  const InstructionBullet({required this.text, super.key});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(
            Icons.check_circle_outline,
            color: Theme.of(context).colorScheme.primary,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              text,
              style: Theme.of(context).textTheme.bodyLarge,
            ),
          ),
        ],
      ),
    );
  }
}

class MrzCameraPage extends StatefulWidget {
  const MrzCameraPage({super.key});

  @override
  State<MrzCameraPage> createState() => _MrzCameraPageState();
}

class _MrzCameraPageState extends State<MrzCameraPage> {
  bool _captured = false;

  Future<void> _captureMrz() async {
    setState(() {
      _captured = true;
    });
    await Future<void>.delayed(const Duration(seconds: 1));
    if (!mounted) {
      return;
    }
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) => const NfcInstructionsPage(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('التقاط منطقة MRZ'),
        ),
        body: Column(
          children: [
            Expanded(
              child: Container(
                color: Colors.black,
                child: Stack(
                  alignment: Alignment.center,
                  children: [
                    AnimatedContainer(
                      duration: const Duration(milliseconds: 300),
                      width: double.infinity,
                      margin: const EdgeInsets.symmetric(horizontal: 32),
                      height: 180,
                      decoration: BoxDecoration(
                        border: Border.all(
                          color:
                              _captured ? Colors.greenAccent : Colors.white,
                          width: 4,
                        ),
                        borderRadius: BorderRadius.circular(12),
                        color: Colors.white.withOpacity(0.05),
                      ),
                    ),
                    Positioned(
                      bottom: 24,
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                          vertical: 8,
                          horizontal: 16,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.black.withOpacity(0.6),
                          borderRadius: BorderRadius.circular(24),
                        ),
                        child: Text(
                          _captured
                              ? 'تم التقاط MRZ بنجاح'
                              : 'وجه الكاميرا نحو منطقة MRZ داخل الإطار الأبيض',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'عند استعدادك اضغط على زر الالتقاط ليتم تحليل النص الموجود في منطقة MRZ تلقائياً.',
                    style: Theme.of(context).textTheme.bodyLarge,
                  ),
                  const SizedBox(height: 16),
                  FilledButton.icon(
                    onPressed: _captured ? null : _captureMrz,
                    icon: const Icon(Icons.camera_alt),
                    label: const Padding(
                      padding: EdgeInsets.symmetric(vertical: 16),
                      child: Text('التقاط MRZ'),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class NfcInstructionsPage extends StatelessWidget {
  const NfcInstructionsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('تعليمات قراءة NFC'),
        ),
        body: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'الآن حان وقت قراءة الشريحة الذكية للبطاقة الوطنية:',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 16),
              const InstructionBullet(
                text: 'قم بتمرير ظهر الهاتف على المنطقة التي تحتوي شريحة NFC في البطاقة.',
              ),
              const InstructionBullet(
                text: 'حافظ على ثبات الهاتف ولا ترفعه حتى يكتمل الاتصال.',
              ),
              const InstructionBullet(
                text: 'تأكد من تفعيل خاصية NFC على هاتفك قبل البدء.',
              ),
              const Spacer(),
              SizedBox(
                width: double.infinity,
                child: FilledButton(
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => const NfcProgressPage(),
                      ),
                    );
                  },
                  child: const Padding(
                    padding: EdgeInsets.symmetric(vertical: 16),
                    child: Text('بدء القراءة'),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class NfcProgressPage extends StatefulWidget {
  const NfcProgressPage({super.key});

  @override
  State<NfcProgressPage> createState() => _NfcProgressPageState();
}

class _NfcProgressPageState extends State<NfcProgressPage> {
  @override
  void initState() {
    super.initState();
    Timer(const Duration(seconds: 2), () {
      if (!mounted) {
        return;
      }
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (_) => ResultPage(
            data: PersonData(
              fullName: 'محمد أحمد عبد الله',
              secondaryNames: ['أحمد محمد', 'محمد أ. عبد الله'],
              gender: 'ذكر',
              birthDate: '01-01-1990',
              birthPlace: 'الرباط، المغرب',
              personalNumber: 'MA123456789',
              documentNumber: 'AB1234567',
              nationality: 'المملكة المغربية',
              issueDate: '01-06-2022',
              expiryDate: '01-06-2032',
              mrzFromChip:
                  'I<MARAB1234567<<<<<<<<<<<<<<<\n9001015M3206019MAR<<<<<<<<<<<6',
              chipDetails: const ChipDetails(
                ldsVersion: '1.7',
                features: ['EAC', 'PACE'],
                dataGroups: ['DG1', 'DG2', 'DG3', 'DG11', 'DG12', 'DG13', 'DG14'],
                accessControl: 'PACE',
                nfcTagType: 'ISO 7816',
                isoStandard: 'ISO 14443-4 (Type A)',
                mrzType: 'TD1',
              ),
            ),
          ),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('جاري الاتصال'),
        ),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const CircularProgressIndicator(),
              const SizedBox(height: 24),
              Text(
                'جارٍ الاتصال بشريحة NFC، يرجى عدم تحريك الهاتف...',
                style: Theme.of(context).textTheme.bodyLarge,
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class ResultPage extends StatelessWidget {
  const ResultPage({required this.data, super.key});

  final PersonData data;

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('نتيجة القراءة'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: CircleAvatar(
                  radius: 48,
                  backgroundColor: Theme.of(context).colorScheme.primary,
                  child: Text(
                    data.fullName.isNotEmpty
                        ? String.fromCharCode(data.fullName.runes.first)
                        : '?',
                    style: const TextStyle(
                      fontSize: 32,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Center(
                child: Text(
                  data.fullName,
                  style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
              ),
              const SizedBox(height: 24),
              InfoSection(
                title: 'معلومات شخصية',
                rows: [
                  InfoRow(label: 'الاسم الكامل', value: data.fullName),
                  InfoRow(
                    label: 'أسماء أخرى',
                    value: data.secondaryNames.join('، '),
                  ),
                  InfoRow(label: 'الجنس', value: data.gender),
                  InfoRow(label: 'تاريخ الولادة', value: data.birthDate),
                  InfoRow(label: 'مكان الولادة', value: data.birthPlace),
                ],
              ),
              const SizedBox(height: 16),
              InfoSection(
                title: 'معلومات الوثيقة',
                rows: [
                  InfoRow(label: 'الرقم الشخصي', value: data.personalNumber),
                  InfoRow(label: 'رقم الوثيقة', value: data.documentNumber),
                  InfoRow(label: 'الدولة', value: data.nationality),
                  InfoRow(label: 'تاريخ الإصدار', value: data.issueDate),
                  InfoRow(label: 'تاريخ الانتهاء', value: data.expiryDate),
                ],
              ),
              const SizedBox(height: 16),
              InfoSection(
                title: 'تفاصيل الشريحة',
                rows: [
                  InfoRow(label: 'إصدار LDS', value: data.chipDetails.ldsVersion),
                  InfoRow(
                    label: 'الخصائص',
                    value: data.chipDetails.features.join('، '),
                  ),
                  InfoRow(
                    label: 'مجموعات البيانات',
                    value: data.chipDetails.dataGroups.join('، '),
                  ),
                  InfoRow(
                    label: 'نوع التحكم في الوصول',
                    value: data.chipDetails.accessControl,
                  ),
                  InfoRow(
                    label: 'نوع وسم NFC',
                    value: data.chipDetails.nfcTagType,
                  ),
                  InfoRow(
                    label: 'المعيار المدعوم',
                    value: data.chipDetails.isoStandard,
                  ),
                  InfoRow(label: 'نوع MRZ', value: data.chipDetails.mrzType),
                ],
              ),
              const SizedBox(height: 16),
              Text(
                'MRZ من الشريحة:',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 8),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surfaceVariant,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  data.mrzFromChip,
                  style: const TextStyle(
                    fontFamily: 'RobotoMono',
                    fontSize: 16,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class InfoSection extends StatelessWidget {
  const InfoSection({required this.title, required this.rows, super.key});

  final String title;
  final List<InfoRow> rows;

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 12),
            ...rows,
          ],
        ),
      ),
    );
  }
}

class InfoRow extends StatelessWidget {
  const InfoRow({required this.label, required this.value, super.key});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 2,
            child: Text(
              label,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
          ),
          Expanded(
            flex: 3,
            child: Text(
              value,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }
}

class PersonData {
  PersonData({
    required this.fullName,
    required this.secondaryNames,
    required this.gender,
    required this.birthDate,
    required this.birthPlace,
    required this.personalNumber,
    required this.documentNumber,
    required this.nationality,
    required this.issueDate,
    required this.expiryDate,
    required this.mrzFromChip,
    required this.chipDetails,
  });

  final String fullName;
  final List<String> secondaryNames;
  final String gender;
  final String birthDate;
  final String birthPlace;
  final String personalNumber;
  final String documentNumber;
  final String nationality;
  final String issueDate;
  final String expiryDate;
  final String mrzFromChip;
  final ChipDetails chipDetails;
}

class ChipDetails {
  const ChipDetails({
    required this.ldsVersion,
    required this.features,
    required this.dataGroups,
    required this.accessControl,
    required this.nfcTagType,
    required this.isoStandard,
    required this.mrzType,
  });

  final String ldsVersion;
  final List<String> features;
  final List<String> dataGroups;
  final String accessControl;
  final String nfcTagType;
  final String isoStandard;
  final String mrzType;
}
