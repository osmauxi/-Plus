using UnityEngine;

public class DummyMenuBullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 2f;


    private void Start()
    {
        // 2秒后自动销毁，绝不占用内存
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 傻瓜式往前飞
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果打到了主菜单用来做背景的墙
        if (other.CompareTag("Wall"))
        {
            // 播放打墙音效 (AudioManager 在 Loading 场景就已经 DontDestroyOnLoad 存活了，直接白嫖！)
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFXByCategory(AudioCategory.SFX_Bullet_Hit_Wall, 0.5f);

            Destroy(gameObject);
        }
    }
}