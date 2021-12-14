using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Tanks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    //���a �ѦҪ���
    public static GameObject localPlayer;
    string gameVersion = "1";

    //�ͦ��I �ѦҪ���
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
        //�۰ʦP�B��������
        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(gameObject);
        instance = this;

        //�]�w�����I�ƾ�
        defaultSpawnPoint = new GameObject("Default SpawnPoint");
        defaultSpawnPoint.transform.position = new Vector3(0, 0, 0);
        defaultSpawnPoint.transform.SetParent(transform, false);
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

        //�ˬd�O�_�w���J�C��
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

    //�[�J�C��
    public void JoinGameRoom()
    {       
        var options = new RoomOptions
        {
            MaxPlayers = 6
        };

        PhotonNetwork.JoinOrCreateRoom("Kingdom", options, null);
    }

    //���I�U�h���s��
    public override void OnJoinedRoom()
    {
        //�p�G�OMaster�ХD����
        if (PhotonNetwork.IsMasterClient)
        {
            //��ܳЫةж�
            Debug.Log("Created room!!");
            //���J����
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            //��ܥ[�J�ж�
            Debug.Log("Joined room!!");
        }
    }

    //��ܥ[�J����
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("Join Room Failed {0}: {1}", returnCode, message);
    }

    //���J����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var spawnPoint = GetRandomSpawnPoint();

        //�ͦ����a�Z�J
        //localPlayer = PhotonNetwork.Instantiate("TankPlayer", new Vector3(0, 0, 0), Quaternion.identity, 0);
        
        //�]�w�H���X�{��m
        localPlayer = PhotonNetwork.Instantiate("TankPlayer",spawnPoint.position,spawnPoint.rotation,0);

        Debug.Log("Player Instance ID: " + localPlayer.GetInstanceID());
    }

    //�]�w�����I
    private Transform GetRandomSpawnPoint()
    {
        var spawnPoints = GetAllObjectsOfTypeInScene<SpawnPoint>();
        return spawnPoints.Count == 0
        ? defaultSpawnPoint.transform
        : spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }

    //����Ҧ������I
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