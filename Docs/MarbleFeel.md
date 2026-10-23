# WOBBLE — mermer temas hissi, tempo ve atmosfer

11 Ekim 2026 · Unity 6000.0.81f1

Kapsam: mevcut mimari içinde ses/temas, istenen hız ayarı ve Carrara çevresi. Yeniden oyun kurulmadı. Menü, ekonomi, kayıt ve localization kaynakları değiştirilmedi. `MovingBlock`, `BlockCutter`, `ComboManager`, tek dokunuş kontrolü ve `CameraController` görev öncesi dosyalarla karşılaştırılarak korundu. Perfect toleransı **0.11**, kesme kuralları ve puan hesabı aynı.

## 1. Başlangıç hızı

**4.5 → 4.0**. Yeni oyun ve tekrar deneme aynı başlangıç değerini kullanır.

## 2. Yerleştirme başına hız artışı

**+0.14 → +0.08**. Başarılı normal/perfect yerleştirmeler aynı fiziksel artışı alır. Başarısız yerleştirme artış üretmez.

## 3. Maksimum hız

**8.0 → 6.6**. Her artış bu tavanda sınırlandırılır. 500 yerleştirmelik test tavanı, tabanı ve tüm seviye geçişlerini denetler.

## 4. Seviye geçişindeki rahatlama

Her **10 blokta bir** seviye geçişi aynı kaldı. Önceden hızdan sabit **0.65** çıkarılıyordu; şimdi artış/tavan uygulandıktan sonra hız **×0.90** olur, başlangıç hızının altına inmez. Sonraki yerleştirmeler +0.08 ile yeniden yükselir. İlk üç geçiş sonrasındaki hızlar yaklaşık **4.32 / 4.608 / 4.867**. Kombo hiçbir hız girdisi üretmez.

## 5. Ses mimarisi

Mevcut `AudioManager` genişletildi; ikinci yönetici veya dış ses SDK'sı yok.

- Yerleştirme: baskın 158–573 Hz ağırlıklı inharmonik taş gövdesi, çok düşük seviyeli filtrelenmiş temas, neredeyse duyulmaz kısa ortam kuyruğu. Gövde 240 ms, temas 90 ms, kuyruk 320 ms. Başlangıç/bitiş zarfları keskin tıklamayı azaltır.
- PERFECT: aynı gövde biraz daha güçlü; 132/264/396 Hz ağırlıklı düşük seviyeli destek. x2, x3, x4, x5+ için ton katmanı çok az artar; fiziksel sesin perdesi kombo ile yükselmez. Seviye geçişinde ayrıca başarı bip'i çalınmaz.
- Kesilen parça: küçük bir sürtünme/chip sesi. Havuzdaki parça yeniden etkinleştiğinde ilk anlamlı fizik çarpışması için kurulur. En az 0.1 saniye sonra, bağıl hız 0.65 üzerindeyse bir kez daha sessiz gövde sesi tetiklenir. Gerçek çarpışma olmadan sahte bir yere düşme sesi üretilmez.
- İki adet dört katmanlı yerleştirme grubu + iki ikincil kanal: **en fazla 10 efekt sesi**, ayrıca mevcut müzik kaynağı. Sınırsız PlayOneShot yığılması yok. İkincil seslerde 120–160 ms bekleme sınırı var.
- Katmanlar aynı DSP zamanına (+6 ms) programlanır; darbe, toz ve görsel temas aynı yerleştirme olayından beslenir. Kısa mono PCM klipler ön yüklenir; yerleştirmede dosya okuma/klip üretimi yapılmaz.
- Efekt kazanç payı 0.65. En kötü teorik iki darbe + iki ikincil ses + tam tepe müzik toplamı yaklaşık **0.922** altında tutulur. Matematiksel peak/headroom testi var; gerçek telefon hoparlörü dinleme testi yerine geçmez.

## 6. Varyasyon sayısı

**5 ayrı gövde + 5 ayrı temas varyasyonu**, 4 PERFECT/kombo destek klibi, 1 ortam kuyruğu ve 1 sürtünme: toplam **16 WAV**. `MarbleImpact_01`…`05` arka arkaya aynı seçilmez. Gövde perdesi **±%3**, ses seviyesi **±%2** değişir. Kendi RNG'si kullanılır; oyunun rastgele sayı dizisine ses nedeniyle müdahale edilmez.

## 7. AudioMixer grupları

`Assets/Resources/Audio/WobbleMix.mixer`:

- Master
  - Gameplay — gövde, temas, perfect destek, kesilme ve düşme
  - UI — mevcut menüyü değiştirmeden hazırlanmış ayrı grup
  - Music — mevcut müzik kanalı
  - Ambience — çok hafif mermer temas kuyruğu

Ses ve müzik tercihleri aynı mevcut menü ve SaveManager üzerinden çalışır. Mixer asset'i editörde oluşturulur; player yalnızca public AudioMixer API'sini kullanır. Editor authoring yaklaşımı [Unity'nin resmi C# kaynak API'siyle](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Audio/Mixer/Bindings/AudioMixerController.cs) doğrulandı.

## 8. Eklenen çevre nesneleri

Eski beş basit silüetin yerine **24 düşük poligonlu mimari kütle**: 16 orta plan (6 taş ocağı terası, 5 kırık/anıtsal sütun, 5 sade taş portal), 8 uzaktaki ocak/sırt kütlesi. Ayrıntılı tapınak veya çevreye yayılan süs yok.

Üç ortak mesh ve iki ortak materyal kullanılır. Her anıt 3 kutu formundan birleşir; toplam **864 üçgen**. Orta plan yarıçapı 31–41, uzak plan 66 birimdir. Çevrede collider yok; gölge atımı ve gölge alımı kapalıdır. Görünürlük, nesnenin tüm sınırları aktif kule düzleminin en az 9 birim arkasında kalacak şekilde yumuşatılır. Opak yüzeyler mevcut gökyüzü gradyanına karışır; taşların iç yüzeyleri saydamlık nedeniyle birbirinin içinden görünmez. Kule, beyaz Carrara materyaliyle en okunur nesne olarak kalır.

## 9. Seviye çevre ilerleyişi

Tek sahne ve aynı koşu devam eder. Her 10 bloktaki mevcut olay, 2.4 saniyelik görsel geçişi tetikler:

1. **Quarry:** koyu çevre, yatay taş ocağı kütleleri baskın.
2. **Columns:** dikey sütun/portal ağırlığı artar, ufuk hafif açılır.
3. **Heights:** kule çevrenin üzerine çıkar; mimari daha aşağıda kalır, sis derinleşir.
4. **Cloud Stone:** daha uzakta/aşağıda kalan mimari, biraz daha açık gökyüzü ve çok az askıda mermer tozu.

Sonraki seviyeler dördüncü atmosferde sürer; sahne yüklenmez. Sis başlangıcı 48→42, bitişi 110→95; ışık şiddeti 1.15→1.22 aralığında yumuşak değişir. Işık açısı yalnızca birkaç derece gelişir. Carrara başka bir renge boyanmaz.

## 10. Parçacıklar ve temas

Mevcut dört elemanlı toz havuzu kullanıldı. Artık normal yerleştirmede de kenarda küçük darbe vardır: normalde 6, kesilen hacme göre en fazla 18, perfect'te simetrik 12 parçacık. Düşük kalitede sayı yarıya iner. Toz, bloğun üstünde bir patlama yerine alt temas/kesim kenarından çıkar.

Yeni ortam sistemi küçük, yavaş ve düşük opaklıklı mermer tozu taşır; yüksek/orta kalitede 36, düşükte 12 parçacık sınırı vardır. Kar, parıltı veya sihirli efekt kullanılmaz.

Görsel sıkışma yalnızca `BlockView` altındaki render yüzeyini etkiler. Collider ve kesme boyutları sabittir. Perfect darbesi küçültülüp normal yerleştirmeye bunun %45'i eklendi; mevcut küçük perfect kamera darbesi korundu.

## 11. Kamera ve parallax

`CameraController` **değişmedi**: FOV, kadraj, dönüş, takip ve hareket koridoru aynı. Önceki arka plan kuleyi yaklaşık bire bir izliyordu. Yeni orta plan kule yüksekliğinin %16'sını, uzak plan %5.5'ini yumuşak biçimde takip eder. Farklı katmanlar büyüme sırasında doğal göreli hareket üretir; arka plan ani kaymaz. Menü sunumu ayrıdır ve yeni oyun çevresi menüde kapalıdır.

## 12. Performans etkisi ve doğrulama

Yeni çevre 5 basit renderer yerine en fazla 24 ucuz renderer kullanır; sadece kameranın arkasında kalmayan uygun kütleler çizilir. Ortak mesh/materyaller, instancing destekli shader, sıfır uzak çevre gölgesi, bounded parçacık/ses havuzları kullanılır. Fade property block'ları ve görünürlük sıralaması nedeniyle her nesnenin mutlaka tek instanced draw'a birleştiği iddia edilmez. 864 üçgen için ayrıca LOD sistemi eklenmedi.

Apple M4 / Mac16,12 development player, aynı 12 yerleştirmeli koşu, her çözünürlükte 40 ısınma + 120 örnek, 4× MSAA ve native render boyutu:

| Çözünürlük | Önce ort./p95 | Sonra ort./p95 |
|---|---:|---:|
| 1080×1920 | 1.38 / 2.06 ms | 1.40 / 2.06 ms |
| 1170×2532 | 1.86 / 2.27 ms | 1.88 / 2.27 ms |
| 1080×2400 | 1.67 / 2.12 ms | 1.67 / 2.10 ms |

Bunlar kısa, kontrolsüz masaüstü frame-time örnekleridir; GPU profiler ölçümü, uzun koşu testi veya **mobilde 60 FPS garantisi değildir**. Küçük farklar ölçüm gürültüsü içindedir. iPhone/Android cihazda termal performans ve hoparlör karakteri ayrıca doğrulanmalıdır.

Edit Mode: 13 WOBBLE testi + Addressables'ın 1 test stub'ı geçti. Play Mode: 19 test geçti; önceki menü/dil/kayıt ve dört yön bounce/kesme/skor testleri, yeni ses/temas ve 4 atmosfer aşamasının 3 portre oranındaki görüntü kontrolleri dahil. XML, 12 aşama ön izlemesi ve performans metinleri `Artifacts/Feel/` içinde saklanır.

Son player kontrolünde Unity'nin sis varyantlarını ayıklaması nedeniyle görünmez olan arka plan düzeltildi: çevre shader'ı sis mesafelerini doğrudan kullanır. Güncel Mac build'de mimari görünürlüğü doğrulandı; bu düzeltmeden sonra ses/temas ve dört aşama testleri yeniden **2/2** geçti (`FinalVisualPlayMode.xml`).

## 13. Değişen/eklenen dosyalar

Proje kökü: `/Users/efekangokce/Documents/ChatGPT/wobble block`.

Değişenler:

- `Assets/Scripts/Audio/AudioManager.cs`
- `Assets/Scripts/Gameplay/LevelManager.cs`
- `Assets/Scripts/Gameplay/StackManager.cs` — yalnızca temas ses/toz/punch bağlantıları
- `Assets/Scripts/Gameplay/BlockView.cs` — yalnızca render yüzeyi punch'ı
- `Assets/Scripts/Gameplay/FallingPiecePool.cs` — çarpışma sesi bileşeni bağlantısı
- `Assets/Scripts/Effects/EffectPool.cs`
- `Assets/Scripts/Themes/ThemeManager.cs`
- `Assets/Scripts/Core/GameManager.cs` — ses olayı ve sunum çevresi bağlantıları
- `Assets/Tests/PlayMode/WobblePlayModeTests.cs` — istenen yeni hız beklentileri

Eklenenler:

- `Assets/Scripts/Audio/FallingStoneContact.cs`
- `Assets/Scripts/Themes/CarraraWorld.cs`
- `Assets/Resources/Shaders/ArchitecturalStone.shader`
- `Assets/Resources/Audio/` — 16 WAV, AudioMixer ve Unity metadata
- `Assets/Editor/MarbleAudioBuilder.cs`, `FeelTests.cs`, `FeelPlayerBuilder.cs`
- `Assets/Tests/PlayMode/FeelPlayModeTests.cs`
- Bu rapor ve `Artifacts/Feel/` çıktıları

Yeni paket veya harici asset bağımlılığı eklenmedi. Sesler yeniden üretilebilir: `WOBBLE > Feel > Build Marble Audio`. Yeni Mac test uygulaması: `/private/tmp/wobble-feel-player/WOBBLE.app`.

## 14. Kalan placeholder / doğrulama sınırları

Sesler bu görev için üretilmiş özgün prosedürel modal-sentez WAV'larıdır; gerçek mermer Foley kaydı oldukları iddia edilmez. Çalışan beş varyasyon ve katmanlar hazırdır; final hissin kulakla ve telefon hoparlöründe değerlendirilmesi hâlâ gerekir. İstenirse aynı resource adlarıyla profesyonel Foley kayıtlarıyla değiştirilebilir.

Çevre kasıtlı düşük poligonlu özgün runtime geometridir; satın alınmış veya bitmiş yüksek detaylı ruin asset'i değildir. Müzik parçası, mevcut görev kapsamındaki gibi eklenmedi; önceki müzik ayarı korunur. UI grubuna yeni menü sesleri eklenmedi. Gerçek mobil cihaz testi yapılmadı.

Önceden mevcut `GameManager.audio` isim gölgeleme ve `ProjectBuilder.SetApplicationIdentifier` eski API uyarıları kapsam dışı bırakıldı. Yeni ses başlıkları/mixer grupları, signal headroom ve derleme hataları testlerle kontrol edildi.
