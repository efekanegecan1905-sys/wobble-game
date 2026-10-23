using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wobble.Effects
{
    public sealed class EffectPool
    {
        private readonly Queue<ParticleSystem> particles = new();
        private readonly MonoBehaviour host;
        private readonly Color color;
        private readonly Material dustMaterial;

        public EffectPool(MonoBehaviour host, Transform parent, Color accent, int count = 4)
        {
            this.host = host; color = new Color(.67f,.65f,.59f,.23f);
            dustMaterial = new Material(Resources.Load<Shader>("Shaders/StoneDust"));
            for (var i = 0; i < count; i++) particles.Enqueue(Create(parent));
        }

        private ParticleSystem Create(Transform parent)
        {
            var go = new GameObject("Pooled stone dust");
            go.transform.SetParent(parent);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false; main.duration = .25f; main.startLifetime = new ParticleSystem.MinMaxCurve(.18f, .45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.05f, .16f); main.startSize = new ParticleSystem.MinMaxCurve(.014f, .035f);
            main.gravityModifier = .06f;
            main.startColor = color;
            main.maxParticles = 32; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 0;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(2f, .02f, 2f);
            var fade = ps.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = dustMaterial;
            go.SetActive(false);
            return ps;
        }

        public void Play(Vector3 position, Vector3 size, bool perfect = true, Vector3? cutEdge = null, float cutFraction = 0)
        {
            var ps = particles.Count > 0 ? particles.Dequeue() : null;
            if (ps == null) return;
            ps.transform.position = position - Vector3.up * (size.y * .5f - .015f);
            var shape = ps.shape;
            shape.scale = new Vector3(size.x * .9f, .02f, size.z * .9f);
            ps.gameObject.SetActive(true);
            ps.Play(true);
            var count=perfect?12:6+Mathf.RoundToInt(cutFraction*12);
            if(QualitySettings.GetQualityLevel()==0)count=Mathf.Max(3,count/2);
            for(var i=0;i<count;i++)
            {
                var side=i%4;var along=(i/4)/(float)Mathf.Max(1,Mathf.CeilToInt(count/4f)-1)*2-1;
                var offset=side<2?new Vector3((side==0?-1:1)*size.x*.5f,0,along*size.z*.5f)
                    :new Vector3(along*size.x*.5f,0,(side==2?-1:1)*size.z*.5f);
                if(!perfect && cutEdge.HasValue)
                {
                    var edge=cutEdge.Value-position;
                    offset=Mathf.Abs(edge.x)>Mathf.Abs(edge.z)
                        ?new Vector3(Mathf.Sign(edge.x)*size.x*.5f,0,Mathf.Lerp(-size.z*.5f,size.z*.5f,i/(float)Mathf.Max(1,count-1)))
                        :new Vector3(Mathf.Lerp(-size.x*.5f,size.x*.5f,i/(float)Mathf.Max(1,count-1)),0,Mathf.Sign(edge.z)*size.z*.5f);
                }
                ps.Emit(new ParticleSystem.EmitParams{position=ps.transform.position+offset,velocity=offset.normalized*.09f+Vector3.up*.07f},1);
            }
            host.StartCoroutine(Return(ps));
        }

        private IEnumerator Return(ParticleSystem ps)
        {
            yield return new WaitForSeconds(1f);
            ps.gameObject.SetActive(false);
            particles.Enqueue(ps);
        }

        public void Dispose() => Object.Destroy(dustMaterial);
    }
}
