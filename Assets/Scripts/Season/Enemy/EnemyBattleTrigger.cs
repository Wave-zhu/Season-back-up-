using System;
using System.Collections.Generic;
using Season.Character;
using Season.Manager;
using Season.Player;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    [Serializable]
    public class EnemyBattleTrigger : MonoBehaviour
    {
        [SerializeField] protected CharacterDataBaseSO _enemies;
        [SerializeField] List<GameObject> _fieldEnemies;
        protected SphereCollider _battleTrigger;
    
        private void Awake()
        {
            _battleTrigger = GetComponent<SphereCollider>();
            _battleTrigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var battleInfo = other.GetComponent<CharacterBattleBase>();
            if (!battleInfo) return;
            if (battleInfo is not PlayerBattle) return;
            
            BattleManager.BattleFieldInteractiveSystem.SetBattleField(transform.position);
            EnemyManager.MainInstance.RegisterEnemyPreBattle(_enemies, _fieldEnemies);
            BattleManager.MainInstance.BattleBegin();
            gameObject.SetActive(false);
        }
    }
}
