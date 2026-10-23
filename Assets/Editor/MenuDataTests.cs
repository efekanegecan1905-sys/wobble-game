#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Wobble.Save;
using Wobble.Economy;

namespace Wobble.Tests
{
    public sealed class MenuDataTests
    {
        private sealed class MemoryStore : ISaveStore
        {
            public readonly Dictionary<string,string> values=new();
            public string Read(string key)=>values.TryGetValue(key,out var value)?value:string.Empty;
            public void Write(string key,string value)=>values[key]=value;
        }
        private sealed class Clock : IRewardClock { public DateTimeOffset now=new(2026,10,10,12,0,0,TimeSpan.Zero);public DateTimeOffset UtcNow=>now; }
        [Test] public void OldSaveMigratesWithoutLosingScoreCurrencyOrSettings()
        {
            var store=new MemoryStore();store.Write("wobble.save.v1","{\"bestScore\":39,\"gems\":250,\"audioEnabled\":false,\"hapticsEnabled\":false,\"ownedSkins\":[\"Carrara\"]}");
            var save=new SaveManager(store);
            Assert.That(save.Data.bestScore,Is.EqualTo(39));Assert.That(save.Data.gems,Is.EqualTo(250));
            Assert.That(save.Data.audioEnabled,Is.False);Assert.That(save.Data.hapticsEnabled,Is.False);
            Assert.That(save.Data.selectedLocale,Is.EqualTo("tr"));Assert.That(save.Data.lives,Is.EqualTo(5));
            Assert.That(save.Data.musicEnabled,Is.True);Assert.That(save.Data.graphicsQuality,Is.EqualTo(2));
        }
        [Test] public void ExtendedDataRoundTrips()
        {
            var store=new MemoryStore();var save=new SaveManager(store);
            save.Data.gems=600;save.Data.lives=22;save.Data.selectedLocale="en";save.Data.bestScore=88;
            save.Data.musicEnabled=false;save.Data.graphicsQuality=1;save.Data.dailyRewardIndex=4;save.Data.lastDailyClaimDay=20000;save.Save();
            var loaded=new SaveManager(store).Data;
            Assert.That(loaded.gems,Is.EqualTo(600));Assert.That(loaded.lives,Is.EqualTo(22));Assert.That(loaded.selectedLocale,Is.EqualTo("en"));
            Assert.That(loaded.bestScore,Is.EqualTo(88));Assert.That(loaded.musicEnabled,Is.False);Assert.That(loaded.graphicsQuality,Is.EqualTo(1));
            Assert.That(loaded.dailyRewardIndex,Is.EqualTo(4));Assert.That(loaded.lastDailyClaimDay,Is.EqualTo(20000));
        }
        [Test] public void DailyCycleClaimsExactlyOnceAndWrapsWithoutLostRewards()
        {
            var store=new MemoryStore();var save=new SaveManager(store);var clock=new Clock();
            var catalog=Resources.Load<MenuCatalog>("Menu/MenuCatalog");var daily=new DailyRewardService(save,catalog,clock);
            for(var i=0;i<7;i++)
            {
                Assert.That(daily.NextIndex,Is.EqualTo(i));Assert.That(daily.Claim(),Is.True);Assert.That(daily.Claim(),Is.False);
                daily=new DailyRewardService(new SaveManager(store),catalog,clock);
                Assert.That(daily.CanClaim,Is.False);clock.now=clock.now.AddDays(1);
            }
            var loaded=new SaveManager(store).Data;
            Assert.That(loaded.gems,Is.EqualTo(200));Assert.That(loaded.lives,Is.EqualTo(12));Assert.That(daily.NextIndex,Is.Zero);
            Assert.That(daily.CanClaim,Is.True);
        }
        [Test] public void DailyBlocksClockRollbackAndAllowsMissedDaysWithoutReset()
        {
            var save=new SaveManager(new MemoryStore());var clock=new Clock();var daily=new DailyRewardService(save,Resources.Load<MenuCatalog>("Menu/MenuCatalog"),clock);
            Assert.That(daily.Claim(),Is.True);clock.now=clock.now.AddDays(-2);Assert.That(daily.CanClaim,Is.False);
            clock.now=clock.now.AddDays(10);Assert.That(daily.NextIndex,Is.EqualTo(1));Assert.That(daily.Claim(),Is.True);
        }
        [Test] public void ContinueHooksDoNotSpendUntilExplicitConsume()
        {
            var store=new MemoryStore();var save=new SaveManager(store);var economy=new EconomyManager(save);
            Assert.That(economy.ContinueWithLife(),Is.False);Assert.That(economy.ContinueWithRewardedAd(),Is.False);Assert.That(economy.Lives,Is.EqualTo(5));
            for(var i=0;i<5;i++)Assert.That(economy.ConsumeLifeForContinue(),Is.True);
            Assert.That(economy.CanContinueWithLife(),Is.False);Assert.That(economy.ConsumeLifeForContinue(),Is.False);
            Assert.That(new SaveManager(store).Data.lives,Is.Zero);
        }
        [Test] public void ProductAndThemeCatalogMatchesRequestedPlaceholders()
        {
            var catalog=Resources.Load<MenuCatalog>("Menu/MenuCatalog");
            Assert.That(catalog.products.Length,Is.EqualTo(7));Assert.That(catalog.themes.Length,Is.EqualTo(5));
            var starter=Array.Find(catalog.products,p=>p.id=="starter");
            Assert.That(starter.gems,Is.EqualTo(500));Assert.That(starter.lives,Is.EqualTo(20));Assert.That(starter.futureSkinId,Is.Not.Empty);
            Assert.That(catalog.themes[0].available,Is.True);
            for(var i=1;i<5;i++)Assert.That(catalog.themes[i].available,Is.False);
        }
    }
}
#endif
