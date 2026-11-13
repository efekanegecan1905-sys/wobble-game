using System;
using UnityEngine;

namespace Wobble.Gameplay
{
    public enum ArcadeMode { Classic, Rush, Balance, Trials }

    [Serializable]
    public sealed class ArcadeTuning
    {
        public float rushDuration = 60, turboAt = 30, balanceGain = .65f;
        public float recovery = .08f, dangerGrace = 2;
        public int rushPerfect = 150, rushNormal = 60;
    }

    public readonly struct TrialDefinition
    {
        public readonly int blocks, chain;
        public readonly float seconds, width;
        public TrialDefinition(int blocks, int chain, float seconds, float width)
        { this.blocks=blocks; this.chain=chain; this.seconds=seconds; this.width=width; }
        public static readonly TrialDefinition[] All = {
            new(6,0,45,1), new(8,3,45,1), new(10,0,35,1.8f),
            new(10,4,40,1.2f), new(12,0,30,1.5f), new(12,5,40,1.8f),
            new(15,3,40,2), new(16,6,45,1.5f), new(18,5,40,2), new(20,8,45,2.2f)
        };
    }

    // Pure run state: never consumes lives, writes saves, or alters Classic scoring.
    public sealed class ArcadeRun
    {
        public ArcadeMode Mode { get; }
        public ArcadeTuning Tuning { get; }
        public int Trial { get; }
        public int Score { get; private set; }
        public int Blocks { get; private set; }
        public int Chain { get; private set; }
        public int BestChain { get; private set; }
        public int Misses { get; private set; }
        public float Remaining { get; private set; }
        public Vector2 Balance { get; private set; }
        public bool Ended { get; private set; }
        public bool Won { get; private set; }
        public int Stars { get; private set; }
        private float danger;
        public bool Turbo => Mode==ArcadeMode.Rush && Remaining<=Tuning.turboAt;
        public ArcadeRun(ArcadeMode mode, ArcadeTuning tuning, int trial=0)
        {
            Mode=mode; Tuning=tuning; Trial=Mathf.Clamp(trial,0,9);
            Remaining=mode==ArcadeMode.Rush?tuning.rushDuration:mode==ArcadeMode.Trials?TrialDefinition.All[Trial].seconds:0;
        }
        public void Tick(float dt)
        {
            if(Ended || Mode==ArcadeMode.Classic)return;
            dt=Mathf.Max(0,dt);
            if(Mode==ArcadeMode.Rush || Mode==ArcadeMode.Trials)
            { Remaining=Mathf.Max(0,Remaining-dt); if(Remaining==0)Ended=true; }
            if(Mode==ArcadeMode.Balance)
            { danger=Balance.magnitude>=1?danger+dt:0; if(danger>=Tuning.dangerGrace)Ended=true; }
        }
        public void Place(bool perfect, Vector2 offset, float width)
        {
            if(Ended || Mode==ArcadeMode.Classic)return;
            Blocks++; Chain=perfect?Chain+1:0; BestChain=Mathf.Max(Chain,BestChain);
            Score+=(perfect?Tuning.rushPerfect:Tuning.rushNormal)+(perfect?Mathf.Min(Chain,20)*25:0);
            if(Mode==ArcadeMode.Balance)
            {
                Balance+=offset*Tuning.balanceGain;
                if(perfect)Balance=Vector2.MoveTowards(Balance,Vector2.zero,Tuning.recovery);
            }
            if(Mode==ArcadeMode.Trials)
            {
                var target=TrialDefinition.All[Trial];
                if(width<target.width){Ended=true;return;}
                if(Blocks>=target.blocks && BestChain>=target.chain)
                { Ended=Won=true; Stars=1+(Remaining>=target.seconds*.25f?1:0)+(width>=target.width+.4f?1:0); }
            }
        }
        public void Miss()
        {
            if(Ended)return;
            Misses++;Chain=0;
            Ended=Mode!=ArcadeMode.Rush || Misses>=3;
        }
    }
}
