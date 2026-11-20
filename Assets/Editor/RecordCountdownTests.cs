#if UNITY_EDITOR
using NUnit.Framework;
using Wobble.Gameplay;
using UnityEngine;
namespace Wobble.Tests
{
    public class RecordCountdownTests
    {
        [TestCase(ArcadeMode.Classic)][TestCase(ArcadeMode.Rush)][TestCase(ArcadeMode.Balance)]
        public void RecordsRequireStrictImprovement(ArcadeMode mode)
        {
            Assert.IsFalse(new RunRecord(mode,100,100,0,0,false,false).Improved);
            Assert.IsFalse(new RunRecord(mode,99,100,0,0,false,false).Improved);
            Assert.IsTrue(new RunRecord(mode,101,100,0,0,false,false).Improved);
        }
        [Test] public void TrialsUseEarnedStarsNotUnsavedScoreOrTime()
        {
            Assert.IsFalse(new RunRecord(ArcadeMode.Trials,9999,3,3,4,true,false).Improved);
            Assert.IsTrue(new RunRecord(ArcadeMode.Trials,100,1,2,4,true,false).Improved);
            Assert.IsFalse(new RunRecord(ArcadeMode.Trials,9999,0,3,4,false,false).Improved);
        }
        [Test] public void RushDurationDoesNotDriftAndExpiryRejectsPlacement()
        {
            var run=new ArcadeRun(ArcadeMode.Rush,new ArcadeTuning());
            for(int i=0;i<5999;i++)run.Tick(.01f);
            Assert.IsFalse(run.Ended);Assert.That(run.Remaining,Is.EqualTo(.01f).Within(.00001f));
            run.Place(true,Vector2.zero,3.3f);int score=run.Score;
            run.Tick(.01f);Assert.IsTrue(run.TimedOut);Assert.AreEqual(0,run.Remaining);
            run.Place(true,Vector2.zero,3.3f);run.Miss();run.Tick(100);
            Assert.AreEqual(score,run.Score);Assert.AreEqual(1,run.Blocks);
        }
        [Test] public void CountdownEmitsOnceAndNeverReplaysSkippedBeats()
        {
            var cue=new CountdownBeat();Assert.AreEqual(-1,cue.Observe(60));
            Assert.AreEqual(15,cue.Observe(15));Assert.AreEqual(-1,cue.Observe(14.9f));
            Assert.AreEqual(3,cue.Observe(2.9f));Assert.AreEqual(-1,cue.Observe(3));
            Assert.AreEqual(0,cue.Observe(0));Assert.AreEqual(-1,cue.Observe(0));
            cue.Reset();Assert.AreEqual(10,cue.Observe(10));
        }
        [Test] public void TimedTrialCannotCompleteAfterExpiry()
        {
            var run=new ArcadeRun(ArcadeMode.Trials,new ArcadeTuning(),0);run.Tick(45);
            for(int i=0;i<8;i++)run.Place(true,Vector2.zero,3.3f);
            Assert.IsTrue(run.TimedOut);Assert.IsFalse(run.Won);Assert.AreEqual(0,run.Stars);
        }
    }
}
#endif
