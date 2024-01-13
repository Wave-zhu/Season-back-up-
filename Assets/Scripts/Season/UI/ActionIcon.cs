using Season.Battle;
using Season.Character;
using TMPro;
using Units.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Season.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ActionIcon : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        private Image _stateImage;
        private CanvasGroup _canvasGroup;
        
        public void Awake()
        {
            _stateImage = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    
        private void SetColorByState(BattleState currentState)
        {
            switch (currentState)
            {
                case BattleState.NORMAL:
                    _stateImage.color = Color.blue;
                    break;
                case BattleState.DELAY:
                    _stateImage.color = Color.cyan;
                    break;
                case BattleState.LIMITED:
                    _stateImage.color = Color.grey;
                    break;
                case BattleState.EXHAUSTED:
                    _stateImage.color = Color.black;
                    break;
                case BattleState.UNLIMITED:
                    _stateImage.color = Color.red;
                    break;
                case BattleState.DISABLE:
                    _stateImage.color = Color.black;
                    break;
                case BattleState.DEATH:
                    DevelopmentTools.WTF("Should be dead");
                    break;
                case BattleState.AD_LIB:
                    _stateImage.color = Color.magenta;
                    break;
            }
        }
        
        //in case
        public void SetName(string name)
        {
            _nameText.text = name;
        }
        public void SetTransparency(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }
        public bool GetActive()
        {
            return _canvasGroup.alpha > 0.9f;
        }
        public void SetStateInfo(CharacterBattleBase battleInfo)
        {
            SetColorByState(battleInfo.CurrentState);
        }
        public void SetPreviewStateInfo(CharacterBattleBase battleInfo)
        {
            SetColorByState(battleInfo.NextState);
            SetTransparency(0.8f);
        }
        
    }
}
