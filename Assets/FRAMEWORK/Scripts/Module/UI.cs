using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace FRAMEWORK
{
    [ModulePriority(1)]
    public class UI : IModule
    {
        private GameObject _uiRoot;
        private const string UIRootName = "[UI]";

        public UniTask OnInit()
        {
            _uiRoot = GameObject.Find(UIRootName);

            Debug.Log("UI模块初始化成功");
            return UniTask.CompletedTask;
        }

        public void OnUpdate()
        {
            //throw new System.NotImplementedException();
        }

        public void Release()
        {
            //throw new System.NotImplementedException();
        }

        public async UniTask<GameObject> OpenPanel(string panelName)
        {
            if (_uiRoot == null)
            {
                Debug.LogError("[UIManager] 模块尚未初始化成功或 UI 根对象丢失！");
                // 抛出异常，让外部调用者知道加载失败
                throw new InvalidOperationException("UI module not initialized.");
            }

            var resourceRequest = Resources.LoadAsync<GameObject>(panelName);
            GameObject panelPrefab = await resourceRequest as GameObject;

            if (panelPrefab == null)
            {
                Debug.LogError($"[UIManager] 错误：找不到 Resources/{panelName} 资源文件！");
                return null;
            }

            GameObject panelInstance = GameObject.Instantiate(panelPrefab, _uiRoot.transform);
            return panelInstance;
        }
    }
}
