using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControllerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector] public int Id;

    [Header("Info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private GameObject hatObject;

    [HideInInspector] private float curHatTime;

    [Header("Components")]
    [SerializeField] private Rigidbody rig;
    [SerializeField] private Player photonPlayer;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        Id = player.ActorNumber;

        Gamemanager.instance.players[Id - 1] = this;

        if (!photonView.IsMine)
        {
            rig.isKinematic = true;
        }

        if (Id == 1)
        {
            Gamemanager.instance.GiveHat(Id, true);
        }

    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= Gamemanager.instance.timeToWin && !Gamemanager.instance.gameEnded)
            {
                Gamemanager.instance.gameEnded = true;
                Gamemanager.instance.photonView.RPC("WinGame", RpcTarget.All, Id);
            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryJump();

                if (hatObject.activeInHierarchy)
                {
                    curHatTime += Time.deltaTime;
                }
            }
        }
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    private void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Gamemanager.instance.GetPlayer(collision.gameObject).Id == Gamemanager.instance.playerWithHat)
            {
                if (Gamemanager.instance.canGetHat())
                {
                    Gamemanager.instance.photonView.RPC("GiveHat", RpcTarget.All, Id, false);
                }
            }
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
