using UnityEngine;

namespace Aquaivy.Unity
{
    /// <summary>
    /// Unity模块的基础管理器，
    /// 使用其他模块前先调用GameManager.Init()
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameObject gameManagerObject;

        /// <summary>
        /// 初始化游戏世界管理器
        /// </summary>
        public static void Init()
        {
            if (gameManagerObject != null)
                return;

            gameManagerObject = new GameObject("GameManager");
            gameManagerObject.AddComponent<GameManager>();
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            int elapseTime = (int)(Time.smoothDeltaTime * 1000);

            TaskLite.Update(elapseTime);
        }
    }
}
