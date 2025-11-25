using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FRAMEWORK
{
    [ModulePriority(0)]
    public class Config : IModule
    {
        public UniTask OnInit()
        {
            Debug.Log("Config模块初始化成功");
            return UniTask.CompletedTask;
        }

        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}
