using Season.Battle;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Season.Animation;
using Season.Character;
using Season.Enemy;
using Season.Manager;
using Season.Player;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using Season.UI;
using Units.Timer;
using Units.Tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Season.BattleSubSystem
{
    #region Classes
    public class BattleCharacter: BaseCharacter
    {
        // sequence for action
        public int Index;
        public CharacterMovementControlBase Move;
        public CharacterBattleBase Info;
        public CharacterStatusBase Status;

        public BattleCharacter()
        {
            
        }
        public BattleCharacter(BaseCharacter baseCharacter)
        {
            Key = baseCharacter.Key;
            Member = baseCharacter.Member;
            Move = baseCharacter.Member.GetComponent<CharacterMovementControlBase>();
            Info =  baseCharacter.Member.GetComponent<CharacterBattleBase>();
            Status = baseCharacter.Member.GetComponent<CharacterStatusBase>();
        }
    }
    
    internal class BattleIcon
    {
        public BattleCharacter BattleCharacter;
        
        //for icon content
        public ActionIcon Icon;
        // sequence for icon
        public int Index;
        // about icon animation
        public float Offset;
        public RectTransform IconRectTransform;
        public IconMath IconMath;
    
        public static BattleIcon CreateABattleIcon(BattleCharacter battleCharacter, Transform parent)
        {
            var scene = (BattleScene)SceneAssets.MainInstance.SceneBehavior;
            GameObject battleIcon = Object.Instantiate(scene.ActionIcon, parent);
            battleIcon.SetActive(false);
            return new BattleIcon() {
                BattleCharacter = battleCharacter,
                Icon = battleIcon.GetComponent<ActionIcon>(),
                IconRectTransform = battleIcon.GetComponent<RectTransform>(),
                IconMath = battleIcon.GetComponent<IconMath>(),
            };
        }
    }
    #endregion
    
    public class BattleSequenceSystem : InActionSystem
    {
        private enum SequenceWorkFlow
        {
            NONE,
            PRE_TURN_BEGIN,
            TURN_BEGIN,
            PRE_ACTION_BEGIN,
            ACTION_BEGIN,
            ACTION_PREVIEW,
            ACTION_DEALING,
            STATUS_UPDATE,
            ACTION_END,
            BEFORE_NEXT_ACTION,
        }

        public int ActionCount { get; private set; }
        public int TurnCount { get; private set; }

        private SequenceWorkFlow _sequenceState = SequenceWorkFlow.NONE;
        private Transform _sequenceBackground;
        
        private List<BattleCharacter> _actionSequence;

        private HashSet<BattleCharacter> _toRemove;
        private Dictionary<BattleCharacter, int> _toChange;
        
        // (icon sequence)
        private List<BattleIcon> _iconSequence;
        //the current sequence
        private Dictionary<BattleCharacter, BattleIcon> _currentBattleIcons;
        //the preview sequence, only the effected member need to readdress the ui
        private Dictionary<BattleCharacter, BattleIcon> _previewBattleIcons;

        #region BattleBegin---->TurnBegin
        public void StartABattle()
        {
            SceneAssets.AnimSubSystem.PlayAPostEffect(Resources.Load<TimelineAsset>("TimeLine/Ui/BattleBegin"));
        }

        public async void RegisterBattle()
        {
            _sequenceState = SequenceWorkFlow.PRE_TURN_BEGIN;
            SequenceReset();
            await BattleManager.BattleUiSystem.OpenWidget(BattleUi.SEQUENCE_LIST);
            _sequenceBackground = BattleManager.BattleUiSystem.GetCache(BattleUi.SEQUENCE_LIST, SceneAssets.MainInstance.SceneBehavior).transform;
            await RegisterCustomDefaultCharacter();
            var enemies = EnemyManager.MainInstance.GetPartyMembers();
            var players = PlayerManager.MainInstance.GetPartyMembers();
            //ADD Current Enemies
            Enemies = enemies.Select(item =>item.Member).ToList();
            RegisterMembers(enemies);
            
            //ADD Current Players
            Players = players.Select(item =>item.Member).ToList();
            RegisterMembers(players);
        }

        public async void DisplayBattle()
        {
            await BattleManager.BattleUiSystem.OpenWidget(BattleMenu.PASS_COMBO_INDICATOR);
            TurnBegin();
        }
        private void SequenceReset()
        {
            _actionSequence.Clear();
            BattleCharacters.Clear();
        }
        #endregion

        #region Register
        private async Task RegisterCustomDefaultCharacter()
        {
            var model = await Addressables.InstantiateAsync("CustomDefaultCharacterModel", transform).Task;
            model.transform.position = BattleManager.BattleFieldInteractiveSystem.FieldCenter;
            var battleCharacter = new BattleCharacter() 
            {
                Key = CharacterNameEnum.None,
                Member = model,
                Move = model.GetComponent<CharacterMovementControlBase>(),
                Info =  model.GetComponent<CharacterBattleBase>(),
                Status = model.GetComponent<CharacterStatusBase>(),
            };
            BattleCharacters[battleCharacter.Key] = battleCharacter;
            RegisterCurrentBattleIcon(battleCharacter);
            RegisterPreviewBattleIcon(battleCharacter);
        }
        private void RegisterMembers(List<BaseCharacter> baseCharacters)
        {
            int index = 0;
            foreach (var baseCharacter in baseCharacters) 
            {
                var battleCharacter = new BattleCharacter(baseCharacter);
                battleCharacter.Move.SwitchMoveAbility(MoveAbility.NO_MOVE);
                BattleManager.BattleUiSystem.RegisterStatusBar(battleCharacter.Status, index);
                BattleCharacters[battleCharacter.Key] = battleCharacter;
                RegisterCurrentBattleIcon(battleCharacter);
                RegisterPreviewBattleIcon(battleCharacter);
                index++;
            }
        }
        private void RegisterCurrentBattleIcon(BattleCharacter battleCharacter)
        {
            _currentBattleIcons[battleCharacter] = BattleIcon.CreateABattleIcon(battleCharacter, _sequenceBackground);
            _currentBattleIcons[battleCharacter].Icon.SetName(battleCharacter.Key.ToString());
        }
        private void RegisterPreviewBattleIcon(BattleCharacter battleCharacter)
        {
            _previewBattleIcons[battleCharacter] = BattleIcon.CreateABattleIcon(battleCharacter, _sequenceBackground);
            _previewBattleIcons[battleCharacter].Icon.SetName(battleCharacter.Key.ToString());
        }

        #endregion

        #region TurnBegin ----> ActionBegin
        
        private void TurnBegin()
        {
            TurnCount++;
            DevelopmentTools.WTF("Turn" + TurnCount);
            ActionCount = 0;
            _sequenceState = SequenceWorkFlow.TURN_BEGIN;
            //init member with current live members
            foreach (var entry in BattleCharacters) 
            {
                if(entry.Value.Key != CharacterNameEnum.None)
                    _actionSequence.Add(entry.Value);
            }
            //random select
            int n = _actionSequence.Count;
            for (int i = 0; i < n; i++) 
            {
                int random = Random.Range(i, n);
                (_actionSequence[i],_actionSequence[random]) = (_actionSequence[random],_actionSequence[i]);
            }
            ResetAtTurnBegin();
            
            //pre action begin
            ResetToThisAction();
            
            GameEventManager.MainInstance.CallEvent("TryEnablePassAttack");
            //action begin
            ActionBegin();
        }

        private void ResetAtTurnBegin()
        {
            BattleManager.PassComboSystem.InitGraph();
            foreach (var battleCharacter in _actionSequence)
            {
                battleCharacter.Info.BattleInit();
            }
            //Reset Able Count, Pass Icon
            BattleManager.BattleStatusSystem.ResetPlayerEnable();
            //make sure reset all ui stuff
            ResetIconsInfo();
        }
        private void ResetIconsInfo()
        {
            foreach (var battleCharacter in _actionSequence)
            {
                _currentBattleIcons[battleCharacter].Icon.gameObject.SetActive(true);
                _currentBattleIcons[battleCharacter].Icon.SetStateInfo(battleCharacter.Info);
                _currentBattleIcons[battleCharacter].Icon.SetTransparency(1f);
            }
        }
        
        public void ResetToThisAction()
        {
            _sequenceState = SequenceWorkFlow.PRE_ACTION_BEGIN;
            for (int i = 0; i < _actionSequence.Count; i++) 
            {
                var battleCharacter = _actionSequence[i];
                battleCharacter.Index = i;
                //very important!
                battleCharacter.Info.CurrentState = battleCharacter.Info.CurrentState;
                
                var currentBattleIcon = _currentBattleIcons[battleCharacter];
                currentBattleIcon.IconRectTransform.anchoredPosition = new Vector2(100 + 200 * i, 0);
                currentBattleIcon.Index = i;
                currentBattleIcon.Offset = 0f;
                
                var previewBattleIcon = _previewBattleIcons[battleCharacter];
                previewBattleIcon.Index = 0;
                previewBattleIcon.Icon.gameObject.SetActive(false);
            }
            _toChange.Clear();
            _toRemove.Clear();
        }
        
        #endregion

        #region Action Begin
        private void ActionBegin()
        {
            ActionCount++;
            DevelopmentTools.WTF("Action:" + ActionCount);
            GameEventManager.MainInstance.CallEvent("TryEnablePassAttack");
            SkillItem.hasCheckThisTurn = false;
            _sequenceState = SequenceWorkFlow.ACTION_BEGIN;
            // register mainMember for all(the only entrance)
            MainBattleCharacter = _actionSequence[0];
            var battleInfo = MainBattleCharacter.Info;
            switch (battleInfo) {
                case PlayerBattle playerBattle:
                    switch (playerBattle.CurrentState) {
                        case BattleState.AD_LIB:
                            AdLibAction();
                            break;
                        case BattleState.DELAY:
                            PlayerDelayAction();
                            break;
                        default:
                            PlayerNormalAction();
                            break;
                    }
                    break;
                case EnemyBattle enemyBattle:
                    //TODO ENEMY DELAY
                    EnemyNormalAction(MainBattleCharacter.Member);
                    break;
            }
        }

        private void PlayerNormalAction()
        {
            GameEventManager.MainInstance.CallEvent("OnMainPlayerChangedOnBattle", MainBattleCharacter.Member);
            GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged", MainBattleCharacter.Member, MoveAbility.LIMITED, MoveAbility.NO_MOVE);
            BattleManager.BattleUiSystem.ShowMenu();
        }

        private void PlayerDelayAction()
        {
            BattleManager.BattleUiSystem.HideMenu();
            GameEventManager.MainInstance.CallEvent("OnMainPlayerChangedOnBattle", MainBattleCharacter.Member);
            GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged", MainBattleCharacter.Member, MoveAbility.NO_MOVE, MoveAbility.NO_MOVE);
            GameEventManager.MainInstance.CallEvent("DisablePassAttack");
            StartCoroutine(BattleManager.PassiveSkillSystem.LoadSkills());
        }
        
        private void AdLibAction()
        {
            BattleManager.BattleUiSystem.HideMenu();
            GameEventManager.MainInstance.CallEvent("OnMainPlayerChangedOnBattle", MainBattleCharacter.Member);
            GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged", MainBattleCharacter.Member, MoveAbility.NO_MOVE, MoveAbility.NO_MOVE);
            GameEventManager.MainInstance.CallEvent("DisablePassAttack");
            StartCoroutine(BattleManager.PassiveSkillSystem.LoadSkills());
        }

        private void EnemyNormalAction(GameObject enemy)
        {
            BattleManager.BattleUiSystem.HideMenu();
            GameEventManager.MainInstance.CallEvent("DisablePassAttack");
            GameEventManager.MainInstance.CallEvent("OnMainEnemyChangedOnBattle", enemy);
            GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged", enemy, MoveAbility.LIMITED, MoveAbility.NO_MOVE);
            StartCoroutine(BattleManager.PassiveSkillSystem.LoadSkills());
        }
        #endregion

        /// <summary>
        /// after select will enter action preview
        /// </summary>
        
        #region Action Preview
        public void ActionPreview()
        {
            GameEventManager.MainInstance.CallEvent("DisablePassAttack");
            _sequenceState = SequenceWorkFlow.ACTION_PREVIEW;
            //pre sort
            
            HandleBeginInitialize();
            if(MainBattleCharacter.Info.CurrentState != BattleState.DELAY &&
               CurrentSkill && 
               CurrentSkill.friendPassList is {Count:> 0})
            {
                foreach (var member in CurrentSkill.friendPassList)
                {
                    HandlePriority(member, CurrentSkill.priorityInfo);
                }
            }
            HandleMainBattleCharacterTransition();
            HandlePreviewSort();
        }
        
        private void HandleBeginInitialize()
        {
            //clear all icon, reset all weight by current sequence
            _iconSequence = _actionSequence.Select(item => _currentBattleIcons[item]).ToList();
        }

        private void HandlePriority(CharacterNameEnum character, int priorityInfo)
        {
            BattleCharacters.TryGetValue(character, out var battleCharacter);
            if (battleCharacter == null) return;
            //only effect to member able!
            if (battleCharacter.Info.CurrentState is BattleState.DISABLE or BattleState.DEATH) return;
            
            // show that being effected
            _currentBattleIcons[battleCharacter].Offset = -400f;
            _iconSequence.Add(_previewBattleIcons[battleCharacter]);
            switch (priorityInfo)
            {
                case -1:
                    HandlePassPresort(battleCharacter);
                    BattleStateRules.PassStateRule(battleCharacter.Info);
                    break;
                case 0:
                    break;
                default:
                    HandleDelay(battleCharacter, priorityInfo);
                    BattleStateRules.DelayStateRule(battleCharacter.Info);
                    break;
            }
        }
        private void HandlePassPresort(BattleCharacter battleCharacter)
        {
            _toChange[battleCharacter] = -1;
            _previewBattleIcons[battleCharacter].Index = _toChange[battleCharacter];
        }
        public void HandleDelay(BattleCharacter battleCharacter, int priorityInfo)
        {
            //simple delay
            _toChange[battleCharacter] = Mathf.Min(battleCharacter.Index + priorityInfo + 1, BattleCharacters.Count);
            _previewBattleIcons[battleCharacter].Index = _toChange[battleCharacter];
        }
        
        
        private void HandleMainBattleCharacterTransition()
        {
            if (!_toChange.ContainsKey(MainBattleCharacter))
            {
                _iconSequence.Add(_previewBattleIcons[MainBattleCharacter]);
                _currentBattleIcons[MainBattleCharacter].Offset = -400f;
                _toChange[MainBattleCharacter] = BattleCharacters.Count + 1;
                _previewBattleIcons[MainBattleCharacter].Index = _toChange[MainBattleCharacter];
                BattleStateRules.NormalActionStateRules(MainBattleCharacter.Info);
            }
            switch (MainBattleCharacter.Info.NextState)
            {
                case BattleState.DELAY:
                    if(MainBattleCharacter.Info.CurrentState != BattleState.DELAY) break;
                    _toChange[MainBattleCharacter] = -1;
                    _previewBattleIcons[MainBattleCharacter].Index = -1;
                    break;
                case BattleState.AD_LIB:
                    _toChange[MainBattleCharacter] = -1;
                    _previewBattleIcons[MainBattleCharacter].Index = -1;
                    break;
                case BattleState.DISABLE:
                    AddToRemove(MainBattleCharacter);
                    break;
                case BattleState.UNLIMITED:
                    if(BattleManager.BattleStatusSystem.PlayerEnableCount == 0)
                        MainBattleCharacter.Info.NextState = BattleState.LIMITED;
                    break;
            }
        }

        private void HandlePreviewSort()
        {
            //sort by weight
            _iconSequence = _iconSequence.OrderBy(item => item.Index).ThenBy(item => item.BattleCharacter.Index).ToList();
            for (int i = 0; i < _iconSequence.Count; i++) 
            {
                //update current icon position
                _iconSequence[i].Index = i;
            }
            
            foreach (var battleCharacter in _actionSequence)
            {
                var currentIcon = _currentBattleIcons[battleCharacter];
                //refresh with offset!!
                currentIcon.IconRectTransform.anchoredPosition = new Vector2(100 + 200 * currentIcon.Index, currentIcon.Offset);
                //pre effect, view
                var previewIcon = _previewBattleIcons[battleCharacter];
                if (_toChange.ContainsKey(battleCharacter)) //means that being effected, so the previewIcon had must be updated index
                {
                    previewIcon.IconRectTransform.anchoredPosition = new Vector2(100 + 200 * previewIcon.Index, previewIcon.Offset);
                    previewIcon.Icon.SetPreviewStateInfo(battleCharacter.Info);
                    previewIcon.Icon.gameObject.SetActive(true);
                } 
                else 
                {
                    previewIcon.Icon.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        #region Action Dealing

        public void HandleActionDealing()
        {
            _sequenceState = SequenceWorkFlow.ACTION_DEALING;
            BattleManager.ProactiveNonTargetSkillSystem.RegisterDelaySkill();
            if (CurrentSkill.priorityInfo <= 0)
            {
                BattleManager.BattleEffectSystem.TriggerEffectToCurrentTarget();
            }
            BattleManager.PassComboSystem.PassAddCheck();
        }
        public void AddToRemove(BattleCharacter battleCharacter)
        {
            _toRemove.Add(battleCharacter);
            _toChange.Remove(battleCharacter);
        }

        public void AddToChange(BattleCharacter battleCharacter, int info)
        {
            if(!_toRemove.Contains(battleCharacter))
                _toChange[battleCharacter] = info;
        }

        public void DisableIcons(BattleCharacter battleCharacter, bool thenRemove)
        {
            var icon = _currentBattleIcons[battleCharacter];
            icon.Index = _actionSequence.Count + 1;
            icon.Offset = 0f;
            icon.IconMath.StartParabolaFade(new Vector2(100 + 200 * icon.Index, icon.Offset), 0.3f);
            _previewBattleIcons[battleCharacter].Icon.gameObject.SetActive(false);
            if (thenRemove)
            {
                TimerManager.MainInstance.TryGetOneTimer(0.3f, () => RemoveIcons(battleCharacter));
            }
        }

        private void RemoveIcons(BattleCharacter battleCharacter)
        {
            if (_currentBattleIcons.TryGetValue(battleCharacter, out var currentBattleIcon))
            {
                Destroy(currentBattleIcon.Icon.gameObject);
                _currentBattleIcons.Remove(battleCharacter);
            }
            
            if (_previewBattleIcons.TryGetValue(battleCharacter, out var previewBattleIcon))
            {
                Destroy(previewBattleIcon.Icon.gameObject);
                _previewBattleIcons.Remove(battleCharacter);
            }
        }
        #endregion
        
        #region StatusUpdate

        public void HandleStatusUpdate()
        {
            _sequenceState = SequenceWorkFlow.STATUS_UPDATE;
            int i = 0;
            int j = 0;
            while (j < _actionSequence.Count)
            {
                BattleManager.BattleStatusSystem.UpdateStatus(_actionSequence[j]);
                if (!_toRemove.Contains(_actionSequence[j]))
                {
                    _actionSequence[i++] = _actionSequence[j++];
                    continue;
                }
                j++;
            }
            while (i < _actionSequence.Count)
            {
                _actionSequence.RemoveAt(_actionSequence.Count - 1);
            }
            
            _toRemove.Clear();
            HandleSort();
        }
        private void HandleSort()
        {
            _sequenceState = SequenceWorkFlow.ACTION_END;
            //only for those currentIcon which has not been effected
            _actionSequence = _actionSequence.OrderBy(item => _toChange.TryGetValue(item, out var value) ? value : item.Index).ThenBy(item => item.Index).ToList();
            _toChange.Clear();
            
            GameEventManager.MainInstance.CallEvent("TryEnablePassAttack");
            
            ResetToNextAction();
        }

        private void ResetToNextAction()
        {
            for (int i = 0; i < _actionSequence.Count; i++) 
            {
                var battleCharacter = _actionSequence[i];
                var icon = _currentBattleIcons[battleCharacter];
                //reset all index
                battleCharacter.Index = i;
                icon.Offset = 0f;
                _previewBattleIcons[battleCharacter].Icon.gameObject.SetActive(false);
                if (icon.Index == i)
                {
                    icon.IconMath.StartLine(new Vector2(100 + 200 * icon.Index, icon.Offset), 0.2f);
                    continue;
                }
                icon.Index = i;
                icon.IconMath.StartParabola(new Vector2(100 + 200 * icon.Index, icon.Offset), 0.2f);
            }
        }
        #endregion
        
        public void SimpleEnd()
        {
            CurrentSkill = null;
            ActionPreview();
            HandleStatusUpdate();
            GameEventManager.MainInstance.CallEvent("OnSkillSelected");
            PreActionBegin();
        }

        #region Before Next Action
        
        public void PreActionBegin()
        {
            _sequenceState = SequenceWorkFlow.BEFORE_NEXT_ACTION;
            if (_actionSequence.Count == 0) 
            {
                TimerManager.MainInstance.TryGetOneTimer(1f, TurnBegin);
            } 
            else
            {
                SetStateInfo();
                TimerManager.MainInstance.TryGetOneTimer(1f, ActionBegin);
            }
        }

        private void SetStateInfo()
        {
            foreach (var battleCharacter in _actionSequence) 
            {
                _currentBattleIcons[battleCharacter].Icon.SetStateInfo(battleCharacter.Info);
            }
        }
        #endregion
        
        #region Special Event
        
        private bool _isNextCustom;
        
        //Todo
        public void CreateACustomAction(BattleCharacter battleCharacter, BattleState currentState, BattleState nextState)
        {
            battleCharacter.Info.CurrentState = currentState;
            battleCharacter.Info.NextState = nextState;
            if (!_actionSequence.Contains(battleCharacter)) 
            {
                battleCharacter.Index = 0;

                var iconName = battleCharacter.Key == CharacterNameEnum.None ? "Combo" : battleCharacter.Key.ToString();
                var currentIcon = _currentBattleIcons[battleCharacter];
                currentIcon.Icon.SetName(iconName);
                currentIcon.Icon.SetTransparency(1f);
                currentIcon.Icon.gameObject.SetActive(true);
                currentIcon.Icon.SetStateInfo(battleCharacter.Info);
                currentIcon.Index = 0;
                currentIcon.IconRectTransform.anchoredPosition = new Vector2(100, 0);

                var previewIcon = _previewBattleIcons[battleCharacter];
                previewIcon.Icon.SetName(iconName);
            
                _actionSequence.Insert(0, battleCharacter);
                for (int i = 1; i < _actionSequence.Count; i++)
                {
                    var icon = _currentBattleIcons[_actionSequence[i]];
                    _actionSequence[i].Index = i;
                    icon.Index = i;
                    icon.IconMath.StartParabola(new Vector2(100 + 200 * i, 0), 0.2f);
                }
            }
            _isNextCustom = true;
        }

        #endregion
        
        private void Awake()
        {
            _actionSequence = new();
            _toRemove = new();
            _toChange = new();
            
            _iconSequence = new();
            _currentBattleIcons = new();
            _previewBattleIcons = new();
        }

        private void Update()
        {
            if (!_isNextCustom) return;
            _isNextCustom = false;
            switch (_sequenceState)
            {
               case SequenceWorkFlow.PRE_ACTION_BEGIN:
                   ActionBegin();
                   break;
               case SequenceWorkFlow.ACTION_BEGIN:
                   ActionCount--;
                   ActionBegin();
                   break;
            }
        }

        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("OnSkillEntered", ResetToThisAction);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled", ResetToThisAction);
        }
        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("OnSkillEntered", ResetToThisAction);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled", ResetToThisAction);
        }
    }
}