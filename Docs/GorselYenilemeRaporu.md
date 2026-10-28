# WOBBLE — Görsel Yenileme Raporu

Bu çalışma yalnızca sunumu kapsar. İstifleme mekaniği, blok kesme mantığı, skor/combo kuralları, giriş-hareket mantığı, ekonomi kuralları, lokalizasyon yapısı ve menü davranışları değiştirilmedi. Tüm görseller kod tarafından üretilir; yeni ağır doku, model veya ekran efekti eklenmedi.

## 1. Yükseltilen ekranlar

| Ekran | Yapılan |
| --- | --- |
| Ana menü | Bulut denizinin üstünde yüzen galeri dünyası, sütun/niş kemerleri, servi ve çalı bahçesi, 4 katmanlı izometrik taş önizleme, aksan şeritli tema kartları, koyu okunur "Dokun · İstifle · Yüksel" satırı, parlak OYNA butonu |
| Tema seçimi | Beş tema kartı kendi sahne rengi, aksan şeridi ve 3D taş önizlemesiyle ayrışır; seçili kart vurgu rengine boyanır |
| Normal oynanış | Tema bazlı gökyüzü, sis, bulut denizi, yüzen harabeler, sütunlar, portal ve zemin (plinth) rengi |
| Perfect / combo anı | Altın halka, kıvılcım, combo aurası ve kamera mikro impulsu |
| Wobble anları | Blok yerleşirken hafif eğim (lean), sürekli toz ve tema aksanı parçacıkları |
| Çöküş / fail | Kule devrilirken kıvılcım patlaması, kırmızı ekran flaşı, ağır kamera sarsıntısı |
| Sonuç ekranı | Skor, en iyi skor, blok, mükemmel, maks combo, kazanılan elmas; tekrar dene / ana menü / paylaş / devam et |

## 2. Tema bazlı iyileştirmeler

Her tema `ThemeVisuals` içinde ayrı bir sanat yönetimi tarifi kullanır: gökyüzü üç duraklı gradyan, güneş yönü/şiddeti, bulut tonu, sis mesafesi, harabe rengi, blok kenar ışığı ve ışık yükseltmesi.

- **Carrara Sarayı** — Doygun seramik mavisi gökyüzü (`#53ACE6` alt, `#9FD4F4` ufuk), sıcak beyaz mermer, lacivert-gri harabe siluetleri, beyaz bulut denizi, altın aksan. Premium "gündüz saray" hissi.
- **Obsidyen Kalesi** — Kömür karası gökyüzü, kor turuncusu ufuk (`#76301A`), volkanik kül denizi (`#5A6678` / `#232B3A`), terracotta bazalt bloklar ve turuncu Fresnel kenar. Gece teması 2.2× ışık yükseltmesiyle okunur kalır.
- **Antik Anıt** — Kum taşı ve altın paleti, sıcak pus, altın kenar ışığı; blok yüzeyleri gün ışığında net.
- **Neon Şehir** — Erik-mor gece, macenta ufuk (`#A326E0`), camgöbeği kenar ışığı, turkuaz taş. 1.7× ışık yükseltmesi ve 1.45 kenar parlaklığı ile siluetler ayrışır.
- **Uzay Kulesi** — Indigo/menekşe gökyüzü, yoğunlaştırılmış ve parlatılmış yıldız alanı, camgöbeği rim, 1.75× ışık yükseltmesi. Derin uzay hissi bulut denizinin yarı saydam katmanından görünür.

## 3. Oynanış görsel değişiklikleri

- Blok shader'ına ucuz bir Fresnel kenar (`_RimColor`, `_RimStrength`) eklendi; kenar rengi tema aksanından gelir. Ek ışık veya ekran efekti kullanılmaz.
- Taş yüzeyleri mermer dokusunu korur; kesim UV'leri ve kesilen yüzün görünümü değişmedi.
- Havuzlanmış parçacık sistemi: toz, aksan kıvılcımı, combo aurası, şok dalgası ve fail patlaması. Aynı anda en fazla 4 efekt örneği döner.
- Zemin (plinth) her temada `propDark`/`propLight` karışımına boyanır; kule ile zemin aynı sahnede durur.
- Sahne arkası: yüzen mermer adalar, molozlar, uzak portal/niş kemerleri, servi ve çalı mesh'leri. Hepsi ayrı bir kök altında ve oyun düzleminin arkasında kalır.

## 4. Combo / wobble / fail değişiklikleri

- **Perfect**: altın halka + kıvılcım + kısa kamera impulsu (`.022`), HUD'da tema rengine uyarlanan "MÜKEMMEL / KOMBO ×N" metni.
- **Combo**: aksan aurası combo ile büyür, kamera impulsu `combo / 8` ile ölçeklenir (`.026` tepe), blok kenarları aksan rengine yaklaşır.
- **Wobble**: yerleşen blok kesim oranına göre hafifçe eğilir; toz ve aksan parçacıkları sürekli akar.
- **Fail**: kule devrilirken parçacık patlaması, kırmızı flaş, `.42 s` süren ağır sarsıntı (`.085`); skor/combo kuralları aynı kaldı.
- Ses tarafında tek bir onaylı yerleştirme sesi (slot 0) kullanılır; eski perküsyon bankası kaldırıldı.

## 5. UI değişiklikleri

- Sonuç ekranı yeniden düzenlendi: skor ve en iyi skor üstte, altında 2×2 istatistik kartı — **Blok, Mükemmel, Maks Combo (×N), Elmas (+N)**. Kartlar `372×156`, taşma yok, çakışma yok.
- Aksiyonlar: **Tekrar Dene** (birincil), **Ana Menü**, **Paylaş**, **Devam Et** (kalan can sayısıyla).
- HUD üstüne okunabilirlik için ince gradyan perde eklendi; skor vurgusu ve tema bazlı metin renkleri korundu.
- Combo metni koyu temalarda krem + tema aksanı karışımına geçer; açık temalarda mevcut koyu yeşil ton korunur.
- Ana menüde "En İyi" plakası koyulaştırıldı, OYNA butonuna parlaklık geldi, tema kartları sahne rengi + aksan şeridi + 3D önizleme ile yenilendi.
- Tüm yeni metinler mevcut Türkçe/İngilizce anahtarlarla eklendi; string table'lar yeniden üretildi, "No translation found" yok.

## 6. Kamera / kadraj değişiklikleri

- Kamera taban mesafesi `14 → 13.2`; kule portre ekranda daha büyük yer kaplar.
- Yatay güvenlik marjı `.95` ve FOV `40` korundu; hareket uçları `.02–.98` / `.20–.78` viewport sınırları içinde kalır.
- Perfect, combo ve fail için ayrı, kısa ve sönümlü impuls'lar eklendi; kadraj testleri etkilenmez.
- Gökyüzü gradyanı aşağı bakan kameraya göre yeniden ayarlandı: görünen bant artık `skyBottom`, ufuk yalnızca üstte ince bir şerit.
- Bulut denizi harabelerin altına indirildi; uzak harabelerin "hayalet" gibi solması giderildi.

## 7. Mobil optimizasyon

- Ekstra sahne geometrisi yok: `CarraraWorld.GeometryCount == 24` sabit, sahne collider'ı yok.
- Parçacık limiti `≤ 36`, efektler havuzlu; yeni materyal/doku üretimi yok.
- Tek yönlü ışık, gölge yalnızca kulede; arka plan ve bitkiler gölge basmaz.
- Post-process tek volume: tonemap + renk ayarı + hafif bloom (`1.05 / 0.22 / 0.55`).
- Bulut dokusu 192 px olarak bir kez pişirilir; gökyüzü, yıldız ve bulut denizi matematiksel (doku okuması yok).
- Materyaller paylaşılır, kule blokları tek materyal; 60 FPS hedefi ve `vSync` kapalı.

## 8. Doğrulama

- **Play Mode**: 27 geçti / 0 başarısız / 1 atlandı (atlanan yalnızca demo derlemesine özel `AllThemesDemoCanSelectAndPlayEveryTheme`).
- **Edit Mode**: 19 / 19 geçti.
- Ekran görüntüleri yeniden üretildi: `Artifacts/Previews`, `Artifacts/ThemeEnvironment`, `Artifacts/Menu/Previews`, `Artifacts/Feel`.
- Değişiklikler `a313521` commit'i ile `main` dalına gönderildi.
