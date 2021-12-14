using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Tanks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    //玩家 參考物件
    public static GameObject localPlayer;
    string gameVersion = "1";

    //生成點 參考物件
    private GameObject defaultSpawnPoint;

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

        //設定重生點數據
        defaultSpawnPoint = new GameObject("Default SpawnPoint");
        defaultSpawnPoint.transform.position = new Vector3(0, 0, 0);
        defaultSpawnPoint.transform.SetParent(transform, false);
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

        var spawnPoint = GetRandomSpawnPoint();

        //生成玩家坦克
        //localPlayer = PhotonNetwork.Instantiate("TankPlayer", new Vector3(0, 0, 0), Quaternion.identity, 0);
        
        //設定隨機出現位置
        localPlayer = PhotonNetwork.Instantiate("TankPlayer",spawnPoint.position,spawnPoint.rotation,0);

        Debug.Log("Player Instance ID: " + localPlayer.GetInstanceID());
    }

    //設定重生點
    private Transform GetRandomSpawnPoint()
    {
        var spawnPoints = GetAllObjectsOfTypeInScene<SpawnPoint>();
        return spawnPoints.Count == 0
        ? defaultSpawnPoint.transform
        : spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }

    //抓取所有重生點
    public static List<GameObject> GetAllObjectsOfTypeInScene<T>()
    {
        var objectsInScene = new List<GameObject>();
        foreach (var go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (go.hideFlags == HideFlags.NotEditable ||
            go.hideFlags == HideFlags.HideAndDontSave)
                continue;
            if (go.GetComponent<T>() != null)
                objectsInScene.Add(go);
        }
        return objectsInScene;
    }
}