using UnityEngine;

public enum GunType
{
    Rifle = 0,
    ShotGun = 1,
    Pistol = 2,
    Sniper = 3,
    SMG = 4
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
