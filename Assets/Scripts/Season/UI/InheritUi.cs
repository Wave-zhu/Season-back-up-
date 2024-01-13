using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Season.UI
{
    public class InheritUi : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        [SerializeField] private Image _image;
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void SetVisual()
        {
            _text.color = Color.grey;
            _image.color =Color.grey;
        }
    }
}