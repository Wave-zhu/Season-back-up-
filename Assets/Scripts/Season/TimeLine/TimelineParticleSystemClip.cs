using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

namespace Season.TimeLine
{
    [Serializable]
    [DisplayName("Particle System")]
    public class TimelineParticleSystemClip : PlayableAsset
    {
        public ExposedReference<Transform> parent;
        public GameObject prefab;
        public Vector3 rotation;
        public Vector3 position;
        public Vector3 scale=Vector3.one;
        public float timeScale = 1f;
        public float particleTimeOffset;
        public bool isDynamic;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TimelineParticleSystemBehaviour>.Create(graph);
            TimelineParticleSystemBehaviour behaviour = playable.GetBehaviour();
            behaviour.Parent = parent.Resolve(graph.GetResolver());
            behaviour.Prefab = prefab;
            behaviour.Rotation = rotation;
            behaviour.Position = position;
            behaviour.Scale = scale;
            behaviour.IsDynamic = isDynamic;
            return playable;
        }
    }
}