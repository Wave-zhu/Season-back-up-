using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Season.ScriptableObject
{
    [Serializable]
    public enum CharacterNameEnum
    {
        None = -1,
        Agnes = 0,
        Sizina = 1,
        Quatre = 2,
        Ganyu = 3,
        Altina = 4,
        E_Agnes = 100,
        E_Sizina = 101,
        E_Quatre = 102,
        E_Ganyu = 103,
        E_Altina = 104,
    }
    public class BaseCharacter
    {
        public CharacterNameEnum Key;
        public GameObject Member;
    }
    
    [CreateAssetMenu(fileName = "CharacterDataBase", menuName = "Create/CharacterDataBase", order = 0)]
    [Serializable]
    public class CharacterDataBaseSO : UnityEngine.ScriptableObject
    {
        public List<CharacterNameEnum> characters;
    }
}