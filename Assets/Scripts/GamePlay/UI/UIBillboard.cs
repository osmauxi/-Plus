using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        // 如果你有自己统一管理的摄像机引用（比如 CameraViewManager），可以用那个替代
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam == null) return;

        // 让 UI 的正前方与摄像机的正前方完全平行（而不是 LookAt，LookAt 会导致边缘的 UI 发生透视变形）
        transform.forward = mainCam.transform.forward;
    }
}