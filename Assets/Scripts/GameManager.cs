using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    //玩家參考物件
    public static GameObject localPlayer;
    string gameVersion = "1";
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogErrorFormat(gameObject,
            "Multiple instances of {0} is not allow", GetType().Name);
            DestroyImmediate(gameObject);
            return;
        }
        //自動同步場景機制
        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

        //檢查是否已載入遊戲
        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    public override void OnConnected()
    {
        Debug.Log("PUN Connected");
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Connected to Master");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Disconnected was called by PUN with reason {0}", cause);
    }

    //加入遊戲
    public void JoinGameRoom()
    {       
        var options = new RoomOptions
        {
            MaxPlayers = 6
        };

        PhotonNetwork.JoinOrCreateRoom("Kingdom", options, null);
    }

    //當點下去按鈕後
    public override void OnJoinedRoom()
    {
        //如果是Master房主的話
        if (PhotonNetwork.IsMasterClient)
        {
            //顯示創建房間
            Debug.Log("Created room!!");
            //載入場景
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            //顯示加入房間
            Debug.Log("Joined room!!");
        }
    }

    //顯示加入失敗
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("Join Room Failed {0}: {1}", returnCode, message);
    }

    //載入場景
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }
        //生成玩家坦克
        localPlayer = PhotonNetwork.Instantiate("TankPlayer", new Vector3(0, 0, 0), Quaternion.identity, 0);
        Debug.Log("Player Instance ID: " + localPlayer.GetInstanceID());
    }
}