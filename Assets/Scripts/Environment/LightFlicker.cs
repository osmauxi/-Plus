using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light torchLight;
    public float baseIntensity = 2f;    // 基础亮度
    public float flickerSpeed = 10f;    // 闪烁速度
    public float flickerAmount = 0.5f;  // 闪烁幅度

    private void Awake()
    {
        torchLight = GetComponent<Light>();
    }

    private void Update()
    {
        // 用柏林噪声生成平滑且随机的闪烁感
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        torchLight.intensity = baseIntensity + (noise * flickerAmount);
    }
}