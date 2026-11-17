# Arcade modları — geliştirme dalı

Başlangıç: `b49ee3f`, dal: `feature/arcade-modes`. Permanent bundle ID `com.wobble.game` korunur. Main ile birleştirilmez.

## Oynanış

- Classic: mevcut puan, kombo, ekonomi, canla devam ve kayıt akışı. PLAY artık mod seçimine gider; Classic seçimi aynı oyun kurallarını başlatır.
- 60 Second Rush: 60 saniye, Perfect/seri bonuslu ayrı skor, son 30 saniyede 7.5 hareket hızı (başlangıç 5.5), son 10 saniyede görsel/sesli sayaç vurgusu. İlk iki kaçırma kuleyi yeniden başlatır, üçüncü kaçırma veya süre sonu koşuyu bitirir. Satın alınan canlar kullanılmaz; süre kule sıfırlarken korunur.
- Wobble Balance: gerçek yerleştirme ofseti X/Z denge vektörüne eklenir. Ters yönlü düzeltme ve Perfect yerleşimler dengeyi toparlar. Denge büyüklüğü 1 veya üzerindeyken iki saniyelik kritik süre sonunda koşu biter. Görsel taş eğimi ve gösterge aynı değeri izler; rastgele devrilme yoktur.
- Tower Trials: blok, seri, süre ve minimum genişlik hedefleri içeren 10 seviye. Önceki seviyeyi bitirme kilidi, 1–3 yıldız, kalıcı en iyi yıldız, retry/next geçişleri. İlk tamamlamada 5 elmas, tekrar tamamlamalarda sıfır ek ilk-bitiriş ödülü.

## Yapı

`ArcadeRun` saf mod kurallarını, `GameManager` mevcut StackManager üzerinden yaşam döngüsünü yönetir. `ArcadeModeUI` yerelleştirilmiş seçim/HUD/sonuç ekranlarını, `ArcadePreview` dört hareketli 3D kartı üretir. Mevcut fontlar, yuvarlatılmış yüzeyler, taş buton tepkisi ve vektör ikon sistemi kullanılır. Trials yıldızları font glifi gerektirmez.

`SaveData.rushBest`, `balanceBest`, `trialStars` ayrı tutulur. Eski kayıtlar yeni alanlarla yüklenir; Classic bestScore yeni modlar tarafından yazılmaz. `ArcadeTuning` zamanlama/denge/puan katsayılarını merkezileştirir; 10 seviye hedefi `TrialDefinition.All` tablosundadır. Yeni paket, reklam veya gerçek satın alma eklenmedi.

## Doğrulama

- EditMode: 31/31 geçti (4 yeni kural testi dahil).
- Tam PlayMode: 32 geçti, 0 başarısız, 1 demo-only atlandı.
- Son HUD/ikon/animasyon değişikliklerinden sonra ArcadeIntegrationTests: 3/3 geçti.
- Entegrasyon: gerçek blok yerleştirme, Balance ofset ve Perfect toparlanması, Rush ücretsiz hata/ayrı kayıt, Trials tekrarlı tamamlama ödül idempotansı, Classic ilk Perfect puanı doğrulandı.
- 10 Trials seviyesi saf kural testinde tamamlandı. Fiziksel cihazda 10 seviyenin tamamı ayrıca oynanmadı.
- Yeni yakalamalar: `Artifacts/Arcade/Selection.png`, `Classic.png`, `Rush.png`, `Balance.png`, `Trials.png`.

## Onay öncesi kalan değerlendirmeler

Bu sürüm geliştirme/playtest sürümüdür. Yeni modlar için fiziksel iPhone performans, güvenli alan ve dokunma QA'sı henüz yapılmadı. Rush temposu, Balance toparlanma katsayısı ve Trials yıldız eşikleri gerçek oyuncu geri bildirimiyle dengelenmelidir. Önizlemeler 1080×1920 hedefinde üretildi; yeni ekranların tüm cihaz oranlarında ve iki dilde kapsamlı görsel onayı tamamlanmadı. Main merge için oynanış onayı beklenir.

## 18 Kasım devamı — cihaz kurulumu

- Yarım kalan Xcode derlemesinin doğrulanan engeli giderildi: mevcut SDK iOS 13 deployment target değerini reddediyordu. `IOSDeviceBuilder` izole cihaz kopyasında iOS 15 hedefler; kaynak ProjectSettings ve permanent bundle ID değiştirilmedi.
- Trials seviye seçimine Türkçe/İngilizce MOD SEÇ / SELECT MODE dönüş düğmesi eklendi.
- ArcadeIntegrationTests tekrar çalıştırıldı: 3/3 geçti. Mevcut önizlemeler test koşusunda yenilendi.
- Unity IL2CPP export ve Xcode Release derlemesi başarılı. Önceki geçici kimlikle (`com.efekangokce.wobble.themedemo`) bağlı iPhone 17 Pro'ya güncelleme kuruldu; normal uygulama başlatma komutu başarılı döndü.
- Bu kontrol cihaz kurulumu/açılış kontrolüdür. Yukarıdaki kapsamlı cihaz performans, ekran oranı ve dört modun manuel oynanış değerlendirmeleri açık kalır. App Store ikonu uyarısı da mağaza yayınından önce giderilmelidir.
