# WOBBLE — Carrara menü ve yerelleştirme raporu

Tarih: 10 Ekim 2026. Unity: **6000.0.81f1**. Bu çalışma yalnızca menü, yerelleştirme, yerel kayıt ve istenen ekonomi hazırlıklarını kapsar.

## 1. Referanslardan kullanılan fikirler

Kod yazılmadan önce iki deponun ilgili kaynak dosyaları incelendi; depo klonlanmadı, kaynak kod aktarılmadı.

- [Laputa mobile gamebase](https://github.com/Laputa-Unity/unity-mobile-gamebase/tree/4e95f9f91ba9fa44d089f21ba803043214d96eff): `ItemConfig`, `DailyRewardConfig`, `DailyRewardController`, `PopupController`, `PlayerDataController`, PlayerData'nın Shop/Setting/DailyReward parçaları, `EncryptionHelper`, Shop ve Settings panelleri. Uyarlanan fikirler: ScriptableObject yapılandırması ile oyuncu verisinin ayrılması, ödül sırası + son alım zamanı, kayıt değişikliği bildirimi, panel/modal sorumluluklarının ayrılması.
- [Team-on UnityGameTemplate](https://github.com/Team-on/UnityGameTemplate/tree/bfcbac80d4d384c347e4a0ca9ecc68fed4690556): `MenuManager`, `MenuBase`, `TemplateMainMenu`, `TemplateSettingsMenu`, `TemplateSelectLanguageScreen`, `SelectLanguageTab`, `AudioTab`. Uyarlanan fikirler: açık ekran yönlendirmesi, geri/modal önceliği, geçiş sırasında etkileşim kilidi, kısa CanvasGroup geçişi, dillerin kendi adlarıyla sunulması ve ses/müzik ayrımı.

## 2. Bilinçli olarak alınmayan fikirler

Singleton/DontDestroyOnLoad ağı, global Data/Observer mimarisi, havuz/tween/touch bağımlılıkları ve şablonların ProjectSettings dosyaları alınmadı. WOBBLE'ın mevcut yöneticileri korundu. Polyglot ve ilk açılışta zorunlu dil seçimi kullanılmadı; istenen başlangıç dili Türkçe.

Laputa'nın sabit gömülü anahtarla şifrelenmiş JSON'u, gerçek güvenlik veya kurcalamaya karşı koruma sağlamayacağı ve mevcut kayıtlarla ikinci bir depolama sistemi oluşturacağı için kopyalanmadı. Mevcut SaveManager genişletildi. Gameplay, Lucky Spin, Spine, Firebase, reklam/IAP SDK'ları, giriş, hesap, bulut kayıt ve liderlik tablosu eklenmedi.

## 3. Oluşturulan dosyalar

Proje kökü: `/Users/efekangokce/Documents/ChatGPT/wobble block`.

- `Assets/Scripts/UI/`: `HomePresentation.cs`, `MenuScreens.cs`, `MenuNavigation.cs`, `MenuLocalization.cs`, `MenuIcon.cs`, `MenuPlayerReview.cs`.
- `Assets/Scripts/Economy/`: `MenuCatalog.cs`, `DailyRewardService.cs`.
- `Assets/Editor/`: `MenuAssetBuilder.cs`, `MenuPlayerBuilder.cs`, `MenuDataTests.cs`.
- `Assets/Tests/PlayMode/MenuPlayModeTests.cs`.
- `Assets/LocalizationAuthoring/Wobble.tsv` — çeviri yazım kaynağı; runtime sözlük değildir.
- `Assets/Localization/`: ayarlar, iki locale, Wobble koleksiyonu, Shared Data ve iki String Table.
- `Assets/Resources/Menu/`: `MenuCatalog.asset`, `CarraraPreview.mat`, `MenuUI.mat`, `WobbleLocalizedSans.asset`, `CurrencyFallback.asset`. UI materyalinin açık Resources referansı, çalışma anında oluşturulan düğmelerin shader'ının player build'den çıkarılmasını önler.
- `Assets/AddressableAssetsData/`: Unity'nin yerel localization grupları, şemaları ve build ayarları.
- `Assets/Fonts/NotoSans.ttf`, `NotoSans-OFL.txt`: yalnızca Liberation Sans'ta bulunmayan ₺ karakteri için lisanslı fallback kaynağı. Kaynak: [Google Fonts / Noto Sans](https://github.com/google/fonts/tree/main/ofl/notosans).
- `Artifacts/Menu/`: test XML'leri ve iki dil × beş ekran × üç çözünürlük = 30 ön izleme.
- Bu rapor ve yukarıdaki Unity dosyalarının `.meta` dosyaları.

## 4. Değiştirilen dosyalar ve kapsam koruması

- `SaveData.cs`, `SaveManager.cs`: geriye uyumlu kayıt alanları, değişim bildirimi, test edilebilir saklama arabirimi.
- `EconomyManager.cs`: kalıcı canlar, ödül ekleme ve pasif continue/purchase hazırlıkları.
- `AudioManager.cs`: aynı yönetici içinde ayrı müzik kaynağı ve tercih. Mevcut efekt üretimi aynı.
- `UIManager.cs`: eski Home yerine yeni menü bağlantısı; HUD/Game Over metinleri String Table'a taşındı; pasif DEVAM ET düğmesi.
- `GameManager.cs`: menü bağımlılıklarını verme, kayıtlı grafik/müzik tercihini uygulama; artık menüden doğrudan kullanılan eski titreşim olay bağlantısı kaldırıldı.
- Runtime/test `.asmdef` dosyaları: Localization/ResourceManager referansları. `WobblePlayModeTests.cs`: localization hazırlığını bekleme ve Home için ayrı menü kamerasını kullanma.
- `Packages/manifest.json`, `Packages/packages-lock.json`.
- `ProjectSettings/EditorBuildSettings.asset`: yalnızca Addressables ve Localization config referansları. Sahne listesi aynı; diğer ProjectSettings dosyaları değiştirilmedi.

Görev öncesi kopyayla karşılaştırıldı: `Assets/Scripts/Gameplay/` altındaki hareket, kesme, StackManager, kombo ve seviyeler **birebir aynı**. `Assets/Scripts/Camera/`, Carrara rendering kaynakları ve shader'ları da aynı. StartGame/OnPlaced/OnLevelUp/OnMissed/GoHome oyun davranışları korunur. Menü, 1000 birim uzağa yerleştirilen ayrı sunum ve ayrı kamerayı kullanır; oyun sırasında sunum kapatılır ve mevcut oyun kamerası tekrar açılır.

## 5. Eklenen paketler

Doğrudan eklenen paket yalnızca `com.unity.localization`. Unity Package Manager'ın çözdüğü başlıca geçişli bağımlılıklar: Addressables **2.9.1**, Newtonsoft JSON **3.2.2**, Scriptable Build Pipeline **2.6.1** ve gerekli Unity asset-bundle/web-request modülleri. Bunlar yerel dil tablolarının paketlenmesi içindir; çevrimiçi servis entegrasyonu yapılmadı. Tam sürümler lock dosyasında sabittir.

## 6. Localization sürümü

`com.unity.localization` **1.5.13**. Unity 6000.0 için yayımlanmış sürüm [Unity 6 resmi paket sayfasından](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.localization.html) doğrulandı; rastgele en yeni sürüm kurulmadı. Testler ve derleme tam olarak 6000.0.81f1 ile çalıştırıldı.

## 7. Locale asset'leri

`tr.asset`, `en.asset`, `WobbleLocalization.asset`. Specific Locale Selector ve Project Locale Türkçe. Manuel seçim SaveManager'a yazılır, sonraki açılışta geri yüklenir. Cihaz dilinin Türkçe varsayılanı değiştirmesine izin verilmez.

## 8. String Table'lar

`Wobble.asset` koleksiyonu, `Wobble Shared Data.asset`, `Wobble_tr.asset`, `Wobble_en.asset`. İstenen tüm anahtarlar; ek olarak ürünler, tema adları, modal açıklamaları, günlük ödüller ve HUD/Game Over metinleri bulunur. Sayısal alanlar resmi Smart Strings parametrelerini kullanır. Her iki tablo ön yüklenir; UI içinde özel çeviri sözlüğü veya dil koşullu görüntü metinleri yoktur.

Çeviri güncellemesi: TSV'yi düzenleyip Unity'de `WOBBLE > Menu > Configure Menu Assets` çalıştırılır. Ürün fiyatları/ödülleri `MenuCatalog.asset` üzerinden düzenlenir; yapılandırıcı mevcut kataloğu sıfırlamaz.

## 9. Menü ve yönlendirme

`UIManager` mevcut oyun olaylarını korur. `MenuScreens` Home, Themes, Shop, Daily ve Settings ekranlarını oluşturur. `MenuNavigation` açık bir ekran kaydı ve 0.24 saniyelik fade + küçük ölçek geçişi uygular. Geri, önce modalı kapatır; alt ekrandan Home'a döner. Sahne yeniden yüklenmez.

Home: altı yavaş dönen mevcut Carrara slab, ayrı kamera, mevcut ışık/sis/malzeme kimliği, tek OYNA, ikincil en iyi skor, küçük para/can göstergeleri ve çizgisel ikonlu dört alt eylem. Eski Home tasarımı kaldırıldı. Yeni bitmap/AI görsel, dashboard kartları veya pembe tasarım kullanılmadı.

## 10. Kayıt modeli

Mevcut `wobble.save.v1` anahtarı korunur; veri şeması 2. Skor, elmas, can, seçili/sahip olunan temalar, ses, müzik, titreşim, kalite, locale ve günlük ödül sırası/zamanı saklanır. Eski JSON, yeni varsayılanların üzerine yüklenir; eksik yeni alanlar sıfıra düşmez. Bozuk JSON için kurtarma kopyası ayrılır. Ham PlayerPrefs erişimi üretim kodunda SaveManager'ın saklama katmanında toplanır; ayrı save manager yoktur.

Yeni kayıtta 0 elmas, 5 can, Carrara, Türkçe ve yüksek kalite vardır; kullanıcının mevcut elmas/skoru korunur. Müzik tercihi ayrı, ses efektleri ayrı yönetilir.

## 11. Ekonomi ve mağaza hazırlıkları

ScriptableObject ürünleri: 10 can ₺19.99; 50 can ₺59.99; 250 elmas ₺19.99; 750 elmas ₺49.99; 2000 elmas ₺99.99; başlangıç paketi ₺39.99; reklamları kaldır ₺79.99. Başlangıç paketi 500 elmas + 20 can + gelecekte özel görünüm kimliği içerir.

Fiyatlar oyun mantığında değildir. Ürün detayları açılır ancak satın alma düğmesi pasiftir; para alınmaz ve ürün verilmez. `CanContinueWithLife`, `ConsumeLifeForContinue`, `ContinueWithLife`, `ContinueWithRewardedAd` hazırlanmıştır. Son iki yöntem şu anda false döner ve can harcamaz. Yalnızca açık tüketim çağrısı can azaltabilir; normal OYNA/TEKRAR DENE bunu çağırmaz. Game Over'daki DEVAM ET pasiftir.

## 12. Günlük ödül

7 alımlık döngü: 20 elmas; 1 can; 30 elmas; 1 can; 50 elmas; 2 can; 100 elmas + 3 can. Ödül ile son alım damgası birlikte kaydedilir. Aynı gün çift alım ve saati geri alma engellenir. Kaçırılan günler döngüyü sıfırlamaz; sıradaki ödülden devam edilir.

Gün sınırı UTC gece yarısıdır ve UI bunu açıklar. `IRewardClock` daha sonra sunucu saatine bağlanabilir. Günlük ekran açıkken gün değişimi de yenilenir. Şu anda cihaz saatini ileri almak engellenemez; backend veya anti-cheat iddiası yoktur.

## 13. Temalar

Carrara seçili/açık; Obsidian, Ancient Stone, Neon ve Space kilitli gelecek içerik. Her tanım id, String Table anahtarı, ön izleme material alanı, renk ve elmas fiyatı içerir. Sahiplik/seçim değişken oyuncu kaydında tutulur; sabit katalogla karıştırılmaz. Yalnızca Carrara'nın gerçek material ön izlemesi vardır; diğer material alanları ve fiyatlar geleceğe hazırlıktır. Yeni oynanış temaları uygulanmadı.

## 14. Türkçe/İngilizce ve regresyon testleri

- Edit Mode: **9 WOBBLE testi geçti** (6 yeni menü/veri + 3 mevcut kesme); runner'ın raporundaki 10. test Addressables paketinin kendi örnek testidir.
- Play Mode: **17/17 geçti**. Türkçe varsayılan, English seçimi ve yeniden oluşturulan uygulamada geri yükleme; tüm menü yolları; ses/müzik/titreşim/kalite; ödülün tek alımı; mağazanın işlem yapmaması; ücretsiz OYNA/TEKRAR DENE ve HOME dönüşü doğrulandı.
- Mevcut dört yön, kesme, çoklu sınır dönüşü, seviye/kombo/skor ve rendering regresyonları aynı test turunda çalıştı.
- **1080×1920, 1170×2532, 1080×2400**: iki dilde beş ekranın tamamı render edildi. Metin taşması/eksik çeviri kontrolleri ve ikonların gerçek piksel görünürlüğü test edildi. 30 PNG `Artifacts/Menu/Previews` içinde.
- Mac development player doğrulaması **geçti**: uygulamaya paketlenmiş TR/EN tabloları, anlık dil değişimi, kayıtlı locale, ücretsiz OYNA ve HOME dönüşü. Sonuç `Artifacts/Menu/PlayerVerification.txt`; uygulamadan alınan iki ekran görüntüsü aynı klasörde. Test uygulaması: `/private/tmp/wobble-menu-player/WOBBLE.app`.

## 15. Bilinçli olarak kalan hazırlıklar

Gerçek ödeme, reklam kaldırma satın alımı, ödüllü reklam, canla devam, özel skin ve diğer dört temanın materyalleri uygulanmadı. Müzik kanalı ve tercih çalışır; projede soundtrack olmadığı için gerçek müzik parçası eklenmedi ve ayarlarda bu belirtilir. Season Pass, hesap, giriş, liderlik tablosu, bulut, Firebase ve bildirim eklenmedi.

## 16. Uyarılar ve sınırlar

Bu masaüstü doğrulamasıdır; gerçek iOS/Android cihazda dokunma, titreşim, güvenli alan ve performans testi yerine geçmez. Kayıt yereldir ve şifrelenmiş/kurcalamaya dayanıklı değildir. Placeholder fiyatlar mağaza platformundan alınmış gerçek satış fiyatları değildir.

Eski koddan kalan derleyici uyarıları: `GameManager.audio` alanının eski `Component.audio` adını gölgelemesi ve `ProjectBuilder` içindeki iki eski `SetApplicationIdentifier` çağrısı. Kapsam dışı oldukları için değiştirilmedi. İlk üretimdeki font/Addressables bağlantı sorunları, ikon çizimi ve standalone UI shader stripping sorunu bu çalışma sırasında düzeltildi; son test sonuçları esas alınmalıdır.
