using System.Collections.Generic;
using Season.AssetLibrary;
using Season.Character;
using Season.Manager;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.BattleSubSystem
{
    internal class EffectParts
    {
        public GameObject member;
        public CharacterStatusBase status;
        public Outline outline;
    }
    public class BattleEffectSystem : InActionSystem
    {
        //Register All Effect
        private Dictionary<GameObject, EffectParts> _effectInfos;

        private HashSet<GameObject> _effectTargets;
        public IEnumerable<GameObject> GetTargets() => _effectTargets;

        public Vector3 GetIdealPosition()
        {
            foreach (var target in _effectTargets)
            {
                return target.transform.position;
            }
            return MainBattleCharacter.Member.transform.position;
        }
        private List<GameObject> _toRemove;

        //TODO
        public void AllowEffect(GameObject target, bool value)
        {
            if (!_effectInfos.ContainsKey(target))
            {
                _effectInfos[target] = new EffectParts()
                {
                    member = target,
                    status = target.GetComponent<CharacterStatusBase>(),
                    outline = target.GetComponent<Outline>(),
                };
            }
            _effectInfos[target].outline.enabled = value;
        }

        #region Tick Targets
        public void TickEffectedMember(GameObject except)
        {
            RegisterEffectedMember(except);
            
            foreach (var target in _effectTargets) 
            {
                if (target != except) _toRemove.Add(target);
            }
            
            foreach (var target in _toRemove) 
            {
                _effectTargets.Remove(target);
            }
            
            _toRemove.Clear();
            
            foreach (var entry in _effectInfos) 
            {
                entry.Value.outline.enabled = entry.Key == except;
            }
        }
        public void TickEffectedMembers(HashSet<GameObject> excepts)
        {
            foreach (var except in excepts) 
            {
                RegisterEffectedMember(except);
            }
            
            foreach (var target in _effectTargets) 
            {
                if (!excepts.Contains(target)) _toRemove.Add(target);
            }
            
            foreach (var target in _toRemove) 
            {
                _effectTargets.Remove(target);
            }
            
            _toRemove.Clear();
            
            foreach (var entry in _effectInfos) 
            {
                entry.Value.outline.enabled = excepts.Contains(entry.Key);
            }
            
        }
        public void AddEffectedMembers(IEnumerable<GameObject> members)
        {
            foreach (var member in members) {
                RegisterEffectedMember(member);
                _effectInfos[member].outline.enabled = true;
            }
        }
        private void RegisterEffectedMember(GameObject member)
        {
            if (!member) return;
            if (!_effectInfos.ContainsKey(member)) {
                _effectInfos[member] = new EffectParts() {
                    member = member,
                    status = member.GetComponent<CharacterStatusBase>(),
                    outline = member.GetComponent<Outline>(),
                };
            }
            _effectTargets.Add(member);
        }
        
        #endregion

        #region All Effect

        private void ResetAllEffect()
        {
            ResetEffect();
        }

        #endregion
        
        #region Fan Effect

        private GameObject _fanEffect;
        private SpriteRenderer _fanEffectRender;
        
        public void AllowFanEffect(bool value)
        {
            _fanEffect.SetActive(value);
        }

        public void ResetFanEffect()
        {
            AllowFanEffect(false);
            ResetEffect();
        }
        
        public void FanRange(Vector3 pivotPos)
        {
            switch (CurrentSkill.attackRange)
            {
                case AttackRange.FAN_L:
                    _fanEffect.transform.localScale = new Vector3(3f, 3f, 3f);
                    _fanEffectRender.material.SetVector("_Angle", new Vector2(0,0.8f));
                    break;
                case AttackRange.FAN_SL:
                    _fanEffect.transform.localScale = new Vector3(5f, 5f, 5f);
                    _fanEffectRender.material.SetVector("_Angle", new Vector2(0,2f));
                    break;
            }
            pivotPos.y = 0.01f;
            _fanEffect.transform.position = pivotPos;
        }

        public void FanDirection(Vector3 forward)
        {
            _fanEffect.transform.localRotation = Quaternion.LookRotation(forward)*Quaternion.Euler(90f,0f,0f);
        }
        

        #endregion
        
        #region Circle Effect

        private GameObject _circleEffect;
        private SpriteRenderer _circleEffectRender;
        
        public void AllowCircleEffect(bool value)
        {
            _circleEffect.SetActive(value);
        }

        public void ResetCircleEffect()
        {
            AllowCircleEffect(false);
            ResetEffect();
        }
        public void CircleRange(Vector3 pivotPos)
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.CIRCLE_L:
                    _circleEffect.transform.localScale = new Vector3(4f, 4f, 4f);
                    break;
                case AttackRange.CIRCLE_M:
                    _circleEffect.transform.localScale = new Vector3(2f, 2f, 2f);
                    break;
                case AttackRange.CIRCLE_S:
                    _circleEffect.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    break;
            }
            pivotPos.y = 0.01f;
            _circleEffect.transform.position = pivotPos;
        }

        #endregion
        
        #region Line Effect

        private GameObject _lineEffect;
        private LineRenderer _lineEffectRender;

        public void ResetLineEffect()
        {
            AllowLineEffect(false);
            ResetEffect();
        }
        public void AllowLineEffect(bool value)
        {
            _lineEffect.SetActive(value);
        }
        public void SetLineEffectRender(Vector3 startPos, Vector3 endPos)
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.LINE_L:
                    _lineEffectRender.startWidth = 1f;
                    _lineEffectRender.endWidth = 1f;
                    break;
                case AttackRange.LINE_M:
                    _lineEffectRender.startWidth = 0.6f;
                    _lineEffectRender.endWidth = 0.6f;
                    break;
            }
            startPos.y = 0.01f;
            endPos.y = 0.01f;
            _lineEffectRender.SetPosition(0, startPos);
            _lineEffectRender.SetPosition(1, endPos);
        }

        #endregion

        #region SingleEffect

        public void ResetSingleEffect()
        {
            ResetEffect();
        }

        #endregion
        
        #region Reset Effect

        private void ResetEffect()
        {
            _effectTargets.Clear();
            foreach (var entry in _effectInfos) 
            {
                entry.Value.outline.enabled = false;
            }
            _effectInfos.Clear();
        }

        private void ResetEffects()
        {
            if(!CurrentSkill) return;
            switch (CurrentSkill.attackRange) {
                case AttackRange.ALL:
                    ResetAllEffect();
                    break;
                case AttackRange.FAN_L:
                case AttackRange.FAN_SL:
                    ResetFanEffect();
                    break;
                case AttackRange.CIRCLE_L:
                case AttackRange.CIRCLE_M:
                case AttackRange.CIRCLE_S:
                    ResetCircleEffect();
                    break;
                case AttackRange.LINE_L:
                case AttackRange.LINE_M:
                    ResetLineEffect();
                    break;
                case AttackRange.SINGLE:
                    ResetSingleEffect();
                    break;
            }
        }
        
        public void TriggerEffectToCurrentTarget()
        {
            BattleManager.BattleUiSystem.ShowStatusBars();
            if(MainBattleCharacter.Key != CharacterNameEnum.None)
                BattleManager.BattleStatusSystem.ApplyAccumulateCost(MainBattleCharacter.Status);
            foreach (var target in _effectTargets)
            {
                var status = _effectInfos[target].status;
                if (!CurrentSkill || !status) return;
                BattleManager.BattleStatusSystem.ApplyStatusEffect(status);
            }
        }

        #endregion

        #region Delay Copy Effect

        public GameObject CopyASkillEffect()
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.FAN_L:
                case AttackRange.FAN_SL:
                    return _fanEffect;
                case AttackRange.CIRCLE_S:
                case AttackRange.CIRCLE_M:
                case AttackRange.CIRCLE_L:
                    return _circleEffect;
                case AttackRange.LINE_M:
                case AttackRange.LINE_L:
                    return _lineEffect;
            }
            return null;
        }
        

        #endregion
        
        private async void RegisterSkillEffects()
        {
            _lineEffect = await Addressables.InstantiateAsync(BattleScene.LineEffect, transform, true).Task;
            _circleEffect = await Addressables.InstantiateAsync(BattleScene.CircleEffect, transform, true).Task;
            _fanEffect = await Addressables.InstantiateAsync(BattleScene.FanEffect, transform, true).Task;
            
            if (_lineEffect) {
                _lineEffectRender = _lineEffect.GetComponent<LineRenderer>();
                _lineEffect.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
                _lineEffect.SetActive(false);
            }
            if (_circleEffect) {
                _circleEffectRender = _circleEffect.GetComponent<SpriteRenderer>();
                _circleEffect.SetActive(false);
            }
            if (_fanEffect)
            {
                _fanEffectRender = _fanEffect.GetComponent<SpriteRenderer>();
                _fanEffect.SetActive(false);
            }
        }
        private void Awake()
        {
            _effectInfos = new();
            _toRemove = new();
            _effectTargets = new();
            RegisterSkillEffects();
        }
        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("OnSkillSelected", ResetEffects);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled", ResetEffects);
        }
        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("OnSkillSelected", ResetEffects);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled", ResetEffects);
        }
    }
}
