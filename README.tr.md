[English](./README.md) · [Türkçe](./README.tr.md)
# 🚗⚡ NetLpr

### Yapay Zeka Destekli Gerçek Zamanlı Plaka Tanıma ve Araç Takip Sistemi

**NetLpr**, **.NET 8** ve **C#** altyapısı üzerine geliştirilmiş; RTSP/IP kamera akışları üzerinden gerçek zamanlı **LPR / ANPR**, araç tespiti, plaka tanıma ve araç takip işlemlerini gerçekleştiren bir masaüstü uygulamasıdır.

---

## ✨ Özellikler

* 🎥 **Gerçek Zamanlı RTSP/IP Kamera Desteği**
* 🤖 **Yapay Zeka Destekli Araç ve Plaka Tespiti**
* 🔤 **OCR Tabanlı Plaka Tanıma**
* 🧠 **Intel OpenVINO™ Inference**
* 💾 **SQLite + Entity Framework Core**
* 🌐 **Runtime Dinamik Çoklu Dil Desteği**
* 🎨 **Modern Fluent UI**
* 🖥️ **Platform Bağımsız Avalonia UI**
* 🧵 **Asenkron ve Yüksek Performanslı İşleme Pipeline'ı**
* 🧠 **C# Unsafe Memory Optimizasyonları**
* 📝 **Serilog ile Yapılandırılmış Logging**
* ⚙️ **Modüler ve Genişletilebilir Mimari**

---

## 🏗️ Mimari


```text
NetLpr/
│
├── 🚀 NetLpr.Boot
│   └── Uygulama başlangıcı, DI Container & IoC yapılandırması
│
├── 🧠 NetLpr.Core
│   └── Domain modelleri, iş mantığı ve servis kontratları
│
├── 📹 NetLpr.Realtime
│   └── FFmpeg tabanlı düşük gecikmeli RTSP stream işleme
│
├── 👁️ NetLpr.ImageProcessingCv
│   └── OpenCV görüntü işleme ve bellek operasyonları
│
├── 🤖 NetLpr.InferenceOv
│   └── OpenVINO inference motoru ve OCR işlemleri
│
├── 💾 NetLpr.Persistence
│   └── EF Core, SQLite, migration ve repository'ler
│
├── 📝 NetLpr.Logs
│   └── Serilog tabanlı yapılandırılmış logging
│
├── 🌐 NetLpr.Localization
│   └── Runtime localization ve dil sözlükleri
│
└── 💻 NetLpr.Desktop
    └── Avalonia UI & MVVM masaüstü uygulaması
```

---

# 📸 Ekran Görüntüleri

## 🖥️ Ana Panel

Canlı kamera akışları, tespit edilen plakalar, sistem istatistikleri ve anlık işlem durumları.

![Ana Panel](./images/1.png)

---

## 🎥 Kamera Yapılandırması & ROI

RTSP bağlantılarını yapılandırabilir ve kamera görüntüsü üzerinde **Region of Interest (ROI)** bölgelerini etkileşimli olarak belirleyebilirsiniz.

![Kamera Yapılandırması](./images/2.png)

---

## 📊 Tespit Geçmişi

Tespit edilen araç ve plakalar aranabilir ve filtrelenebilir bir geçmiş ekranı üzerinden incelenebilir.

![AI Ayarları](./images/3.png)

---

# 🧠 AI Pipeline

Plaka tanıma pipeline'ı birden fazla aşamadan oluşur:

```text
Camera Frame
     │
     ▼
Vehicle Detection
     │
     ▼
Vehicle Tracking
     │
     ▼
License Plate Detection
     │
     ▼
Image Preprocessing
     │
     ▼
Plate Detection
     │
     ▼
    OCR 
     │
     ▼
Detection Result
```

Modüler inference katmanı sayesinde farklı AI modelleri ve inference backend'leri masaüstü uygulamasına doğrudan bağımlı olmadan entegre edilebilir.

---

# 💾 Veri Kalıcılığı

NetLpr, yerel veri saklama işlemleri için **Entity Framework Core** ve **SQLite** kullanır.

Sistemde aşağıdaki bilgiler saklanabilir:

* Plaka sonuçları
* Tespit zamanları
* Kamera yapılandırmaları
* Analiz ayarları
* Uygulama ayarları
* Araç takip geçmişi

Persistence katmanı domain ve UI katmanlarından ayrılmıştır. Böylece ihtiyaç halinde farklı veritabanı sağlayıcılarının sisteme eklenebilmesi mümkün hale gelir.

---

# 🌐 Localization

NetLpr, bağımsız bir localization katmanına sahiptir:

```text
NetLpr.Localization
```

Dil kaynakları uygulamayı yeniden başlatmaya gerek kalmadan runtime sırasında değiştirilebilir.

Bu sayede masaüstü arayüzü dil değişikliklerine dinamik olarak tepki verebilir.

---

# 🎨 Masaüstü Uygulaması

Masaüstü istemcisi:

* **Avalonia UI**
* **MVVM**
* **Fluent UI**
* **.NET 8**

kullanılarak geliştirilmiştir.

Arayüz, modern bir masaüstü deneyimi sunarken belirli bir işletim sistemine bağımlı olmayacak şekilde tasarlanmıştır.

---

# 🛠️ Teknoloji Stack'i

| Teknoloji                 | Kullanım Alanı                       |
| ------------------------- | ------------------------------------ |
| **C#**                    | Uygulama ve performans kritik kodlar |
| **.NET 8**                | Runtime ve uygulama platformu        |
| **Avalonia UI**           | Platform bağımsız masaüstü arayüzü   |
| **OpenVINO™**             | AI inference hızlandırma             |
| **OpenCV**                | Görüntü işleme                       |
| **FFmpeg.AutoGen**        | RTSP/video decoding                  |
| **Entity Framework Core** | ORM ve persistence                   |
| **SQLite**                | Yerel veritabanı                     |
| **Serilog**               | Yapılandırılmış logging              |
| **MVVM**                  | UI mimarisi                          |

---

# 📦 Gereksinimler

NetLpr'ı çalıştırmadan önce aşağıdaki bileşenlerin sistemde bulunması gerekir:

* .NET 8 SDK
* Desteklenen Windows, Linux veya macOS ortamı
* Uyumlu FFmpeg runtime kütüphaneleri
* OpenVINO runtime
* Uyumlu AI modelleri


* 🤖 **AI Model [fast-alpr](https://github.com/ankandrew/fast-alpr)**
---

# ⚙️ Yapılandırma

Kamera ve inference ayarları uygulama içerisinden yapılandırılabilir.

Temel yapılandırma:

```text
Camera
├── RTSP URL
├── Authentication
├── Stream configuration
└── ROI

Inference
├── Vehicle Detection Model
├── License Plate Detection Model
├── OCR Model
├── Confidence Threshold
└── Target Device

Processing
├── Resolution
├── FPS
├── Tracking
└── Preprocessing
```

---

# 📁 Proje Yapısı

```text
NetLpr
│
├── NetLpr.Boot
├── NetLpr.Core
├── NetLpr.Realtime
├── NetLpr.ImageProcessingCv
├── NetLpr.InferenceOv
├── NetLpr.Persistence
├── NetLpr.Logs
├── NetLpr.Localization
└── NetLpr.Desktop
```



<div align="center">

### 🚗 NetLpr

**Real-Time License Plate Recognition • AI • Computer Vision**

</div>
