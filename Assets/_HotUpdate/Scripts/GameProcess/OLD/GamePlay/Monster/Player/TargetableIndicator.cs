using System.Collections;
using UnityEngine;

public enum IndicatorType { Monster, Chest }

public class TargetableIndicator : MonoBehaviour
{
    public IndicatorType type;
    private bool isRegistered = false;

    private void OnEnable()
    {
        // 【核心修复 1】：每次从对象池被拿出来，必须强行重置大脑状态！
        isRegistered = false;

        // 【核心修复 2】：改用协程！对象池预热时的瞬间 Disable 会直接掐死这个协程，防止幽灵注册 
        StartCoroutine(RegisterRoutine());
    }

    private IEnumerator RegisterRoutine()
    {
        // 【核心修复 3】：无限期等待！
        // 如果怪物比玩家先生成（比如刚进房间），它会在这里安静地等，直到本地玩家单例赋值完毕 
        while (PlayerIndicatorController.LocalInstance == null)
        {
            yield return null;
        }

        // 等到玩家后，确保自己还没有被注册
        if (!isRegistered)
        {
            PlayerIndicatorController.LocalInstance.RegisterTarget(this.transform, type);
            isRegistered = true;
        }
    }

    private void OnDisable()
    {
        Unregister();
    }

    public void Unregister()
    {
        // 【核心修复 4】：就算玩家突然掉线/销毁导致 LocalInstance 为空，也要把自己的状态重置！
        if (isRegistered && PlayerIndicatorController.LocalInstance != null)
        {
            PlayerIndicatorController.LocalInstance.UnregisterTarget(this.transform);
        }

        // 必须放在外面无条件执行，保证下一次出池子时是个干净的模块
        isRegistered = false;
    }
}