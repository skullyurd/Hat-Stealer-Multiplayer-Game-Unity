using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class Gamemanager : MonoBehaviourPunCallbacks
{
    [Header("stats")]
    [SerializeField] public bool gameEnded;
    [SerializeField] public float timeToWin;
    [SerializeField] private float invincibleDuration;
    [SerializeField] private float hatPickUpTime;

    [Header("Player")]
    [SerializeField] private string playerPrefabLocation;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] public PlayerControllerScript[] players;
    [SerializeField] public int playerWithHat;
    [SerializeField] private int playersInGame;

    public static Gamemanager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerControllerScript[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity, 0);
        PlayerControllerScript playerScript = playerObj.GetComponent<PlayerControllerScript>();

        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerControllerScript GetPlayer(int playerId)
    {
        return players.First(x => x.Id == playerId);
    }

    public PlayerControllerScript GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }

    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {
        if (!initialGive)
        {
            GetPlayer(playerWithHat).SetHat(false);
        }

        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickUpTime = Time.time;
    }

    public bool canGetHat()
    {
        if (Time.time > hatPickUpTime + invincibleDuration)
        {
            return true;
        }
        else
        {

            return false;
        }
    }

    [PunRPC]
    public void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerControllerScript player = GetPlayer(playerId);
        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 3.0f);
    }

    public void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
