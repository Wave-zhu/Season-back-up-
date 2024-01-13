using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Season.Character;
using Season.Input;
using Season.ScriptableObject;
using Units.Tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace Season.Manager
{
    public class PlayerManager : Singleton<PlayerManager>,CharacterAsset
    {
        private CharacterDataBaseSO _players;
        private List<BaseCharacter> _playerParty;
        public List<BaseCharacter> GetPartyMembers() => _playerParty;

        public Vector3 CentralLocation
        {
            get
            {
                Vector3 temp = _playerParty.Aggregate(Vector3.zero, (current, player) => current + player.Member.transform.position);
                temp /= _playerParty.Count;
                temp.y = 1.7f;
                return temp;
            }
        }

        private void MainPlayerChangedOnField(GameObject mainPlayer)
        {
            GameEventManager.MainInstance.CallEvent("OnMainPlayerChangedOnField",mainPlayer);
            GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged",mainPlayer, MoveAbility.UNLIMITED, MoveAbility.AGENT_FOLLOWING);
        }
    
        private void SwitchPlayer(InputAction.CallbackContext context)
        {
            var mainPlayer = _playerParty[0];
            for(int i = 0; i < _playerParty.Count - 1; i++) 
            {
                _playerParty[i] = _playerParty[i + 1];
            }
            _playerParty[^1] = mainPlayer;
            MainPlayerChangedOnField(_playerParty[0].Member);
        }

        public async void ReParty()
        {
            _playerParty.Clear();
            _players = await Addressables.LoadAssetAsync<CharacterDataBaseSO>("DefaultPlayers").Task;
            foreach (var player in _players.characters)
            {
                _playerParty.Add(await CreateBaseCharacter(player));
            }
            GameInputManager.FieldGameInputAction.Enable();
            // begin point!
            MainPlayerChangedOnField(_playerParty[0].Member);
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
            _characterInfo[key] = await Addressables.LoadAssetAsync<GameObject>(key.ToString()).Task;
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
            _characterCSInfo[key] = await Addressables.LoadAssetAsync<GameObject>("CS_" + key).Task;
            return Instantiate(_characterCSInfo[key]);
        }
        
        public async Task<GameObject> CreateGhostMember(CharacterNameEnum key)
        {
            if (_characterGHInfo.TryGetValue(key, out var info))
            {
                return Instantiate(info);
            }
            _characterGHInfo[key] = await Addressables.LoadAssetAsync<GameObject>("GH_" + key).Task;
            return Instantiate(_characterGHInfo[key]);
        }
        
        #endregion
        
        
        
        protected override void Awake()
        {
            base.Awake();
            _characterInfo = new();
            _characterCSInfo = new();
            _characterGHInfo = new();
            _playerParty = new();
        }
        protected void Start()
        {
            ReParty();
        }
        
        private void OnEnable()
        {
            GameInputManager.FieldGameInputAction.GameInput.Switch.performed += SwitchPlayer;
        }
        private void OnDisable()
        {
            GameInputManager.FieldGameInputAction.GameInput.Switch.performed -= SwitchPlayer;
        }
    }
}
