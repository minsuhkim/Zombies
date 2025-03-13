using UnityEngine;

public enum GunType
{
    Rifle, Pistol, ShotGun, Sniper, SMG
}

[CreateAssetMenu(fileName = "Item", menuName = "ItemGun")]
public class ItemSO : ScriptableObject
{
    public GunType gunType;
    public int bulletTotalCount;
    public int bulletCurrentCount;
    public int damage;
    public int maxWeaponDistance;
    public float fireDelay;
}
