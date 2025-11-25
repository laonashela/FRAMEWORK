using UnityEngine;
namespace FRAMEWORK
{
    // 约束 T 必须是 MonoBehaviour 类型，并且 T 必须有一个无参构造函数 (new())
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 静态私有字段，用于存储单例实例
        private static T _instance;

        // 静态公有属性，用于全局访问单例
        public static T Instance
        {
            get
            {
                // 1. 如果实例为空，尝试在场景中查找
                if (_instance == null)
                {
                    // FindObjectOfType 性能较低，但在初始化时只执行一次，可以接受
                    _instance = FindObjectOfType<T>();
                }

                // 2. 如果场景中仍然没有实例，则创建一个新的 GameObject
                if (_instance == null)
                {
                    // 创建一个新的 GameObject，名字设置为单例的类型名
                    GameObject singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                    // 将组件添加到新的 GameObject 上
                    _instance = singletonObject.AddComponent<T>();

                    // 可选：让单例在场景切换时不被销毁 (如果需要跨场景存在)
                    // DontDestroyOnLoad(singletonObject);
                }

                // 3. 返回实例
                return _instance;
            }
        }

        /// <summary>
        /// 在 Awake 中执行单例的初始化逻辑和生命周期管理。
        /// 确保场景中只有一个实例，并设置 _instance 引用。
        /// </summary>
        protected virtual void Awake()
        {
            // 如果 _instance 尚未设置 (即这是第一个实例)
            if (_instance == null)
            {
                // 将当前实例设置为单例
                _instance = this as T;

                // 可选：让单例在场景切换时不被销毁
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                // 如果 _instance 已经存在，并且它不是当前这个实例
                if (_instance != this)
                {
                    // 销毁重复的实例
                    Debug.LogWarning($"[Singleton] 发现重复的实例: {typeof(T).Name}，已销毁重复项。");
                    Destroy(gameObject);
                }
            }
        }

        // 如果需要执行跨场景不销毁的逻辑，可以重写此方法
        // protected void MakePersistent()
        // {
        //    if (_instance == this)
        //    {
        //        DontDestroyOnLoad(gameObject);
        //    }
        // }
    }
}