using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/Monster Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("核心标识")]
    public string monsterName; // 策划备注，如"变异丧尸"
    public string poolId;      // 对象池唯一ID，如"Zombie_Elite"
    public GameObject prefab; // 直接把带 NetworkObject 的预制件拖到这里！
    public int initialPoolSize = 10; // 默认池子大小

    [Header("经济与导演系统")]
    public int cost;           // 购买价格
    public int minLayerToSpawn;// 解锁层数

    [Header("基础战斗属性 (ROM)")]
    public float baseMaxHealth;
    public float baseSpeed;
    public float baseDamage;
    public float baseDefense;
}
