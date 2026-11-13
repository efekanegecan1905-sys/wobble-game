#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Wobble.Gameplay;
namespace Wobble.Editor
{
    public class ArcadeRulesTests
    {
        [Test] public void ClassicRulesStayInert()
        { var r=new ArcadeRun(ArcadeMode.Classic,new());r.Tick(100);r.Place(true,Vector2.one,3);Assert.AreEqual(0,r.Score);Assert.IsFalse(r.Ended); }
        [Test] public void RushExpiresAndThirdMissEnds()
        { var r=new ArcadeRun(ArcadeMode.Rush,new());r.Tick(30);Assert.IsTrue(r.Turbo);r.Miss();r.Miss();Assert.IsFalse(r.Ended);r.Miss();Assert.IsTrue(r.Ended);var timer=new ArcadeRun(ArcadeMode.Rush,new());timer.Tick(60);Assert.IsTrue(timer.Ended); }
        [Test] public void BalanceIsDeterministicAndCorrectable()
        { var r=new ArcadeRun(ArcadeMode.Balance,new());r.Place(false,Vector2.right,3);r.Place(false,Vector2.left,3);Assert.AreEqual(Vector2.zero,r.Balance);r.Place(false,Vector2.right*2,3);r.Tick(1);Assert.IsFalse(r.Ended);r.Tick(1);Assert.IsTrue(r.Ended); }
        [Test] public void AllTenTrialsCanBeCompleted()
        { for(int i=0;i<10;i++){var r=new ArcadeRun(ArcadeMode.Trials,new(),i);for(int n=0;n<TrialDefinition.All[i].blocks;n++)r.Place(true,Vector2.zero,3.3f);Assert.IsTrue(r.Won);Assert.AreEqual(3,r.Stars);} }
    }
}
#endif
