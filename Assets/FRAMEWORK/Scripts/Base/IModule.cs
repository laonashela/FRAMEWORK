using Cysharp.Threading.Tasks;
using System;

namespace FRAMEWORK
{
    public interface IModule
    {
        UniTask OnInit();
        void OnUpdate();
        void Release();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModulePriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public ModulePriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
