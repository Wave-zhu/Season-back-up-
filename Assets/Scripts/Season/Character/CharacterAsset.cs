using System.Threading.Tasks;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.Character
{
    public interface CharacterAsset
    {
        public Task<BaseCharacter> CreateBaseCharacter(CharacterNameEnum key);
        public Task<GameObject> CreateCutsceneMember(CharacterNameEnum key);
        public Task<GameObject> CreateGhostMember(CharacterNameEnum key);

    }
}