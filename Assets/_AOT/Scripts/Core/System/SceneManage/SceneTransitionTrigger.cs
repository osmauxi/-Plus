using UnityEngine;

namespace MyScripts.MySystem
{
    // 场景跳转触发器：绑定在跳转点上（如道路、门），触发场景跳转
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("跳转配置")]
        [SerializeField] private string targetSceneName; // 目标场景名（如 "CastleScene"）
        //[SerializeField] private string targetSpawnPointTag = "Spawn_Default"; // 目标场景的出生点标签
        [SerializeField] private TriggerType triggerType = TriggerType.Collision; // 触发类型：碰撞/按钮
        [SerializeField] private string triggerTag = "Player"; // 触发对象标签（如主角）

        // 触发类型枚举
        public enum TriggerType
        {
            Collision, // 碰撞触发（走道路、进门）
            ButtonClick // 按钮触发（UI 按钮、场景内交互按钮）
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerType == TriggerType.Collision && other.CompareTag(triggerTag))
            {
                TriggerTransition();
            }
        }

        public void OnButtonClickTrigger()
        {
            if (triggerType == TriggerType.ButtonClick)
            {
                TriggerTransition();
            }
        }

        private void TriggerTransition()
        {
            // 调用全局管理器跳转场景
            //SceneManager.Instance.TransitionToGameScene(targetSceneName, targetSpawnPointTag);

            // （可选）禁用触发器，避免重复触发
            gameObject.SetActive(false);
            Invoke("EnableTrigger", 1f); // 1 秒后重新启用
        }

        private void EnableTrigger()
        {
            gameObject.SetActive(true);
        }
    }
}