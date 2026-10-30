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



## 14. İkinci geçiş: tipografi, dünya seçimi ve blok malzemesi

Bu geçiş oynanışı değiştirmedi; sunum, tipografi ve yüzey okunabilirliğine odaklandı. Blok çarpışma boyutları, kesme matematiği, hareket sınırları ve mükemmel toleransı **hiç** değişmedi; yalnızca görünen mesh ve shader güncellendi.

### Tipografi sistemi

| Konu | Önce | Şimdi |
| --- | --- | --- |
| Başlık / logo / CTA | Lilita One (dar, oyuncak-harfli) | **Nunito Black** (`Assets/Fonts/Nunito-Black.ttf`) |
| Gövde / etiket / istatistik | Noto Sans + Lilita karışımı | **Inter SemiBold** (`Assets/Fonts/Inter-SemiBold.ttf`) |
| Yedek zinciri | Display → Liberation (Türkçe dışı glif riski) | Display → Inter → Liberation, üçünde de aynı karakter seti |
| Atlas | 2048² statik, 96 px örnekleme | 2048² statik, tek atlas; çalışma anında atlas üretimi yok |

- Karakter seti, yerelleştirme tablosundaki tüm metin + Latin-1 + Türkçe bloğu (`ÇĞİÖŞÜçğıöşü`) + kullanılan noktalama ve para birimlerini birlikte pişiriyor. Dil değişiminde ekranların yarısı başka bir fonta düşmüyor.
- Yeni `Wobble.Editor.WobbleFontBuilder.Build()` iki aileyi ve yedek fontu tek komutla yeniden üretiyor; atlaslar `Assets/Resources/Menu/` altındaki aynı yollara yazıldığı için mevcut referanslar korunuyor.
- Nunito Black zaten siyah olduğu için TMP'nin sahte kalınlaştırması (`FontStyles.Bold`) kaldırıldı; harfler şişmiyor, ince kenarlar net kalıyor.

### Ekran bazlı değişiklikler

- **Ana Menü:** logo 98 px + otomatik sığdırma, seçili dünya adı artık altın bir **chip** içinde (açık gökyüzünde altın yazı okunmuyordu), tagline kendi koyu plakasında, `OYNA` butonu büyütüldü (780×158) ve ikon/etiket hiyerarşisi ayrıldı, en iyi skor plakası yeniden ölçeklendi, alt menü etiketleri gövde fontuna geçti.
- **Temalar:** kart ızgarası 3 sütundan **2 sütuna** çıktı; kartlar 492 px yüksekliğinde, poster alanı büyüdü. Tek kalan kart ortalanıyor. Seçili kart accent çerçeve + üst kural + renkli zemin; kilitli kartın posteri karartılıyor ama kimliği korunuyor; fiyat ve durum aynı pill içinde.
- **HUD:** skor için yuvarlatılmış koyu plaka eklendi (hem açık saray göğü hem neon gece üzerinde okunur), skor 66 px'e çıktı, marka/seviye hizalandı.
- **Sonuç ekranı:** kart 780 → 724 px, istatistik çipleri yeni yüksekliğe göre konumlandı, ayırıcı hattı çakışmayacak şekilde taşındı; tüm rakamlar yeni fontla.
- **Bölüm başlıkları** (Mağaza/Günlük): altın yerine krem metin + accent şerit işareti.

### Bloklar (yalnızca görsel)

- `BlockView` mesh'indeki pah (chamfer) 0.8% → 2.2% içe, 4.5% → 5.25% yüksekliğe çıkarıldı. BoxCollider `Vector3.one` olarak kaldı; `Resize` aynı boyutu kullanıyor, yani çarpışma ve kesme aynı.
- `Carrara.shader` içinde: üst yüz daha parlak, yanlar biraz daha zengin; yan yüzlerde dikeyde ince bir parlaklık gradyanı; 45° pah halkasına düşen dar bir **kenar ışığı** (`_EdgeLight`, varsayılan 0.16) — ekstra geometri veya ışık yok.

### Dünya ayrımı

- Obsidian, Ancient Stone, Neon ve Space için sis mesafesi ve orta plan renkleri ayrıştırıldı: siluetler gökyüzünden ayrılıyor, alt ufuk daha koyu, harabe tonları daha okunur. Onaylı blok tonları (`previewTint`) ve geometri sayısı (`GeometryCount == 24`) korundu.

### Doğrulama

- Play Mode **27/28** (1 demo-only skip), Edit Mode **19/19**.
- `Artifacts/Menu/Previews` 30 ekran görüntüsü (5 ekran × 2 dil × 3 çözünürlük) yeniden üretildi; test her metin için taşma ve "No translation found" kontrolünü geçti.
- `Artifacts/ThemeEnvironment` beş dünya için yeniden üretildi; `Artifacts/Product/Previews` sonuç ekranı güncellendi.

## 15. Üçüncü geçiş: premium his ve yaşayan dünyalar

Amaç, anlık istifleme ve tüm sunum anlarını "premium" hissettirmekti. Oynanış sözleşmesi **hiç** değişmedi: hareket yönleri/sınırları, kesme matematiği, örtüşme, mükemmel toleransı, skor ve kombo kuralları, giriş, ekonomi, kayıt/lokalizasyon mimarisi, ses eşlemesi ve `com.wobble.game` paket kimliği aynı. Tüm yeni durum **yalnızca sunum** katmanında; hiçbir yeni alt sistem, rastgele oynanış sapması veya ekstra ekonomi yok.

### Blok yüzeyi (yalnızca render)

| Konu | Önce | Şimdi |
| --- | --- | --- |
| Görsel kalınlık | Oynanış yüksekliğiyle birebir | **1.12×** görsel yükseklik (`.4 → ≈ .448`); collider ve yerleşim yüksekliği `.4` |
| Pah / kenar | %2.2 içe pah | Daha derin pah + üst yüz/yan yüz ayrımı, alt temas gölgesi, güçlendirilmiş kenar ışığı |
| Vuruş (punch) | Yok | **105 ms**, tepe **1.2–1.8%** ölçek; yalnızca görsel kök, collider'a dokunmaz |
| Mükemmel | Tek kıvılcım | Kısa parlak süpürme (sweep) + daha geniş halka |
| Eğilme | Yok | Merkez dışı kesimlerde **1.4–4.2°** sönümlü lean (görsel çocuk) |

`StackManager` boyutu, `BoxCollider.size == Vector3.one` ve `view.Size.y == .4` korunur; testler bunu doğrular.

### Yerleşim ve Perfect geri bildirimi

- Normal yerleşim sakin: temas halkası + kısa kamera impulsu; ekran sarsıntısı yok.
- Perfect belirgin şekilde daha iyi: daha geniş halka, süpürme, kısa güçlü impuls ve `MÜKEMMEL` etiketi.
- Kombo kırıldığında parlama **~0.42 s** içinde yumuşakça sönüyor; ani kapanma yok.
- Tüm efektler `EffectPool` üzerinden havuzlu; yerleşim başına `Instantiate`/`Destroy` yok.

### FLOW / FEVER

- Yeni `ComboEnergy` (yalnızca sunum): `DisplayMultiplier` 5 → **FLOW**, 8 → **FEVER**. Tam aralık saldırısı **~0.16 s**, bırakış **~0.42 s**.
- HUD'da üst-orta **AKIŞ / COŞKU** çipi: koyu plaka, geniş harf aralığı, tema aksanına hafif ısınan metin. x5 sakince yükseliyor, x8 kahraman anı ama kaos değil.
- Kombo enerjisi kamera, blok kenarı, dünya ışığı ve parçacıklara yayılıyor; skor/kombo matematiğine girmiyor.

### WOBBLE / STABILIZED

- İlk **x8 Perfect**'te kule "gerilir": her dilim kendi merkezi etrafında **< 1°** salınır (yalnızca görsel), alt-orta **SALLANIYOR** çipi belirir.
- Gerilimde gelen ilk Perfect **stabilize eder**: `STABİLİZE` anı, sönümlü salınım, toz halkası. Ekonomi ödülü **yok**; skor yalnızca normal yerleşim puanını alır.
- WOBBLE sırasında collider, kesme ve sıradaki yerleşim tamamen sabit.

### Tema dünya reaksiyonları

| Tema | Kombo/Perfect reaksiyonu |
| --- | --- |
| **Carrara** | Altın kenar ışığı, bulut denizinde yumuşak dalga, mercan vurgular |
| **Obsidian** | Kor turuncusu kenarlar, lav parlaması, dumanlı toz |
| **Ancient Stone** | Güneş altını, toz bulutu, sıcak kum tonu |
| **Neon** | Camgöbeği → magenta kovalayan şerit, zemin halkası nabzı |
| **Space** | Yıldız kırpışması, halka dalgası, nebula tonu |

Reaksiyonlar `ThemeManager`, `CarraraWorld` ve `ThemeDistrict` içinde mevcut yapılar genişletilerek yapıldı; `Environment` sözleşmeleri ve `GeometryCount == 24` korunuyor.

### Fail ve sonuç

- Başarısızlıkta kamera kısa ve güçlü bir impuls alır, toz/halka patlar; sonuç arayüzü **0.72 s** sonra açılır (0.6–1.2 s aralığı).
- Kart overshoot ile gelir, skor sayılır, istatistik çipleri sırayla açılır; **YENİ REKOR** altın plakası ve sıcak kart tonu yalnızca rekor kırıldığında görünür.
- Skor, en iyi, blok, mükemmel, maks kombo, elmas, tekrar dene / ana menü / paylaş / devam butonları korunur; butonlar reveal bitmeden tıklanamaz.

### Ana Menü mikro-hareket

- Home yeniden inşa edilmedi: logo nefesi, bulut sürüklenmesi, ada salınımı, ara sıra altı harfte imza parlaması, dokunuşta tactile basış ve ekran geçişleri eklendi.

### Tipografi ve HUD okunurluğu

- İki aile korunuyor: **Nunito Black** (display) + **Inter SemiBold** (gövde); yedek zincir aynı karakter setini taşıyor.
- `TypographyCoverageTests` üç TMP varlığında `A-Z a-z 0-9 ÇĞİÖŞÜ çğıöşü` + kullanılan noktalama için glif kapsamasını ve iki ailenin farklı olduğunu doğrular; `₺` kullanılmadığı için kapsam dışı.
- Küçük puntolar büyütüldü (en iyi etiketi, alt menü, tema alt yazı/durum/fiyat, günlük ödül, notlar, ayarlar satırları); HUD'a marka ve skor için simetrik koyu plakalar eklendi, böylece güneş/ay diski veya bulut bankı arkasında bile "SEVİYE n" okunur.
- Tüm yakalama testleri her metin için `isTextOverflowing == false` ve "No translation found" yok kontrolünü geçiyor.

### Kamera / haptik / VFX

- Kamera yalnızca mikro davranış: yerleşim impulsu, FLOW nabzı, x8'de kısa hero roll, fail'de güçlü ama kısa impuls. Kadraj ve hassasiyet değişmedi.
- Haptikler mevcut eşlemeyle sınırlı; yeni yoğun post-process yok.

### Performans

- `CarraraWorld.GeometryCount == 24` sabit, sahne collider'ı yok.
- Ambient parçacık limiti **≤ 36**, tüm efektler havuzlu, mesh/materyal paylaşımlı.
- Yerleşim yolunda LINQ yok, yerleşim başına `Instantiate`/`Destroy` yok; ağır post-process eklenmedi.

### Doğrulama

- **Edit Mode: 27/27** (yeni `ComboEnergyTests` + `TypographyCoverageTests` dahil).
- **Play Mode: 29 geçti / 0 başarısız / 1 atlandı** (atlanan yalnızca demo derlemesine özel `AllThemesDemoCanSelectAndPlayEveryTheme`).
- Yeni `FeelPassCaptureTests`; FLOW x5, FEVER + WOBBLE x8, STABILIZED ve YENİ REKOR kartını üç çözünürlükte üretir, collider/ekonomi sözleşmelerini doğrular; ayrıca Obsidian / Ancient Stone / Neon / Space için x8 FEVER kareleri alır.
- Yenilenen kanıtlar: `Artifacts/Feel/Pass`, `Artifacts/Previews`, `Artifacts/ThemeEnvironment`, `Artifacts/Product/Previews`, `Artifacts/Menu/Previews`.
- Bundle ID `com.wobble.game` (iOS + Android) değişmedi.

### Bu turda değişen dosyalar

| Dosya | Rol |
| --- | --- |
| `Assets/Scripts/Gameplay/ComboEnergy.cs` *(yeni)* | FLOW/FEVER sunum durumu, saldırı/bırakış zamanlaması |
| `Assets/Scripts/Gameplay/BlockView.cs` | 1.12× görsel mesh, punch, lean, dilim başına WOBBLE salınımı |
| `Assets/Resources/Shaders/Carrara.shader` | Üst/yan ayrımı, temas gölgesi, Perfect süpürmesi |
| `Assets/Scripts/Gameplay/StackManager.cs` | Gerilim başlatma/stabilize, efekt beslemesi (oynanış matematiği değişmedi) |
| `Assets/Scripts/Core/GameManager.cs` | Enerji akışı, x8 gerilimi, yeni rekor, 0.72 s fail gecikmesi |
| `Assets/Scripts/Effects/EffectPool.cs` | Temas/Perfect halkaları, aura, stabilize ve fail efektleri |
| `Assets/Scripts/Camera/CameraController.cs` | Yerleşim/FLOW/fail impulsları, hero roll |
| `Assets/Scripts/Themes/ThemeManager.cs`, `CarraraWorld.cs`, `ThemeDistrict.cs` | Tema bazlı kombo/dünya reaksiyonları |
| `Assets/Scripts/UI/UIManager.cs` | AKIŞ/COŞKU, SALLANIYOR/STABİLİZE, YENİ REKOR, plakalar, punto ve kontrast |
| `Assets/Scripts/UI/MenuScreens.cs`, `HomePresentation.cs` | Küçük punto düzeltmeleri, Home nefesi ve imza parlaması |
| `Assets/LocalizationAuthoring/Wobble.tsv` + `Assets/Localization/*.asset` | `hud.flow`, `hud.fever`, `hud.wobble`, `hud.stabilized`, `result.newbest` (87 anahtar × 2 dil) |
| `Assets/Editor/LocalizationSync.cs` *(yeni)* | TSV → String Table hedefli senkron + glif doğrulaması |
| `Assets/Editor/ComboEnergyTests.cs`, `TypographyCoverageTests.cs` *(yeni)* | Sunum enerjisi ve tipografi sözleşmeleri |
| `Assets/Tests/PlayMode/FeelPassCaptureTests.cs` *(yeni)* | Premium an yakalamaları + collider/ekonomi doğrulaması |
| `Docs/GorselYenilemeRaporu.md` | Bu bölüm |
