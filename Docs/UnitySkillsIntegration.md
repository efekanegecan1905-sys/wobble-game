# Unity Agent entegrasyonu — 18 Kasım 2026

Başlangıç: `feature/arcade-modes`, `368d4af`. Unity `6000.0.81f1`, uGUI `2.0.0`, URP `17.0.4`. Dört modun kuralları ve `com.wobble.game` korunmuştur.

## Kurulum

Resmî [Unity Agent Plugin](https://github.com/Unity-Technologies/unity-agent-plugin) README komutlarıyla `unity@unity-agent-plugin` `0.1.10-beta` kuruldu; Codex yapılandırmasında `enabled=true` doğrulandı. Beceriler tek `using-unity` girişinin referansları olarak gelir; yedi ayrı kopya kurulmadı. Yeni sohbetlerde otomatik keşif için Codex yeniden başlatılabilir; bu oturumda kurulu rehberler doğrudan okundu.

Unity CLI `1.0.0-beta.8` → `1.0.1` güncellendi. Canlı Editor incelemesi ve testler için resmî `com.unity.pipeline` `1.0.0` eklendi. Bunun zorunlu bağımlılıkları lock dosyasındadır. Runtime Pipeline build sembolleri açılmadı; IAP, reklam veya başka oyun sistemi paketi eklenmedi.

## Kullanılan beceriler ve bulgular

| Beceri | Uygulama |
| --- | --- |
| `ui-ugui` | Mevcut Canvas korundu. Mod/seviye/sonuç içerikleri güvenli ekran alanına sığar; çözünürlük değişiminde Canvas render öncesi ölçüm yenilenir. Alt açıklamalar, giriş hareketi, opak arka plan ve kilitli seviyelerde pasif butonlar eklendi. Dekoratif metinler raycast engellemez. |
| `optimize-text-mesh-pro` | Canlı font sorgusu `■`, `□`, `≥` karakterlerinin üç atlasın hiçbirinde bulunmadığını doğruladı. Balance grafik dolguya, Trials minimum genişlik ifadesi desteklenen metne dönüştürüldü. HUD kontrast zemini eklendi. Sayaç AutoSize kapalı. Üç statik 2048×2048 atlas, tek doku/atlas ve scale=1 korunuyor; ek atlas/bellek tasarrufu iddiası yok. |
| `urp-postprocessing` | Canlı oyun kamerasında HDR, post-processing ve katman 0 global Volume doğrulandı. CarraraPost ACES, exposure .16, contrast 11, saturation 24 ve bloom .22 korunuyor. Kart kameralarında HDR/MSAA, gölge, depth/color kopyaları kapalı; far clip 20. Önizlemelerde siyah kare yerine şeffaf zemin. Yeni pahalı efekt eklenmedi. GPU kazancı cihazda ölçülmedi. |
| `physics-3d-collision` | Güncel pakette adı `fix-3d-physics-collisions`; rehber minimum 6000.3 belirtiyor. Proje yükseltilmedi. Unity 6000.0 çarpışma dokümanı ve mevcut kod incelendi. Balance deterministik offset hesabı ve yalnızca görsel çocuk eğimi kullanıyor. PlayMode testi collider merkez/boyut/dönüşünün değişmediğini ve Classic'e dönüşte skorun korunduğunu doğrular. PhysX ayarlarını değiştirecek doğrulanmış hata bulunmadı. |
| `optimize-audio` | Kullanıcı inceleme onayı sonrası gerçek build sahnesi denetlendi: 1 dinleyici, 10 efekt + 1 müzik kaynağı; tümü 2D, 5 mixer grubu, yalnız attenuation ve iki katmanlı routing. Sekiz onaylı combo WAV mono/44.1 kHz/PCM/DecompressOnLoad/preload; süreler .113–.300 sn. Ses kimliği ve importer ayarları değiştirilmedi. |
| `implement-in-app-purchases` | Paket/native/üçüncü taraf taraması yapıldı; gerçek satın alma sistemi yok. Aşağıdaki plan hazırlandı, ödeme açılmadı. |
| `unity-cli` | Canlı Editor C# sorguları, derleme, EditMode/PlayMode testleri, konsol kontrolü ve mevcut ekran yakalama harness'i kullanıldı. |

Ses incelemesindeki ilk iki dinleyici, boş Editor sahnesinin varsayılan kamerası + runtime kameradan kaynaklandı. Build listesinde bulunan `Assets/Scenes/Wobble.unity` açıldığında sayı 1 oldu; bu nedenle oyun kodunda dinleyici silinmedi. Editor ölçümü 48 kHz, DSP 1024×4; bu cihazdaki dokunma-ses gecikmesinin ölçümü değildir. Kullanıcının son talimatına göre yalnız doğrulanmış sorun düzeltileceği için tampon değiştirilmedi.

Okunan resmî referanslar: yedi becerinin ana dosyaları; `unity-pipeline.md`; CLI `integration-advanced.md`, `build-run-test.md` test bölümü, `diagnostics-maintenance.md` self-update bölümü, `playmode-verification-loop.md`; uGUI `FontAssetsFallback.md`, `TMPObjectUIText.md`; ses `audio-import-api.md`, `platform-settings.md`; IAP `pre-check.md`, `path-add-iap-to-new-project.md`. [Unity 6000.0 collision detection](https://docs.unity3d.com/6000.0/Documentation/Manual/choose-collision-detection-mode.html) sürüm kontrolü için kullanıldı.

## Ödeme entegrasyon planı — henüz uygulanmadı

Mevcut `MenuCatalog` tek ürün kaynağı olarak korunmalı. Fiyatlar yer tutucudur; UI satın alma penceresi “yakında” gösterir. `EconomyManager` satın alma metotları boş, rewarded-ad devamı kapalı. `SaveManager` JSON'u `ISaveStore` üzerinden PlayerPrefs'e yazar; ödeme işlem defteri ve sunucu doğrulaması yoktur.

| Mevcut katalog kimliği | Ödül | Gelecekte önerilen tür |
| --- | --- | --- |
| lives10 / lives50 | 10 / 50 can | Consumable |
| gems250 / gems750 / gems2000 | 250 / 750 / 2000 elmas | Consumable |
| starter | 500 elmas + 20 can | Tek seferlik paket mi, tekrar alınabilir mi ürün kararı gerekli |
| removeAds | removeAdsOwned | NonConsumable; reklam sistemi olmadığı için şimdilik satış dışı |

1. Ayrı bir onaylı aşamada mağaza ürün kimliklerini kesinleştir; mevcut kısa katalog kimliklerini doğrulanmış App Store ürünleriyle eşleştir. Kalıcı bundle ID değişmez. Unity 6.0 ile uyumlu güncel kararlı IAP sürümü o aşamada doğrulanır ve Package Manager üzerinden eklenir.
2. Mevcut servis yapısına tek satın alma servisi ekle. Başarı, hata, deferred ve bağlantı olaylarını bağladıktan sonra mağazaya bağlan; fiyatları mağazanın yerelleştirilmiş metadatasından göster. Başlatılıyor/ürün yok/bekliyor/iptal/hata durumlarında buton davranışı açık olmalı.
3. Sipariş doğrulama ve benzersiz işlem kimliği kontrolü ardından ödül + işlenmiş işlem kaydı birlikte kalıcılaştırılır; ancak başarılı kalıcı kayıttan sonra confirm edilir. Aynı callback tekrar gelirse ödül ikinci kez verilmez. Deferred/failed işlem ödül vermez. Mevcut `GrantReward` tek başına bu sözleşmeyi sağlamaz; kayıt sınırı ve başarısızlık dönüşü genişletilmelidir. [Unity purchase flow](https://docs.unity.com/en-us/iap/purchases)
4. Sanal para için sunucu doğrulaması ve kalıcı sipariş defteri planla. PlayerPrefs'i ödenmiş bakiyenin güvenilir otoritesi sayma. Apple StoreKit 2 işlem verisini sunucuda doğrulama yolu kullan; istemciye mağaza sırrı koyma. [Unity receipt validation](https://docs.unity.com/en-us/iap/receipt-validation)
5. Kalıcı haklar için geri yükleme UI ve açılışta sahiplik kontrolü ekle. Tüketilmiş elmas/can paketleri restore sırasında yeniden verilmez. Trials ilk-temizleme ödülü ve Rush'ın ücretsiz hata sayısı satın alma akışından ayrı kalır.
6. Sandbox test matrisi: iki kez callback, grant sonrası/confirm öncesi kapanma, kayıt hatası, ağ kesilmesi, deferred onay, iptal, ürün bulunamaması, restore ve yeniden kurulum. Gerçek ödeme yalnız bu testler ve ayrı kullanıcı onayından sonra açılır.

## Doğrulama ve kalan işler

- EditMode: 31/31 geçti.
- Hedefli ArcadeIntegrationTests: 5/5 geçti. Yeni kontroller: TR/EN kart ve seviye metinleri atlaslarda mevcut, metin taşması yok, portre/tablet butonları viewport içinde; Balance eğimi collider'ı etkilemiyor.
- Tam PlayMode: 34 geçti, 0 hata, 1 beklenen atlama; toplam 35, 145.31 saniye. Atlanan `AllThemesDemoCanSelectAndPlayEveryTheme` yalnız ayrı tüm-temalar demo build'ine aittir. Son konsolda 0 hata ve derleme hatası yok. QA için açılan Editor kapatıldı; auto-tick kapatıldı.
- Gerçek Unity render yakalamaları `Artifacts/Arcade/`: dört mod, TR/EN seçim ve seviye ekranları; 1080×1920, 1080×2340 ve 1536×2048. Bunlar Editor yakalamalarıdır; fiziksel iPhone kanıtı değildir.
- Bu aşamada yeni iOS uygulaması kurulmadı. iPhone'da çentik/safe-area, uzun süreli FPS/ısı ve dokunma-ses gecikmesi ölçümü hâlâ gerekli. Dört önizleme kamerasının GPU maliyeti ayrıca ölçülmeli.
- Mod kuralları, puanlar, ekonomi, ses bankası, global Carrara görüntü profili ve kalıcı bundle ID değişmedi; main'e merge yapılmadı.
