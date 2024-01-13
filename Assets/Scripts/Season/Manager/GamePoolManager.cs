using System.Collections.Generic;
using Season.SceneBehaviors;
using Units.Tools;
using UnityEngine;

namespace Season.Manager
{
    [System.Serializable]
    public struct PoolItem
    {
        public string Name;
        public GameObject Item;
        public int Size;
    }
    public class GamePoolManager : Singleton<GamePoolManager>
    {
        
        private SceneBehavior _sceneBehavior;
        public SceneBehavior SceneBehavior
        {
            get => _sceneBehavior;
            set
            {
                if (_sceneBehavior != null)
                {
                    ClearPool();
                }
                _sceneBehavior = value;
                InitPool();
            } 
        }
    
        private Dictionary<string, Queue<GameObject>> _poolCenter;
        private Dictionary<string, GameObject> _subPool;
        private GameObject _poolItemParent;
        private async void InitPool()
        {
            if(_sceneBehavior == null) return;
            await _sceneBehavior.LoadPoolItems();
            if(_sceneBehavior.PoolItems.Count == 0) return;
            //divide
            foreach (var prefab in _sceneBehavior.PoolItems)
            {
                for(int j = 0; j < prefab.Size; j++)
                {
                    if (!_poolCenter.ContainsKey(prefab.Name))
                    {
                        _poolCenter.Add(prefab.Name, new Queue<GameObject>());
                        _subPool.Add(prefab.Name, new GameObject(prefab.Name));
                        _subPool[prefab.Name].transform.SetParent(_poolItemParent.transform);
                    }
                    var item = Instantiate(prefab.Item, _subPool[prefab.Name].transform);
                    item.SetActive(false);
                    _poolCenter[prefab.Name].Enqueue(item);
                }
            }
        }
        private void ClearPool()
        {
            _sceneBehavior.ReleasePoolItems();
            foreach (var entry in _subPool)
            {
                Destroy(entry.Value);
            }
            _subPool.Clear();
            foreach (var entry in _poolCenter)
            {
                foreach (var item in entry.Value)
                {
                    Destroy(item);
                }
                entry.Value.Clear();
            }
            _poolCenter.Clear();
        }

        public GameObject TryGetPoolItem(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (_poolCenter.ContainsKey(prefabName) && !_poolCenter[prefabName].Peek().activeSelf)
            {
                var item = _poolCenter[prefabName].Dequeue();
                item.transform.position = position;
                item.transform.rotation = rotation;
                item.SetActive(true);
                _poolCenter[name].Enqueue(item);
                return item;
            }
            else
            {
                DevelopmentTools.WTF("there is no pool named "+ name);
                return null;
            }
        }

        public GameObject TryGetPoolItem(string prefabName)
        {
            if (_poolCenter.ContainsKey(prefabName) && !_poolCenter[prefabName].Peek().activeSelf)
            {
                var item = _poolCenter[prefabName].Dequeue();
                item.SetActive(true);
                _poolCenter[prefabName].Enqueue(item);
                return item;
            }
            DevelopmentTools.WTF("there is no pool named " + name);
            return null;
        }


        protected override void Awake()
        {
            base.Awake();
            _poolCenter = new();
            _subPool = new();
        }

        private void Start()
        {
            _poolItemParent = new GameObject("Pool Parent");
            _poolItemParent.transform.SetParent(transform);
            InitPool();
        }
    }
}
