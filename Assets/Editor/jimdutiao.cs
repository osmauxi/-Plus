using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    public VisualEffect targetVFX; // 拖入你的 Visual Effect 组件
    public Slider progressSlider; // 拖入一个 UI Slider 作为进度条

    private bool isPlaying = false; // 特效是否正在播放
    private float currentTime = 0.0f; // 当前特效的“逻辑时间”
    public float maxDuration = 5.0f; // 特效的总时长（可在Inspector中手动设置）

    void Start()
    {
        // 初始化 Slider，最大值设为特效的总时长
        if (progressSlider != null)
        {
            progressSlider.minValue = 0.0f;
            progressSlider.maxValue = maxDuration;
            progressSlider.value = 0.0f;
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        // 可选：让特效默认暂停
        targetVFX.pause = true;
    }

    void Update()
    {
        if (isPlaying)
        {
            // 正向播放：使用 Time.deltaTime 推进时间
            currentTime += Time.deltaTime;

            // 当时间超过总时长后，重置到开头（可根据需要修改为停止或循环）
            if (currentTime >= maxDuration)
            {
                currentTime = 0.0f;
                // targetVFX.Reinit(); // 可选：重置特效，使其重新开始
            }

            // 更新进度条的值
            if (progressSlider != null)
                progressSlider.SetValueWithoutNotify(currentTime);

            // 通过 Simulate 驱动特效前进一帧
            targetVFX.Simulate(Time.deltaTime, 1);
        }
    }

    // 当滑块值改变时调用
    public void OnSliderValueChanged(float newTime)
    {
        // 计算从 currentTime 到 newTime 的时间差
        float deltaTime = newTime - currentTime;
        currentTime = newTime;

        // 如果当前处于暂停或停止状态，通过 Simulate 立即更新画面
        if (!isPlaying)
        {
            targetVFX.Simulate(deltaTime, 1);
        }
    }

    // --- 以下方法可以绑定到 UI 按钮的 OnClick 事件 ---
    public void PlayVFX()
    {
        isPlaying = true;
        targetVFX.pause = false;
    }

    public void PauseVFX()
    {
        isPlaying = false;
        targetVFX.pause = true;
    }

    public void StopVFX()
    {
        isPlaying = false;
        targetVFX.Stop(); // 停止所有粒子生成
        // targetVFX.Reinit(); // 如果需要重置，可以使用 Reinit()
        currentTime = 0.0f;
        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0.0f);
    }
}
