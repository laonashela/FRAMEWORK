using UnityEngine;
using UnityEngine.SceneManagement;


namespace FRAMEWORK
{
    public class Root : MonoBehaviour
    {
        public static UI UI { get; private set; }
        public static Audio Audio { get; private set; }
        public static Config Config { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Init()
        {
            Config = GetComponentInChildren<Config>();
            UI = GetComponentInChildren<UI>();
            Audio = GetComponentInChildren<Audio>();
        }
    }
}

