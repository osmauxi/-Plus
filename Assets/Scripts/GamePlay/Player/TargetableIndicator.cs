using UnityEngine;

public enum IndicatorType { Monster, Chest }

public class TargetableIndicator : MonoBehaviour
{
    public IndicatorType type;
    private bool isRegistered = false;

    private void OnEnable()
    {
        // 延迟一帧注册，确保本地玩家已经生成完毕
        Invoke(nameof(TryRegister), 0.1f);
    }

    private void TryRegister()
    {
        if (PlayerIndicatorController.LocalInstance != null && !isRegistered)
        {
            PlayerIndicatorController.LocalInstance.RegisterTarget(this.transform, type);
            isRegistered = true;
        }
    }

    private void OnDisable()
    {
        Unregister();
    }

    // 开放给外部手动调用 (比如宝箱被打开时，不销毁物体但要取消箭头)
    public void Unregister()
    {
        if (isRegistered && PlayerIndicatorController.LocalInstance != null)
        {
            PlayerIndicatorController.LocalInstance.UnregisterTarget(this.transform);
            isRegistered = false;
        }
    }
}