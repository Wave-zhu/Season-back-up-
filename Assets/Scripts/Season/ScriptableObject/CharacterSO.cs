using UnityEngine;

namespace Season.ScriptableObject
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Create/CharacterData", order = 0)]
    public class CharacterSO : UnityEngine.ScriptableObject
    {
        public CharacterNameEnum characterName;
        public int level;


        public int attack;
        public int defense;
    
        public int maxHealth;
        public int maxBreakGauge;
        public int maxAccumulate;
        public int moveRange;
    }
}