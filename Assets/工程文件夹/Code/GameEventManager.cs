using System;
using UnityEngine;

namespace 工程文件夹.Code
{
    /// <summary>
    /// 在这个脚本里的Triggered方法决定传什么
    /// </summary>
    [AddComponentMenu("GameInputManage")]
    public class GameEventManager : MonoBehaviour
    {
        private static readonly Lazy<GameEventManager> Lazy = new Lazy<GameEventManager>(() => new GameEventManager());

        private GameEventManager()
        {
        }

        public static GameEventManager Instance => Lazy.Value;

        public delegate void TriggerEventHandler(string message,Transform _transform);
        public static event TriggerEventHandler OnTrigger;
    

    
        public void Triggered(string message,Transform _transform) {
            Debug.Log("Triggered: " + message);
            if (OnTrigger != null)
                OnTrigger(message,_transform);
        }

        public void AddListener(TriggerEventHandler listener) {
            OnTrigger += listener;
        }

        public void RemoveListener(TriggerEventHandler listener) {
            OnTrigger -= listener;
        }
    
    }
}

#region 监听交互的方法
// 以下是监听交互的方法
// void OnEnable()
// {
//     PlayerManager.OnTrigger += HandleTrigger;
// }
//
// void OnDisable()
// {
//     PlayerManager.OnTrigger -= HandleTrigger;
// }
//
// void HandleTrigger(string message,EGameObj gameObj)
// {
//     Debug.Log("Trigger event received:222222222222 " + message,StuffEnum);
// }
#endregion

#region 以下是使用方法，发送事件

// GameEventManager.Instance.Triggered("to touch",transform);

#endregion