using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxClip;
    public int totalAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;
    public float reloadTime;

    private float lastShootTime;
    private float maxSpread = .1f;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;
    public bool isShotgun;

    public AudioSource audioSource;
    public AudioClip shotSound;
    public AudioClip reloadSound;
    public AudioClip reloadSoundShotgun;

    private PlayerController player;


    void Awake()
    {
        // get required components
        player = GetComponent<PlayerController>();

    }

    public void TryShoot()
    {
        // can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
        {
            return;
        }

        curAmmo--;
        lastShootTime = Time.time;

        // update the ammo UI
        GameUI.instance.UpdateAmmoText();

        // spawn the bullet
        if (isShotgun)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 dir = Camera.main.transform.forward + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread));
                player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, dir);
                audioSource.clip = shotSound;
                audioSource.Play();
            }
            //audioSource.clip = reloadSoundShotgun;
            //audioSource.Play();
        }
        else
        {
            player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, Camera.main.transform.forward);
            audioSource.clip = shotSound;
            audioSource.Play();
        }
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        if (isShotgun)
        {
            bulletObj.GetComponent<Bullet>().bulletLife = 1f;
        }

        // get the bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // intialize it and set the vel
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rig.velocity = dir * bulletSpeed;
    }

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        totalAmmo = Mathf.Clamp(totalAmmo + ammoToGive, 0, maxAmmo);

        // update ammo text
        GameUI.instance.UpdateAmmoText();
    }

    public void Reload()
    {

        if (isShotgun)
        {
            for (int i = curAmmo; i < maxClip; i++)
            {
                curAmmo++;
                totalAmmo--;
            }
        }
        else
        {
            for (int i = curAmmo; i < maxClip; i++)
            {
                curAmmo++;
                totalAmmo--;
            }
        }

        GameUI.instance.UpdateAmmoText();
    }

    public void Reload2()
    {
        audioSource.clip = reloadSound;
        audioSource.Play();
        Invoke("Reload", reloadTime);
    }
}
