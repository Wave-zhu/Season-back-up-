using UnityEngine;

namespace Units.Tools
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        protected static T _instance;
        private static object _lock = new object();
        public static T MainInstance//don't call it at disable, destroy
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = FindObjectOfType<T>() as T; //先去场景中找有没有这个类
                        if (_instance == null)//如果没有，那么我们自己创建一个Gameobject然后给他加一个T这个类型的脚本，并赋值给instance;
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            _instance = go.AddComponent<T>();
                        }
                    }
                }

                return _instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); // 销毁额外的实例
                return;
            }

            _instance = (T)this;
            DontDestroyOnLoad(gameObject); // 只有一个实例时，设置跨场景保留
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }
    }
    
}