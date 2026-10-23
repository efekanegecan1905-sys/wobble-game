using UnityEngine;

namespace Wobble.Audio
{
    // Re-armed by the existing pool; one audible collision per released offcut.
    public sealed class FallingStoneContact : MonoBehaviour
    {
        private AudioManager audioManager;
        private float armedAt;
        private bool sounded;
        public void Initialize(AudioManager manager)=>audioManager=manager;
        private void OnEnable(){sounded=false;armedAt=Time.time+.1f;}
        private void OnCollisionEnter(Collision collision)
        {
            if(sounded || Time.time<armedAt || collision.relativeVelocity.magnitude<.65f)return;
            sounded=true;
            audioManager?.FallingImpact(collision.relativeVelocity.magnitude);
        }
    }
}
