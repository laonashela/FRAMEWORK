using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FRAMEWORK
{
    public class Root : Singleton<Root>
    {
        public Config Config { get; private set; }
        public UI UI { get; private set; }
        public Audio Audio { get; private set; }

        private List<IModule> _modulesList = new List<IModule>();
        private bool _isInitialized = false;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);

            AutoRegisterModules();

            Init();
        }
        /// <summary>
        /// 核心黑科技：通过反射自动实例化并赋值给属性
        /// </summary>
        private void AutoRegisterModules()
        {
            // 1. 获取 GameRoot 下所有公开的属性 (UI, Config, Audio...)
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 临时列表用于排序
            var tempModules = new List<(int priority, IModule module, PropertyInfo prop)>();

            foreach (var prop in properties)
            {
                // 只处理实现了 IModule 接口的属性
                if (typeof(IModule).IsAssignableFrom(prop.PropertyType))
                {
                    // 创建实例 (相当于 new UIManager())
                    var moduleInstance = (IModule)Activator.CreateInstance(prop.PropertyType);

                    // 获取该模块类的优先级特性
                    var priorityAttr = prop.PropertyType.GetCustomAttribute<ModulePriorityAttribute>();
                    int priority = priorityAttr != null ? priorityAttr.Priority : 100; // 没标签的默认100

                    tempModules.Add((priority, moduleInstance, prop));
                }
            }

            // 2. 按照优先级排序 (从小到大)
            var sortedModules = tempModules.OrderBy(x => x.priority);

            // 3. 依次赋值并加入管理列表
            foreach (var item in sortedModules)
            {
                // 将创建好的实例赋值给 GameRoot 的对应属性 (例如 this.UI = instance)
                item.prop.SetValue(this, item.module);

                // 加入列表用于后续 Update
                _modulesList.Add(item.module);

                Debug.Log($"[系统] 模块自动装载: {item.module.GetType().Name} (优先级: {item.priority})");
            }
        }
        private async void Init()
        {
            foreach (var module in _modulesList)
            {
                await module.OnInit();
            }

            _isInitialized = true;

            //加载另一个场景
            SceneManager.LoadScene(1);
        }
    }
}

