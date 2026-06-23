using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHUDPresenter : NetworkBehaviour
{
    private Health health;
    [SerializeField]private WeaponBase weapon;

    // 我们持有的 View 实例
    private PlayerHUDView myView;

    private void Awake()
    {
        health = GetComponent<Health>();
        // 注意：如果你有切枪逻辑，这里以后可能需要动态监听当前激活的武器
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 1. 本地玩家认领主 UI
            myView = UIManager.Instance.mainPlayerView;

            // 2. 读取全局状态，控制队友 UI 面板的显隐
            bool isSoloMode = GameStateController.instance.isSolo.Value;
            // 单人模式隐藏，双人模式显示
            UIManager.Instance.teammateView.gameObject.SetActive(!isSoloMode);
        }
        else
        {
            // 1. 远端队友认领副 UI
            myView = UIManager.Instance.teammateView;
            myView.gameObject.SetActive(true);
        }

        // 1. 订阅 Model 层事件
        health.OnHealthChanged += OnHealthChanged;
        weapon.OnAmmoChanged += OnAmmoChanged;
        weapon.OnReloadStart += OnReloadStart;
        health.OnShieldChanged += OnShieldChanged;

        ForceRefreshUI();
    }

    public override void OnNetworkDespawn()
    {
        // 养成好习惯，玩家被销毁时务必取消订阅，防止内存泄漏！
        health.OnHealthChanged -= OnHealthChanged;
        weapon.OnAmmoChanged -= OnAmmoChanged;
        weapon.OnReloadStart -= OnReloadStart;
        health.OnShieldChanged -= OnShieldChanged;
    }

    // ==========================================
    // 强制刷新当前所有 UI (切图、切房间、或者重新复活时调用)
    // ==========================================
    public void ForceRefreshUI()
    {
        // 强制刷新血条
        myView.UpdateHealth(health.currentHealth.Value, health.maxHealth.Value);

        // 强制刷新弹药
        int currentAmmo = weapon.currentAmmo;
        int maxAmmo = (int)weapon.stats.GetStatValue(StatType.MagSize);
        bool isWarning = (float)currentAmmo / maxAmmo <= 0.3f;
        myView.UpdateAmmo(currentAmmo, maxAmmo, isWarning);
        myView.UpdateShield(health.currentShield.Value, health.maxHealth.Value); // 护盾基于最大生命值比例
    }
    // ==========================================
    // Presenter 的业务逻辑处理 (翻译官)
    // ==========================================

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // 翻译给 View 听
        myView.UpdateHealth(currentHealth, maxHealth);
    }

    private void OnAmmoChanged(int currentAmmo, int maxAmmo)
    {
        bool isWarning = (float)currentAmmo / maxAmmo <= 0.3f;
        myView.UpdateAmmo(currentAmmo, maxAmmo, isWarning);
    }

    private void OnReloadStart(float duration)
    {
        myView.PlayReloadAnimation(duration);
    }
    private void OnShieldChanged(float currentShield, float maxHealth)
    {
        // 直接传给 View 绘制
        myView.UpdateShield(currentShield, maxHealth);
    }
}