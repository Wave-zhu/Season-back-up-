using System;
using System.Collections.Generic;
using Season.Manager;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Season.TimeLine
{
    [Serializable]
    public class TimelineParticleSystemBehaviour : PlayableBehaviour
    {
        public bool IsDynamic;
        public GameObject Prefab;
        public IEnumerable<GameObject> Targets;
        public Transform Parent;
        public Vector3 Rotation;
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public float speed;


        private List<GameObject> _objects = new ();
        private List<ParticleSystem> _particleSystems = new();
    
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            Targets = BattleManager.BattleEffectSystem.GetTargets();
            if (Prefab == null) return;
            foreach (var obj in _objects)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
            }
            _objects.Clear();
            _particleSystems.Clear();

            if (IsDynamic && Targets !=null)
            {
                foreach (var target in Targets)
                {
                    var obj = Object.Instantiate(Prefab, target.transform);
                    _objects.Add(obj);
                    obj.SetActive(true);
                    var particleSystem = obj.GetComponent<ParticleSystem>();
                    _particleSystems.Add(particleSystem);
                    particleSystem.Play();
                }
            }
            else
            {
                var obj = Object.Instantiate(Prefab, Parent);
                _objects.Add(obj);
                obj.SetActive(true);
                var particleSystem = obj.GetComponent<ParticleSystem>();
                _particleSystems.Add(particleSystem);
                particleSystem.Play();
            }
       
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            foreach (var obj in _objects)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
            }
            _objects.Clear();
            _particleSystems.Clear();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            foreach (var obj in _objects)
            {
                obj.transform.localRotation = Quaternion.Euler(Rotation);
                obj.transform.localPosition = Position;
                obj.transform.localScale = Scale;
            }
#if UNITY_EDITOR
            if (_particleSystems.Count > 0 && !EditorApplication.isPlaying)
            {
                double time = playable.GetTime();
                foreach (var particleSystem in _particleSystems)
                {
                    particleSystem.Simulate((float)time, true, false);
                    particleSystem.Play();
                }
                SceneView.RepaintAll();
            }
#endif
        }
    
    }
}