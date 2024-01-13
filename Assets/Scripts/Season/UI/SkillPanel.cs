using System;
using System.Collections.Generic;
using Season.Manager;
using UnityEngine;

namespace Season.UI
{
    public class SkillPanel : MonoBehaviour
    {
        [SerializeField] private Transform _backGround;

        private int _actionCount;
        private int _turnCount;
        
        private List<SkillItem> _skillList;

        public Action SkillEnableCheck;
        
        private void UpdateUiItem()
        {
            var skillSetSO = BattleManager.ProactiveNonTargetSkillSystem.SkillSet;
            if (!skillSetSO) return;
            
            var skillSet = skillSetSO.skillSet;
            
            for(int i = 0; i < skillSet.Count; i++) 
            {
                _skillList[i].gameObject.SetActive(true);
                _skillList[i].InitItem(skillSet[i]);
                _skillList[i].EnableCheckUsability();
            }

            for (int i = skillSet.Count; i < 10; i++)
            {
                _skillList[i].gameObject.SetActive(false);
            }
        }
        
        
        private void Awake()
        {
            _skillList = new();
            for (int i = 0; i < 10; i++)
            {
                var obj = GamePoolManager.MainInstance.TryGetPoolItem("SkillItem");
                obj.transform.SetParent(_backGround, false);
                _skillList.Add(obj.GetComponent<SkillItem>());
            }
        }
        
        private void OnEnable()
        {
            
            if(BattleManager.BattleSequenceSystem.ActionCount == _actionCount
               && BattleManager.BattleSequenceSystem.TurnCount == _turnCount) return;
            _actionCount = BattleManager.BattleSequenceSystem.ActionCount;
            _turnCount = BattleManager.BattleSequenceSystem.TurnCount;
            UpdateUiItem();
        }
    }
}
