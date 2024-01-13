using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Season.TimeLine
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(TimelineParticleSystemClip))]
    [DisplayName("Particle System Track")]
    public class TimelineParticleSystemTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TimelineParticleSystemBehaviour>.Create(graph, inputCount);
        }
    
    }
}