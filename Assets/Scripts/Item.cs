using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO itemData;

    public GunType gunType;
    public int bulletTotalCount;
    public int bulletCurrentCount;
    public int damage;
    public int maxDistance;
    public float fireDelay;
    //public ParticleSystem[] shotEffect;

    void Start()
    {
        gunType = itemData.gunType;
        bulletCurrentCount = itemData.bulletCurrentCount;
        bulletTotalCount = itemData.bulletTotalCount;
        damage = itemData.damage;
        maxDistance = itemData.maxWeaponDistance;
        fireDelay = itemData.fireDelay;
        //shotEffect = GetComponentsInChildren<ParticleSystem>();
    }
}
