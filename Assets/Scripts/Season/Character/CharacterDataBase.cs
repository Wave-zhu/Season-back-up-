using Season.ScriptableObject;
using UnityEngine;

namespace Season.Character
{
    public abstract class CharacterDataBase: MonoBehaviour
    {
        [SerializeField] protected CharacterSO _characterData;
        protected CharacterNameEnum _characterName;
        protected int _characterAttack;
        protected int _characterDefense;
        public CharacterNameEnum GetCharacterName() => _characterName;

        protected virtual void Awake()
        {
            _characterName = _characterData.characterName;
            _characterAttack = _characterData.attack;
            _characterDefense = _characterData.defense;
        }
    }
}