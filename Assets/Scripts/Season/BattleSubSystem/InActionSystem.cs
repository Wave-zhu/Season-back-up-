using System.Collections.Generic;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.BattleSubSystem
{
    public abstract class InActionSystem : MonoBehaviour
    {
        protected static BattleCharacter MainBattleCharacter { get; set; }
        public static SkillSO CurrentSkill { get; set; }
        
        
        public int PlayerEnableCount { get; set; }
        protected static List<GameObject> Players;
        protected static List<GameObject> Enemies;
        protected static Dictionary<CharacterNameEnum, BattleCharacter> BattleCharacters = new();
        //protected static Dictionary<int, BattleCharacter> BattleCharactersById = new();
        
    }
}