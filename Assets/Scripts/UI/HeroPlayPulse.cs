using UnityEngine;
using UnityEngine.UI;

namespace Wobble.UI
{
    // Idle life for the hero call to action: a slow breath on the face and a sheen
    // that crosses the button every few seconds. Unscaled, event-free and tiny; it
    // stops the moment the menu is hidden.
    public sealed class HeroPlayPulse : MonoBehaviour
    {
        public Image face;
        public RectTransform sheen;
        private Color baseColor = Color.white;
        private float startY;
        private float clock;
        private bool captured;

        private void OnEnable()
        {
            clock = 0f;
            captured = false;
        }

        private void Update()
        {
            if (!captured)
            {
                if (face != null) baseColor = face.color;
                if (sheen != null) startY = sheen.anchoredPosition.y;
                captured = true;
            }
            clock += Time.unscaledDeltaTime;
            if (face != null)
            {
                var breath = .5f + .5f * Mathf.Sin(clock * 2.1f);
                face.color = Color.Lerp(baseColor, Color.Lerp(baseColor, Color.white, .11f), breath * .7f);
            }
            if (sheen == null) return;
            // Sweep with a long pause so it reads as a highlight, not a loop.
            var cycle = Mathf.Repeat(clock * .34f + .28f, 1f);
            var travel = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((cycle - .12f) / .46f));
            sheen.anchoredPosition = new Vector2(Mathf.Lerp(-540f, 540f, travel), startY);
        }
    }
}
