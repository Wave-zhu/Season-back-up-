using System.Collections.Generic;
using System.Threading.Tasks;
using Season.Character;
using Season.ScriptableObject;
using Units.Tools;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.Manager
{
    public class EnemyManager : Singleton<EnemyManager>, CharacterAsset
    {
        private CharacterDataBaseSO _enemies;
        private List<BaseCharacter> _enemyParty;
        public List<BaseCharacter> GetPartyMembers() => _enemyParty;
        public bool HasMember => _enemyParty != null;
    
        public Vector3 CentralLocation
        {
            get
            {
                Vector3 temp = Vector3.zero;
                foreach (var enemy in _enemyParty)
                {
                    temp += enemy.Member.transform.position;
                }
                temp /= _enemyParty.Count;
                temp.y = 1.7f;
                return temp;
            }
        }
        #region Assets
        
        private Dictionary<CharacterNameEnum, GameObject> _characterInfo;
        private Dictionary<CharacterNameEnum, GameObject> _characterCSInfo;
        private Dictionary<CharacterNameEnum, GameObject> _characterGHInfo;
        
        public async Task<BaseCharacter> CreateBaseCharacter(CharacterNameEnum key)
        {
            if (_characterInfo.TryGetValue(key, out var info))
            {
                return new BaseCharacter()
                {
                    Key = key,
                    Member = Instantiate(info),
                };
            }
            _characterInfo[key] = await Addressables.LoadAssetAsync<GameObject>(key.ToString()[2..]).Task;
            return new BaseCharacter()
            {
                Key = key,
                Member = Instantiate(_characterInfo[key]),
            };
        }
        
        public async Task<GameObject> CreateCutsceneMember(CharacterNameEnum key)
        {
            if (_characterCSInfo.TryGetValue(key, out var info))
            {
                return Instantiate(info);
            }
            _characterCSInfo[key] = await Addressables.LoadAssetAsync<GameObject>("CS_" + key.ToString()[2..]).Task;
            return Instantiate(_characterCSInfo[key]);
        }
        
        public async Task<GameObject> CreateGhostMember(CharacterNameEnum key)
        {
            if (_characterGHInfo.TryGetValue(key, out var info))
            {
                return Instantiate(info);
            }
            _characterGHInfo[key] = await Addressables.LoadAssetAsync<GameObject>("GH_" + key.ToString()[2..]).Task;
            return Instantiate(_characterGHInfo[key]);
        }
        
        #endregion
        //TODO: Shou kan
        public async Task RegisterEnemyInBattle(CharacterDataBaseSO enemies)
        {
            foreach (var enemy in enemies.characters)
            {
                _enemyParty.Add(await CreateBaseCharacter(enemy));
            }
        }
        public void RegisterEnemyPreBattle(CharacterDataBaseSO enemies, List<GameObject> enemyGameObjects)
        {
            for (int i = 0; i < enemies.characters.Count; i++)
            {
                _enemyParty.Add(new BaseCharacter()
                {
                    Key = enemies.characters[i],
                    Member = enemyGameObjects[i],
                });
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _characterInfo = new();
            _characterCSInfo = new();
            _characterGHInfo = new();
            _enemyParty = new();
        }
    }
}