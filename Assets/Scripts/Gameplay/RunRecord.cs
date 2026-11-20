namespace Wobble.Gameplay
{
    // Presentation snapshot: persistence stays exclusively in GameManager/SaveManager.
    public readonly struct RunRecord
    {
        public readonly ArcadeMode Mode;
        public readonly int Score, Previous, Value, Best, Trial;
        public readonly bool Improved, Won, FirstClear;
        public RunRecord(ArcadeMode mode,int score,int previous,int stars,int trial,bool won,bool firstClear)
        {
            Mode=mode;Score=score;Previous=previous;Trial=trial;Won=won;FirstClear=firstClear;
            Value=mode==ArcadeMode.Trials?(won?stars:0):score;
            Improved=Value>previous;Best=System.Math.Max(Value,previous);
        }
        public bool NearRecord=>!Improved&&Previous>0&&Value>0&&Value>=Previous*.9f;
    }
    public sealed class CountdownBeat
    {
        private int previous=int.MaxValue;
        public void Reset()=>previous=int.MaxValue;
        // One cue for the current second, never a burst of missed beats after a slow frame.
        public int Observe(float remaining)
        {
            int second=UnityEngine.Mathf.Max(0,UnityEngine.Mathf.CeilToInt(remaining));
            if(second>=previous)return -1;
            previous=second;
            return second==15||second==10||second<=5?second:-1;
        }
    }
}
