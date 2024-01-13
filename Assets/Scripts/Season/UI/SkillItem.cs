using System.Collections.Generic;
using Season.BattleSubSystem;
using Season.Manager;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Season.UI
{
    public class SkillItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _breakGaugeText;
        [SerializeField] private Image _rangeIcon;
        [SerializeField] private Image _rangeIntensityIcon;
        [SerializeField] private Image _maskColor;
        [SerializeField] private List<GameObject> _inheritUiList;
        private Btn _skillButton;
    
        private SkillSO _skillSO;

        private static readonly Color _costMask = new (0.81f,0.11f,0.11f,0.65f);
        private static readonly Color _limitedMask = new (0.16f,0.57f,0.69f,0.65f);
        private static readonly Color _passMask = new (0.40f,0.32f,0.44f,0.65f);
        private static readonly Color _noMask = new(0f, 0f, 0f, 0f);

        public static bool hasCheckThisTurn = false;
    
        public void InitItem(SkillSO skill)
        {
            _skillSO = skill;
            _skillNameText.text = skill.skillName;
            _healthText.text = skill.baseHealthValue.ToString();
            _breakGaugeText.text = skill.baseBreakGaugeValue.ToString();
            var scene = (BattleScene)SceneAssets.MainInstance.SceneBehavior;
            switch (_skillSO.attackRange)
            {
                case AttackRange.FAN_SL:
                    _rangeIntensityIcon.color = BattleScene.ColorL;
                    _rangeIcon.sprite = scene.FanIcon;
                    break;
                case AttackRange.FAN_L:
                    _rangeIntensityIcon.color = BattleScene.ColorM;
                    _rangeIcon.sprite = scene.FanIcon;
                    break;
                case AttackRange.ALL:
                    _rangeIntensityIcon.color = BattleScene.ColorL;
                    _rangeIcon.sprite = scene.AllIcon;
                    break;
                case AttackRange.CIRCLE_L:
                    _rangeIntensityIcon.color = BattleScene.ColorL;
                    _rangeIcon.sprite = scene.CircleIcon;
                    break;
                case AttackRange.CIRCLE_M:
                    _rangeIntensityIcon.color = BattleScene.ColorM;
                    _rangeIcon.sprite = scene.CircleIcon;
                    break;
                case AttackRange.CIRCLE_S:
                    _rangeIntensityIcon.color = BattleScene.ColorS;
                    _rangeIcon.sprite = scene.CircleIcon;
                    break;
                case AttackRange.LINE_L:
                    _rangeIntensityIcon.color = BattleScene.ColorL;
                    _rangeIcon.sprite = scene.LineIcon;
                    break;
                case AttackRange.LINE_M:
                    _rangeIntensityIcon.color = BattleScene.ColorS;
                    _rangeIcon.sprite = scene.LineIcon;
                    break;
                case AttackRange.SINGLE:
                    _rangeIntensityIcon.color = BattleScene.ColorByPower(skill);
                    _rangeIcon.sprite = scene.SingleIcon;
                    break;
            }
            int inheritEffectType = (int) skill.inheritEffectType;
            for (int i = 0; i < _inheritUiList.Count; i++) 
            {
                int type = 1 << i;
                if ((inheritEffectType & type) == 0) 
                    _inheritUiList[i].GetComponent<InheritUi>().SetVisual();
            }
        }

        public void EnableCheckUsability()
        {
            if (hasCheckThisTurn) return;
            switch (BattleSkillSystem.LegalCheck(_skillSO))
            {
                case SkillAbleType.ENABLE:
                    _skillButton.interactable = true;
                    _maskColor.color = _noMask;
                    break;
                case SkillAbleType.COST_DISABLE:
                    _skillButton.interactable = false;
                    _maskColor.color = _costMask;
                    break;
                case SkillAbleType.LIMITED_DISABLE:
                    _skillButton.interactable =false;
                    _maskColor.color = _limitedMask;
                    break;
                case SkillAbleType.PASS_DISABLE:
                    _skillButton.interactable =false;
                    _maskColor.color = _passMask;
                    break;
            }
        }
        private void Awake()
        {
            _skillButton= GetComponent<Btn>();
            _maskColor.color = _noMask;
        }
    
        private void OnEnable()
        {
            _skillButton.onClick.AddListener(()=>
            {
                //register current skill for all(proactive)
                InActionSystem.CurrentSkill = _skillSO;
                GameEventManager.MainInstance.CallEvent("OnSkillEntered");
            });
        }
        private void OnDisable()
        {
            _skillButton.onClick.RemoveAllListeners();
        }
    }
}
