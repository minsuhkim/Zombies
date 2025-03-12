using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    private int hp = 100;

    public int HP
    {
        get
        {
            return hp;
        }
        set
        {
            if (hp < 0)
            {
                hp = 0;
            }
            else
            {
                hp = value;
            }
        }
    }
}
