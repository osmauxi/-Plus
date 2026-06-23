using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ShellParticleAudio : MonoBehaviour
{
    private ParticleSystem partSystem;
    private List<ParticleCollisionEvent> collisionEvents;

    [Header("弹壳音效配置")]
    [Tooltip("触发第一下落地声音的速度阈值。先设为 0.1 测试")]
    public float firstBounceVelocityThreshold = 0.90f; // 【修改点 1】：默认值降到极低

    [Tooltip("声音冷却，防止散弹枪一瞬间掉出 8 个弹壳导致严重的音效爆音")]
    public float soundCooldown = 0.15f;
    private float lastPlayTime = 0f;

    [Tooltip("弹壳落地的清脆音效")]
    public AudioClip[] shellDropClips;
    [Tooltip("弹壳音量 (0~1)")]
    [Range(0f, 1f)]
    public float shellVolume = 0.5f;


    private void Awake()
    {
        partSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = partSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            float currentVelocity = collisionEvents[i].velocity.magnitude;

            if (currentVelocity > firstBounceVelocityThreshold)
            {
                if (Time.time - lastPlayTime > soundCooldown)
                {
                    lastPlayTime = Time.time;

                    if (shellDropClips != null && shellDropClips.Length > 0)
                    {
                        // 【修改】：使用你可以在面板上拉动的 shellVolume
                        AudioManager.instance.PlaySFXAtPosition(
                            shellDropClips[Random.Range(0, shellDropClips.Length)],
                            collisionEvents[i].intersection,
                            shellVolume, // <--- 这里！
                            1f + Random.Range(-0.15f, 0.15f)
                        );
                    }
                }
            }
        }
    }
}