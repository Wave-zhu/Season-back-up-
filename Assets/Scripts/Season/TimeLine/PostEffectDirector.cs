using Season.Manager;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Season.TimeLine
{
    public class PostEffectDirector : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private UnityEngine.Camera PostEffectCamera;
        [SerializeField] private SignalReceiver _signalReceiver;
        
        public void SetATimeLine(TimelineAsset timeline)
        {
            if (!_director || !timeline) return;
            _director.playableAsset = timeline;
                
            foreach (var track in timeline.GetOutputTracks())
            {
                switch (track.name)
                {
                    case "Signal":
                        _director.SetGenericBinding(track, _signalReceiver);
                        break;
                }
            }
            _director.Play();
        }
    }
}