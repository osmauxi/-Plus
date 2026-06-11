using UnityEngine;

/// <summary>
/// 音频配置 ScriptableObject
/// 用于集中管理所有音频资源，方便策划和音频师调整
/// 创建方式：Project 窗口右键 → Create → Config → Audio
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Config/Audio")]
public class AudioConfigSO : ScriptableObject
{
    [Header("BGM")]
    [Tooltip("标题界面/主菜单 BGM")]
    public AudioClip titleBGM;
    [Tooltip("局内战斗 BGM")]
    public AudioClip gameplayBGM;
    [Tooltip("Boss 战 BGM")]
    public AudioClip bossBGM;
    [Tooltip("胜利结算 BGM")]
    public AudioClip victoryBGM;

    [Header("子弹音效")]
    [Tooltip("普通子弹射击音效")]
    public AudioClip normalBullet;
    [Tooltip("闪电子弹射击音效")]
    public AudioClip lightningBullet;
    [Tooltip("爆炸子弹射击音效")]
    public AudioClip explosionBullet;
    [Tooltip("激光子弹射击音效")]
    public AudioClip laserBullet;
    [Tooltip("子弹命中音效")]
    public AudioClip bulletHit;
    [Tooltip("子弹命中墙壁音效")]
    public AudioClip bulletHitWall;

    [Header("怪物音效")]
    [Tooltip("怪物行走脚步声数组（随机播放）")]
    public AudioClip[] monsterWalk;
    [Tooltip("怪物发现玩家/攻击前叫声数组（随机播放）")]
    public AudioClip[] monsterRoar;
    [Tooltip("怪物前扑攻击音效数组（随机播放）")]
    public AudioClip[] monsterLunge;
    [Tooltip("怪物受击音效")]
    public AudioClip[] monsterHurt;
    [Tooltip("怪物死亡音效")]
    public AudioClip[] monsterDeath;

    [Header("玩家音效")]
    [Tooltip("玩家行走脚步声数组（随机播放）")]
    public AudioClip[] playerWalk;
    [Tooltip("玩家奔跑脚步声数组（随机播放）")]
    public AudioClip[] playerRun;
    [Tooltip("玩家受击音效")]
    public AudioClip[] playerHurt;
    [Tooltip("玩家死亡音效")]
    public AudioClip playerDeath;
    [Tooltip("换弹音效")]
    public AudioClip reload;
    [Tooltip("拾取道具音效")]
    public AudioClip itemPickup;
    [Tooltip("升级音效")]
    public AudioClip levelUp;

    [Header("地形脚步声")]
    [Tooltip("毒地形行走脚步声数组（随机播放）")]
    public AudioClip[] footstepPoison;
    [Tooltip("冰地形行走脚步声数组（随机播放）")]
    public AudioClip[] footstepIce;
    [Tooltip("岩浆地形行走脚步声数组（随机播放）")]
    public AudioClip[] footstepLava;
    [Tooltip("普通地面行走脚步声数组（随机播放）")]
    public AudioClip[] footstepNormal;

    [Header("UI 音效")]
    [Tooltip("按钮点击音效")]
    public AudioClip uiClick;
    [Tooltip("按钮悬停音效")]
    public AudioClip uiHover;
    [Tooltip("确认/成功音效")]
    public AudioClip uiConfirm;
    [Tooltip("取消/失败音效")]
    public AudioClip uiCancel;
    [Tooltip("警告音效")]
    public AudioClip uiWarning;

    [Header("环境音效")]
    [Tooltip("环境背景音（如风声、雨声）")]
    public AudioClip ambientBackground;
    [Tooltip("开门音效")]
    public AudioClip doorOpen;
    [Tooltip("宝箱开启音效")]
    public AudioClip chestOpen;
    [Tooltip("传送门激活音效")]
    public AudioClip portalActivate;

    [Header("技能与机制音效")]
    [Tooltip("获得雷云时的天雷音效")]
    public AudioClip skillThunderCloud;
    [Tooltip("闪电链/落雷命中音效")]
    public AudioClip skillLightningHit;
    [Tooltip("爆炸词条触发的爆炸音效")]
    public AudioClip skillExplosion;
}
