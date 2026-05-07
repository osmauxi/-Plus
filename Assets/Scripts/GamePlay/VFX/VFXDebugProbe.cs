using UnityEngine;

public class VFXDebugProbe : MonoBehaviour
{
    //// 这个方法只要物体被 SetActive(true) 就会触发
    //private void OnEnable()
    //{
    //    Debug.Log($"[特效探针] {gameObject.name} 被激活了！当前世界坐标是: {transform.position}，当前帧: {Time.frameCount}");
    //}

    //// 追踪每一帧的位置，看看它是不是在播放途中被瞬移了
    //private void Update()
    //{
    //    // 假设它激活后活不到 10 帧，我们打印出它前几帧的坐标轨迹
    //    if (Time.frameCount % 10 == 0)
    //    {
    //        Debug.Log($"[特效轨迹] {gameObject.name} 播放中，坐标: {transform.position}");
    //    }
    //}
}