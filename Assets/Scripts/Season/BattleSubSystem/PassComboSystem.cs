using System;
using Season.AssetLibrary;
using System.Collections.Generic;
using System.Linq;
using Season.Battle;
using Season.Manager;
using Season.ScriptableObject;
using Season.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.BattleSubSystem
{
    public class PassComboSystem : InActionSystem
    {
        public SkillSO PassCombo { get; set; }
        public PassComboIndicator PassComboIndicator { get; set; }
        
        private int _graphNum;

        private int _party;
        
        private readonly Dictionary<int, int> _graph = new()
        {
            {12, 1 << 11}, {13, 1 << 10}, {14, 1 << 9}, {1, 1 << 11 | 1 << 10 | 1 << 9 | 1 << 8 | 1 << 5 | 1 << 2 },
            {21, 1 << 8}, {23, 1 << 7}, {24, 1 << 6}, {2, 1 << 8 | 1 << 7 | 1 << 6 | 1 << 11 | 1 << 4 | 1 << 1 },
            {31, 1 << 5}, {32, 1 << 4}, {34, 1 << 3}, {3, 1 << 5 | 1 << 4 | 1 << 3 | 1 << 10 | 1 << 7 | 1 << 0 },
            {41, 1 << 2}, {42, 1 << 1}, {43, 1 << 0}, {4, 1 << 2 | 1 << 1 | 1 << 0 | 1 << 9 | 1 << 6 | 1 << 3 },
        };
        private readonly Dictionary<int, int> _circles= new()
        {
            {2304, 2},{1056, 2},{516, 2},{144, 2},{66, 2},{9, 2},
            {2208, 3},{2116, 3},{1296, 3},{1036, 3},{770, 3},{545, 3},{138, 3},{81, 3},
            {2188, 4},{2145, 4},{1108, 4},{1290, 4},{674, 4},{785, 4},
        };

        private static int GetId(CharacterNameEnum key) => (int)(object) key;
        
        public void UnBindAPlayer(CharacterNameEnum key)
        {
            int id = GetId(key);
            if (id is < 0 or >= 4) return;
            PassComboIndicator.SetAbility(id, false);
            RemoveEdges(id);
        }
        
        public void TryBindAPlayer(int id)
        {
            if (id is < 0 or >= 4) return;
            PassComboIndicator.SetAbility(id, BattleCharacters.ContainsKey((CharacterNameEnum) id));
        }

        public void PassAddCheck()
        {
            if(CurrentSkill.priorityInfo != -1) return;
            foreach (var member in CurrentSkill.friendPassList.Where(member => BattleManager.BattleStatusSystem.CheckCharacterEnable(member)))
            {
                AddAEdge(GetId(MainBattleCharacter.Key), GetId(BattleCharacters[member].Key));
            }
        }
        private void AddAEdge(int begin, int end)
        {
            if(begin == end) return;
            if(!AbleToPass(begin, end)) return;
            PassComboIndicator.SetArrowEffect(begin, end);
            _graphNum |= _graph[(begin + 1) * 10 + end + 1];
            CountCircle();
        }
        private void RemoveEdges(int begin)
        {
            for (int i = 0; i < 4; i++) 
            {
                if(begin == i) continue;
                PassComboIndicator.RemoveArrowEffect(begin, i);
            }
            int temp = _graph[begin + 1];
            _graphNum ^= _graphNum & temp;
            CountCircle();
        }

        private bool AbleToPass(int begin, int end)
        {
            var temp = _graph[(begin + 1) * 10 + end + 1];
            return  PassComboIndicator.AbleToPass(begin, end) && (_graphNum & temp) == 0;
        }
        
        private int _doubleAttackCount;
        private int _tripleAttackCount;
        private int _quadrupleAttackCount;
        
        private void CountCircle()
        {
            _doubleAttackCount = 0;
            _tripleAttackCount = 0;
            _quadrupleAttackCount = 0;
            foreach (var circle in _circles.Where(circle => (_graphNum & circle.Key) == circle.Key))
            {
                switch (circle.Value)
                {
                    case 2:
                        _doubleAttackCount++;
                        break;
                    case 3:
                        _tripleAttackCount++;
                        break;
                    case 4:
                        _quadrupleAttackCount++;
                        break;
                }
            }
            PassComboIndicator.SetVisualPassCount(_doubleAttackCount,_tripleAttackCount,_quadrupleAttackCount);
        }

        private void TickParty()
        {
            int res = 0;
            for (int i = 0; i < 4; i++)
            {
                if (_circles.Where(circle => (_graphNum & circle.Key) == circle.Key).Any(circle => (circle.Key & _graph[i + 1]) != 0))
                {
                    res |= 1 << i;
                }
            }
            _party = res;
        }
        
        //TODO
        public async void PassAttack()
        {
            TickParty();
            PassCombo = await Addressables.LoadAssetAsync<SkillSO>("PassCombo").Task;
            PassCombo.baseHealthValue = _doubleAttackCount * 200 + _tripleAttackCount * 400 + _quadrupleAttackCount * 800;
            
            for (int i = 0; i < 4; i++)
            {
                if ((_party & 1 << i) == 0) continue;
                var battleCharacter = BattleCharacters[(CharacterNameEnum)i];
                battleCharacter.Info.NextState = BattleState.DISABLE;
                BattleManager.BattleSequenceSystem.AddToRemove(battleCharacter);
            }

            if (_quadrupleAttackCount > 0)
            {
                PassCombo.resultType = ResultType.MAGIC;
            }
            else if (_tripleAttackCount > 0)
            {
                PassCombo.resultType = ResultType.NORMAL;
            }
            else
            {
                PassCombo.resultType = ResultType.MAGIC;
            }

            BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CharacterNameEnum.None], new SkillAllParam()
            {
                skill = PassCombo,
                caster = CharacterNameEnum.None,
            }, false);
            BattleManager.BattleSequenceSystem.CreateACustomAction(BattleCharacters[CharacterNameEnum.None], BattleState.AD_LIB, BattleState.DISABLE);
        }
        
        public void InitGraph()
        {
            _graphNum = 0;
            for (int i = 0; i < 4; i++)
            {
                PassComboIndicator.SetName(i, ((CharacterNameEnum)i).ToString());
            }
            PassComboIndicator.ResetArrows();
            PassComboIndicator.DisableFlagButton();
            PassComboIndicator.SetVisualPassCount(0,0,0);
        }

        private void Awake()
        {
            _party = new();
        }
    }
}