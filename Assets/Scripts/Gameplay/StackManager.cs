using System;
using System.Collections.Generic;
using UnityEngine;
using Wobble.Audio;
using Wobble.Effects;
using Wobble.Themes;

namespace Wobble.Gameplay
{
    public sealed class StackManager
    {
        public const float BlockHeight = .40f;
        public const float TravelBoundary = 3.8f;
        private readonly Transform towerRoot;
        private readonly ThemeManager themes;
        private readonly AudioManager audio;
        private readonly HapticManager haptics;
        private readonly FallingPiecePool fallingPool;
        private readonly EffectPool effects;
        private readonly List<BlockView> tower = new();
        private MovingBlock moving;
        private int directionIndex;
        private Vector3 lastPlacement;
        private Vector3 lastSize = Vector3.one;
        private float tension, tensionTarget, tensionTime;
        private bool tense;
        private float comboEnergy;
        private int comboTier;

        public BlockView Top => tower.Count == 0 ? null : tower[^1];
        public int BlockCount => Mathf.Max(0, tower.Count - 1);
        public bool CanPlace => moving != null && moving.IsMoving;
        public MovingBlock ActiveBlock => moving;
        public bool IsTense => tense;
        // True for the single placement that resolves WOBBLE tension. Read by the
        // orchestrator after Placed fires; never affects score, size or input.
        public bool StabilizedThisPlacement { get; private set; }
        public event Action<bool, Vector3> Placed;
        public event Action Missed;

        public StackManager(Transform parent, ThemeManager themeManager, AudioManager audioManager, HapticManager hapticManager, MonoBehaviour host)
        {
            themes = themeManager; audio = audioManager; haptics = hapticManager;
            towerRoot = new GameObject("Tower").transform;
            towerRoot.SetParent(parent);
            var physicsRoot = new GameObject("Pooled Falling Pieces").transform;
            physicsRoot.SetParent(parent);
            fallingPool = new FallingPiecePool(physicsRoot, themes.BlockMaterial, themes.TopMaterial, host, audio);
            effects = new EffectPool(host, parent, themes.Current.accent);
        }

        public void StartRun(float speed)
        {
            ClearTower();
            directionIndex = 0;
            var baseBlock = CreateBlock("Foundation", new Vector3(3.3f, BlockHeight, 3.3f), new Vector3(0, 0, 0));
            tower.Add(baseBlock);
            SpawnNext(speed);
        }

        private BlockView CreateBlock(string objectName, Vector3 size, Vector3 position)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(towerRoot);
            go.transform.position = position;
            var view = go.AddComponent<BlockView>();
            view.Initialize(size, themes.BlockMaterial, themes.TopMaterial);
            return view;
        }

        public void SpawnNext(float speed)
        {
            var previous = Top;
            if (previous == null) return;
            var y = previous.transform.position.y + BlockHeight;
            var block = CreateBlock($"Moving block {tower.Count}", previous.Size, new Vector3(previous.transform.position.x, y, previous.transform.position.z));
            moving = block.gameObject.AddComponent<MovingBlock>();
            var direction = (MoveDirection)(directionIndex % 4);
            directionIndex++;
            moving.Begin(direction, previous.transform.position, TravelBoundary, speed);
        }

        public void ResumeAfterMiss(float speed)
        {
            if (Top != null && moving == null) SpawnNext(speed);
        }

        public void PlaceCurrent()
        {
            if (!CanPlace) return;
            StabilizedThisPlacement = false;
            moving.Stop();
            var view = moving.GetComponent<BlockView>();
            var result = BlockCutter.Calculate(view, Top, moving.Axis, .11f);
            if (!result.Success)
            {
                Fail(view);
                return;
            }

            // Rendering coordinates only: both cut halves retain their original pattern.
            var sourcePosition = view.transform.position;
            var sourceStone = view.StoneOrigin;
            view.transform.position = result.KeptPosition;
            view.Resize(result.KeptSize);
            view.SetStoneOrigin(sourceStone + result.KeptPosition - sourcePosition);
            UnityEngine.Object.Destroy(moving);
            moving = null;
            tower.Add(view);

            if (result.Perfect)
            {
                view.Punch(1f, true);
                haptics.Perfect(); effects.Play(view.transform.position, view.Size, true);
                if (tense)
                {
                    StabilizeTension();
                    StabilizedThisPlacement = true;
                    effects.Stabilized(view.transform.position, view.Size);
                }
            }
            else
            {
                var away = (result.CutPosition - view.transform.position).normalized;
                fallingPool.Release(result.CutPosition, result.CutSize, away, sourceStone + result.CutPosition - sourcePosition);
                var cutVolume=result.CutSize.x*result.CutSize.y*result.CutSize.z;
                var keptVolume=result.KeptSize.x*result.KeptSize.y*result.KeptSize.z;
                var cutFraction=cutVolume/(cutVolume+keptVolume);
                view.Punch(.55f, false);
                view.Lean(cutFraction);
                audio.Cut(cutFraction); haptics.Light();
                effects.Play(view.transform.position, view.Size, false, result.CutPosition, cutFraction);
                EndTension();
            }
            lastPlacement = view.transform.position;
            lastSize = view.Size;
            Placed?.Invoke(result.Perfect, view.transform.position);
        }

        private void Fail(BlockView view)
        {
            lastPlacement = view.transform.position;
            lastSize = view.Size;
            moving = null;
            EndTension();
            comboEnergy = 0f; comboTier = 0;
            effects.SetEnergy(0f, 0, lastPlacement, lastSize);
            view.transform.SetParent(null);
            var rigidbody = view.gameObject.AddComponent<Rigidbody>();
            rigidbody.mass = 3.2f;
            rigidbody.angularVelocity = UnityEngine.Random.onUnitSphere * 1.5f;
            rigidbody.linearVelocity = Vector3.down * .3f;
            UnityEngine.Object.Destroy(view.gameObject, 4f);
            audio.GameOver(); haptics.GameOver();
            Missed?.Invoke();
        }

        public void ClearTower()
        {
            moving = null;
            tense = false;
            tension = tensionTarget = 0f;
            comboEnergy = 0f; comboTier = 0;
            tower.Clear();
            for (var i = towerRoot.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(towerRoot.GetChild(i).gameObject);
        }

        // Presentation-only hooks. They never touch scoring, sizing or input.
        public void SetAccents(Color primary, Color secondary) => effects.SetAccent(primary, secondary);
        public void SetComboEnergy(float intensity, int tier)
        {
            comboEnergy = Mathf.Clamp01(intensity);
            comboTier = tier;
        }
        public void TickEffects(float dt)
        {
            effects.Tick(dt);
            tension = Mathf.MoveTowards(tension, tensionTarget, dt / .38f);
            tensionTime += dt;
            if (Top != null)
            {
                for (var i = 0; i < tower.Count; i++)
                {
                    var pair = tower[i];
                    if (pair == null) continue;
                    var weight = i == 0 ? .35f : i >= tower.Count - 7 ? 1f : .72f;
                    pair.SetTension(tension * 1.55f * weight, i * .73f);
                }
                effects.SetEnergy(comboEnergy, comboTier, Top.transform.position, Top.Size);
            }
            else effects.SetEnergy(0f, 0, lastPlacement, lastSize);
        }
        public void BeginTension()
        {
            if (tense) return;
            tense = true;
            tensionTarget = 1f;
            tensionTime = 0f;
        }
        public void StabilizeTension()
        {
            tense = false;
            tensionTarget = 0f;
        }
        public void EndTension()
        {
            tense = false;
            tensionTarget = 0f;
        }
        public void Celebrate(bool perfect, int combo) => effects.Celebrate(perfect, lastPlacement, lastSize, combo);
        public void FailBurst() => effects.Fail(lastPlacement, lastSize);

        public void Dispose() => effects.Dispose();
    }
}
