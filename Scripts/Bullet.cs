using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public float bulletLife = 5f;

    public Rigidbody rig;

    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        Destroy(gameObject, bulletLife);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Bullet"))
        {

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
