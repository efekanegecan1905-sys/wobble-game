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

- **Carrara Sarayı** — Doygun seramik mavisi gökyüzü (`#3E9BDF` alt, `#93D0F2` ufuk), sıcak beyaz mermer, lacivert-gri harabe siluetleri, krem bulut denizi (`#FFF4E2` / `#4E8FC9`), altın aksan. Premium "gündüz saray" hissi.
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

- **Play Mode (tam suite)**: 27 geçti / 0 başarısız / 1 atlandı (atlanan yalnızca demo derlemesine özel `AllThemesDemoCanSelectAndPlayEveryTheme`).
- **Edit Mode**: 19 / 19 geçti.
- Ekran görüntüleri yeniden üretildi: `Artifacts/Previews`, `Artifacts/ThemeEnvironment`, `Artifacts/Menu/Previews`, `Artifacts/Feel`.
- Bundle ID `com.wobble.game` (iOS + Android) değişmeden korundu.

## 9. Bu tur: ne yeniden inşa edildi, ne cilalandı?

Önceki tur ağırlıklı palet, materyal ve efekt cilasıydı; ekran iskeletleri prototip düzenini koruyordu. Bu turda sunum katmanı yeniden kuruldu, oynanış kurallarına dokunulmadı.

| Alan | Karar | Yapılan |
| --- | --- | --- |
| Ana menü | **Yeniden inşa** | Uzak "monolit" harabe sırası tamamen kaldırıldı (başlığın arkasında gri bıçaklar gibi duruyordu). Yerine üç teraslı, oyulmuş etekli ve ters koni gövdesiyle dağdan koparılmış hissi veren bir kaide; üzerinde 10 dilimlik hafif sarhoş kahraman kule, altın kakma taç, yüzen kristal + hale, tema başına ada ve landmark katmanı geldi. Logo girişi, OYNA hapı, motto plakası ve "En İyi" plakası yeniden yerleştirildi. |
| Tema seçimi | **Yeniden inşa** | Kartlar kodla boyanan `ThemePoster` sahne resmi, taş çerçeve, seçili durumda aksan çerçevesi/şeridi ve kilit/fiyat plakasıyla yenilendi; kartlar sırayla yaylanarak açılıyor. |
| Oynanış | **Yeniden inşa** | `ThemeDistrict` ile her temaya özel landmark, midground, yüzen ada ve moloz kompozisyonu; kamera açısına göre çerçeve kenarlarına yerleşiyor. Kaide `1.0 → .46` ölçeğe indirildi; kule tekrar kadrajın kahramanı. Carrara'da 8 sütun yerine 12 parçalı kırık parapet ve 5 çalı var. |
| Bloklar | **Cila** | Kesim, UV, ölçü ve collider aynı. Yüz ayrımı, tema kenar ışığı ve gölge eşiği okunurluğu arttırıldı. |
| Motion | **Yeniden inşa** | Menü girişinde `EaseOutBack`, basış/yaylanma; combo `×5` ve `×8` için ayrı süre ve punch beat'leri; sonuç kartında overshoot, skor sayacı ve dört istatistik kartının sıralı pop'u. |
| İçerik ekranları | **Cila** | Tek lacivert perde (`#0A121C`, %72), altın bölüm başlıkları, krem not metinleri. Kartlar perdenin üstünde net duruyor. |
| Efektler | **Cila** | Combo kademesine göre kıvılcım sayısı, halka yarıçapı ve aura parlaklığı artıyor; havuzlanmış sistem ve parçacık limiti aynı. |

## 10. Değişen dosyalar

| Dosya | Rol |
| --- | --- |
| `Assets/Scripts/UI/HomePresentation.cs` | Kahraman sahne, kaide, tema ada/landmark katmanı, ışık, kamera ve giriş animasyonu |
| `Assets/Scripts/Themes/ThemeWorldKit.cs` *(yeni)* | Beş tema için paylaşılan mesh'ler: yapılar, adalar, parapet, kaide |
| `Assets/Scripts/Themes/ThemeDistrict.cs` *(yeni)* | Oynanış gökyüzü hattı: landmark, midground, ada, moloz, küre, kaide ölçek/renk |
| `Assets/Scripts/UI/ThemePoster.cs` *(yeni)* | Kodla boyanan tema kartı posterleri |
| `Assets/Scripts/UI/MenuScrim.cs` *(yeni)* | Menü gradyan perdeleri |
| `Assets/Scripts/UI/HeroPlayPulse.cs` *(yeni)* | OYNA butonu nefes/parlama animasyonu |
| `Assets/Scripts/UI/MenuScreens.cs` | Ana menü hiyerarşisi, tema kartları, perde, başlık/not renkleri |
| `Assets/Scripts/UI/UIManager.cs` | HUD, combo animasyonu, sonuç ekranı girişi ve skor sayacı |
| `Assets/Scripts/UI/MenuIcon.cs` | OYNA ve kupa ikonları |
| `Assets/Scripts/Themes/ThemeVisuals.cs` | Carrara paleti (gökyüzü, bulut denizi) |
| `Assets/Scripts/Themes/CarraraWorld.cs` | Oynanış sahne bağlantısı, bulut denizi, bahçeler |
| `Assets/Scripts/Effects/EffectPool.cs` | Combo kademeli kutlama, aura ve halka |
| `Docs/GorselYenilemeRaporu.md` | Bu rapor |

## 11. İncelenen referanslar

Kamera hissi, kart hareketi, seçilebilir renk durumları ve mobil çizim bütçesi için yayınlanmış yapıları incelendi:

| Referans | Alınan | Alınmayan |
| --- | --- | --- |
| Cinemachine | Yumuşak takip, sönümlü impuls ve kadraj eğrisi yaklaşımı | Paket bağımlılığı; mevcut `CameraController` korundu |
| Unity UI Toolkit örnekleri | Kart giriş/çıkış zamanlaması ve easing eğrileri | UI Toolkit'e geçiş; Canvas/TMP düzeni korundu |
| Unity UI Extensions | `CanvasGroup.interactable` yerine yalnızca `blocksRaycasts` ile giriş kilitleme | Bileşen paketi |
| Boat Attack | Paylaşılan materyal/mesh, gölge bütçesi ve mobil batching alışkanlıkları | Sahne, shader ve render pipeline kopyası |

Bunlar tasarım referansıydı; kaynak kod taşınmadı.

## 12. Teslim

- Kod commit'i: `01b22c4` — "Rebuild the home, destination and run presentation"
- Dal: `main`, `https://github.com/efekanegecan1905-sys/wobble-game`
- Testler: Play Mode 27/28 (1 demo-only skip), Edit Mode 19/19.
- Oynanış, ekonomi, lokalizasyon ve akış kodunda davranış değişikliği yok; yalnızca sunum katmanı ve efekt parametreleri değişti.

## 13. Telefon demo yapısı

Sunumu gerçek cihazda göstermek için ayrı bir **demo** derlemesi üretildi. Normal uygulama ve mağaza ekonomisi bu yapıdan etkilenmez.

| Konu | Davranış |
| --- | --- |
| Demo tetikleyicisi | Yalnızca demo derlemesine gömülen `Resources/Demo/AllThemes` işaret dosyası. Depoda tutulmaz; derlemeden sonra silinir, normal oyunda demo modu kapalı kalır. |
| Temalar | Beş tema da sahip olarak açılır; kilit veya elmas fiyatı gösterilmez. Karta dokunmak temayı seçer. |
| Elmas | Cüzdanda `999999` görünür, harcama sınırsızdır (mağaza ve tema satın alma hep başarılı). |
| Can | Mevcut kayıttan gelir (5). |
| Kayıt | Demo oturumu bellekte çalışır; `PlayerPrefs`'e yazmaz. Skor/ayar değişiklikleri uygulama kapanınca sıfırlanır, mevcut kayıt bozulmaz. |
| Paket | `com.efekangokce.wobble.themedemo`, görünen ad **WOBBLE Demo**, takım `2WCK86PG29`, iOS 15.0 hedefi. Normal `com.efekangokce.wobble.game` paketine dokunulmadı. |

Bu turda cihazda görülen iki sunum hatası da düzeltildi:

- **Collider'sız primitive'ler**: `GameObject.CreatePrimitive` IL2CPP'de collider tipleri budandığı için her çağrıda "Can't add component because class '…Collider' doesn't exist" hatası basıyordu (geliştirme konsolu ekranı kaplıyordu). Zemin, bulut denizi ve efekt dörtgenleri artık `ThemeWorldKit` mesh'leriyle collider oluşturmadan kuruluyor.
- **Güvenli alan şeritleri**: içerik ekranlarında çentik ve home-indicator şeritleri perdenin dışında kalıp ham gökyüzünü gösteriyordu. Canvas'a perde renginde iki şerit eklendi; menü geçişiyle aynı sürede açılıp kapanıyor, Ana Menü'de görünmüyor.

Ayrıca yalnızca geliştirme derlemelerinde çalışan bir yakalama kancası eklendi: `WOBBLE_SCREEN=Themes|Shop|Daily|Settings` ortam değişkeni (veya `--wobble-screen=`) açılışta ilgili ekranı açar. Cihazda ekran görüntüsü almak için kullanıldı, mağaza derlemesine girmez.

Doğrulama: iPhone 17 Pro (iOS 26) üzerinde kuruldu ve açıldı; cüzdan `999999`, Temalar ekranında beş tema da açık, geliştirme konsolu hatası yok. Play Mode 27/28 (1 demo-only skip), Edit Mode 19/19.

Güvenli alan şeritleri cihaz ekran görüntüleriyle ölçüldü. Temalar ekranında üst şerit, şerit eklenmeden önce ham gökyüzü rengini (`0.51 / 0.76 / 0.87`) gösteriyordu; şeritten sonra ekran gövdesiyle aynı tona (`0.29 / 0.43 / 0.49`) indi. Alt şeritte de aynı düşüş var (`0.35 / 0.63 / 0.84` → `0.18 / 0.36 / 0.47`). Ana Menü'de şeritler şeffaf kaldığı için gökyüzü kesilmiyor.


