using Season.AssetLibrary;
using UnityEngine;
namespace Season.UI
{
    public class BtnConfig : MonoBehaviour
    {
        public float fadeTime = 0.2f;
        public float onHoverAlpha = 0.6f;
        public float onClickAlpha = 0.7f;
        public AudioClip pressSound;
        public AudioClip rolloverSound;
    }
}