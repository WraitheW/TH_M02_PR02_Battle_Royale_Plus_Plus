using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Health,
    Ammo
}

public class Pickup : MonoBehaviourPun
{
    public PickupType type;
    public int value;

    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * (15 * Time.deltaTime));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (type == PickupType.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
            }
            else if (type == PickupType.Ammo)
            {
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);
            }

            // destroy
            photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void DestroyPickup()
    {
        gameObject.SetActive(false);
        Invoke("RespawnPickup", 10);
    }

    public void RespawnPickup()
    {
        gameObject.SetActive(true);
    }
}
