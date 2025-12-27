using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] List<Weapon> prefabs;
    [SerializeField] List<Weapon> weapons;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        foreach(var w in prefabs)
        {
            Weapon instance = Instantiate(w, transform);
            weapons.Add(instance);
        }
    }

    private void Start()
    {
        ChangeWeapon(0);
    }

    public void ChangeWeapon(int index)
    {
        if(index > weapons.Count -1)
        {
            return;
        }
        player.Arm?.EnableSprite(false);
        player.Arm?.transform.SetParent(transform);
        player.Arm = weapons[index];
        player.Arm.transform.SetParent(player.transform);
        player.Arm.transform.localPosition = player.ArmPositon.localPosition;
        player.ChangeState(player.States["Idle"]);
    }
}
