using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    [SerializeField] private PlayerUIContainer[] playerContainers;
    [SerializeField] TextMeshProUGUI winText;

    [SerializeField] private float updateTimer;

    [SerializeField] public static GameUI instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializePlayerUI(); 
    }

    void InitializePlayerUI()
    {
        for (int x = 0; x < playerContainers.Length; ++x)
        {
            PlayerUIContainer container = playerContainers[x];

            if (x < PhotonNetwork.PlayerList.Length)
            {
                container.obj.SetActive(true);
                container.nameText.text = PhotonNetwork.PlayerList[x].NickName;
                container.hatTimeSlider.maxValue = Gamemanager.instance.timeToWin;
            }
            else
            {
                container.obj.SetActive(false);
            }
        }
    }

    private void Update()
    {
        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        for (int x = 0; x < Gamemanager.instance.players.Length; ++x)
        {
            if(Gamemanager.instance.players[x] != null)
            {
                playerContainers[x].hatTimeSlider.value = Gamemanager.instance.players[x].curHatTime;
            }
        }

    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + " wins!";
    }

}

[System.Serializable]
public class PlayerUIContainer
{
    [SerializeField] public GameObject obj;
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public Slider hatTimeSlider;
}