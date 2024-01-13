using Season.Manager;
using Season.SceneBehaviors;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Season.TimeLine
{
    public class CutsceneDirector : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private Animator _followCamera;
        [SerializeField] private Animator _backCamera;
        [SerializeField] private SignalReceiver _signalReceiver;
        
        public void SetATimeLine(TimelineAsset timeline, Animator animator)
        {
            if (!_director || !timeline) return;
            _director.playableAsset = timeline;
                
            foreach (var track in timeline.GetOutputTracks())
            {
                switch (track.name)
                {
                    case "Avatar":
                        _director.SetGenericBinding(track, animator);
                        break;
                    case "Cinemachine":
                        _director.SetGenericBinding(track, SceneAssets.CameraSubSystem.SequenceCameraControl);
                        break;
                    case "Follow":
                        _director.SetGenericBinding(track, _followCamera);
                        break;
                    case "Back":
                        _director.SetGenericBinding(track, _backCamera);
                        break;
                    case "Signal":
                        _director.SetGenericBinding(track, _signalReceiver);
                        break;
                }
            }
            _director.Play();
        }
    }
}