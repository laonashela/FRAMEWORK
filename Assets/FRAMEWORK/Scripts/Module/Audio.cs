using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FRAMEWORK
{
    [ModulePriority(2)]
    public class Audio : IModule
    {
        // --- 内部常量与字段 ---
        private const string AudioRootName = "[Audio]";
        private const string BgmNodeName = "BGM";
        private const string SfxNodeName = "SFX";
        private const int SfxSourceCount = 8; // 最大同时播放的音效数量

        private GameObject _audioRoot;
        private Transform _sfxRootTransform; // 存储 SFX 根节点的 Transform
        private AudioSource _bgmSource;

        // SFX 池：初始只包含一个 AudioSource，动态增长到8个
        private List<AudioSource> _sfxSources = new List<AudioSource>();
        private int _nextSfxSourceIndex = 0; // 用于循环覆盖的索引

        // --- 核心初始化 ---

        public UniTask OnInit()
        {
            // 1. 查找场景中的 [Audio] 根节点
            _audioRoot = GameObject.Find(AudioRootName);
            if (_audioRoot == null)
            {
                Debug.LogError($"[Audio] 致命错误：场景中找不到名为 '{AudioRootName}' 的根对象！");
                throw new Exception($"Audio Root object not found: {AudioRootName}");
            }
            SetupBgmNode(_audioRoot.transform);
            SetupSfxPoolRoot(_audioRoot.transform);
            Debug.Log($"Audio模块初始化成功");
            return UniTask.CompletedTask;
        }

    

        public void OnUpdate()
        {
            // 模块通常不需要每帧更新，留空即可
        }

        public void Release()
        {
            Debug.Log("Audio模块资源释放。");

            // 清理创建的 AudioSource 节点 (虽然它们是子节点，但显式清理更好)
            if (_audioRoot != null)
            {
                // 可以只销毁创建的子节点，或者直接销毁根节点下所有子节点
                GameObject.Destroy(_audioRoot.transform.Find(BgmNodeName)?.gameObject);
                GameObject.Destroy(_audioRoot.transform.Find(SfxNodeName)?.gameObject);
            }
        }

        // 仅创建 BGM 节点 (BGM 部分保持不变)
        private void SetupBgmNode(Transform parent)
        {
            GameObject bgmNode = new GameObject(BgmNodeName);
            bgmNode.transform.SetParent(parent);
            _bgmSource = bgmNode.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        // 初始化 SFX 池的根节点并创建第一个 AudioSource
        private void SetupSfxPoolRoot(Transform parent)
        {
            GameObject sfxNode = new GameObject(SfxNodeName);
            sfxNode.transform.SetParent(parent);
            _sfxRootTransform = sfxNode.transform;

            // 初始只创建一个 AudioSource
            CreateNewSfxSource();
        }

        /// <summary>
        /// 创建并初始化一个新的 AudioSource，并将其添加到池中。
        /// </summary>
        /// <returns>新创建的 AudioSource</returns>
        private AudioSource CreateNewSfxSource()
        {
            int index = _sfxSources.Count;
            GameObject sourceNode = new GameObject($"SFX_{index:1}");
            sourceNode.transform.SetParent(_sfxRootTransform);

            AudioSource source = sourceNode.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            _sfxSources.Add(source);
            Debug.Log($"[SFX Pool] 动态创建新的 AudioSource，当前池大小: {_sfxSources.Count}");
            return source;
        }

        // --- 播放音效 (SFX) ---

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource sourceToUse = null;

            // 1. 查找是否有空闲的 AudioSource (未在播放)
            // 这种查找方式比简单的循环覆盖更高效地利用了空闲资源
            sourceToUse = _sfxSources.FirstOrDefault(s => !s.isPlaying);

            if (sourceToUse == null)
            {
                // 2. 如果没有空闲的，判断是否可以扩展
                if (_sfxSources.Count < SfxSourceCount)
                {
                    // 未达到上限，则动态创建新的 AudioSource
                    sourceToUse = CreateNewSfxSource();
                }
                else
                {
                    // 3. 已达到上限（8个），则循环覆盖最老的那个音效
                    sourceToUse = _sfxSources[_nextSfxSourceIndex];

                    // 准备覆盖， advance index
                    _nextSfxSourceIndex = (_nextSfxSourceIndex + 1) % SfxSourceCount;
                    Debug.Log($"[SFX Pool] 达到上限({SfxSourceCount})，覆盖最老的音效索引: {_nextSfxSourceIndex}");
                }
            }

            // 4. 播放音效
            if (sourceToUse != null)
            {
                sourceToUse.clip = clip;
                sourceToUse.volume = volume;
                sourceToUse.Play();
            }
        }


        // --- 播放背景音乐 (BGM) ---

        public void PlayBgm(AudioClip clip, float volume = 1f)
        {
            if (_bgmSource == null || clip == null) return;

            if (_bgmSource.clip != clip)
            {
                _bgmSource.clip = clip;
                _bgmSource.volume = volume;
                _bgmSource.Play();
            }
        }
    }
}