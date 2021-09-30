using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    public int ADS;

    private bool flashingDamage;
    private bool isSprint = false;
    private NetworkManager nM;
    private Camera miniMap;
    private Camera[] maps;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public PlayerWeapon weapon;
    public MeshRenderer mr;
    public GameObject gun;

    void Awake()
    {
        nM = FindObjectOfType<NetworkManager>();
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        // is this not our local player?
        if (!photonView.IsMine)
        {
            maps = GetComponentsInChildren<Camera>();

            foreach (Camera m in maps)
            {
                m.enabled = false;
            }
            rig.isKinematic = true;
            
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || dead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprint = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprint = false;
        }

        Move(isSprint);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Camera.main.fieldOfView = ADS;
        }

        if (Input.GetMouseButtonUp(1))
        {
            Camera.main.fieldOfView = 60;
        }

        if (nM.currSel == "Shotgun")
        {
            if (Input.GetMouseButtonDown(0))
            {
                weapon.TryShoot();
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                weapon.TryShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weapon.Reload2();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
        }
    }

    void Move(bool isSprint)
    {
        // get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        if (isSprint)
        {
            Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed * 2f;
            // calculate a direction relative to where we're facing
            dir.y = rig.velocity.y;

            // set that as our velocity
            rig.velocity = dir;
        }
        else
        {
            Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
            // calculate a direction relative to where we're facing
            dir.y = rig.velocity.y;

            // set that as our velocity
            rig.velocity = dir;
        }
        
    }

    void TryJump()
    {
        // create a ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot the raycast
        if (Physics.Raycast(ray, 1.5f))
        {
            if (photonPlayer.NickName == "NinnyGarrett")
            {
                rig.AddForce(Vector3.up * (jumpForce * 2), ForceMode.Impulse);
            }
            else
            {
                rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
        {
            return;
        }

        curHp -= damage;
        curAttackerId = attackerId;

        // flash player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        // update the health bar UI
        GameUI.instance.UpdateHealthBar();

        // die if no health left
        if (curHp <= 0)
        {
            photonView.RPC("Die", RpcTarget.All);
        }
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
        {
            return;
        }

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();
        
        // host check win condition
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }

        // is local player
        if (photonView.IsMine)
        {
            if (curAttackerId != 0)
            {
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);
            }

            // set camera to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            // disable physics and hide player
            rig.isKinematic = true;
            //transform.position = new Vector3(0, -50, 0);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            transform.position = new Vector3(0, 0, 0);

            gun.SetActive(false);
            Camera.main.fieldOfView = 60;
        }

        Camera.main.fieldOfView = 60;
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;

        // update UI
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
    }
}
