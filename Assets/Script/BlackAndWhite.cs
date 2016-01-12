using UnityEngine;
using System.Collections;
using System.Net;

public class BlackAndWhite : MonoBehaviour {

    public GameObject RPSBackGroundPrefab;
    bool isBgCreated = false;

    public GameObject RPSSlectorPrefab;

    const int PLAYER_NUM = 2;
    const int PLAY_MAX = 3;

    InputData[] m_inputData = new InputData[PLAYER_NUM];
    NetworkController networkController = null;

    string ipAddr = "";

    string nickName = "닉네임";

    string oppNickName = "";

    bool isGameOver = false;

    float width;
    float height;

    float waitTime = 3.0f;

    float sendTime = -1.0f;

    enum GameState
    {
        None = 0,
        Ready,      // 게임 상대의 로그인 대기.
        SelectRPS,  //가위바위보 선택.
        WaitRPS,    //수신대기.
        EndGame,    //끝.
        Disconnect,	//오류.
    };

    GameState gameState;

	// Use this for initialization
	void Start () {
        width = Screen.width;
        height = Screen.height;

        gameState = GameState.None;

        GameObject go = new GameObject("Network");
        if(go!=null)
        {
            TCP transport = go.AddComponent<TCP>();
            if (transport != null)
                transport.RegisterEventHandler(EventCallback);

        }

        DontDestroyOnLoad(go);
        string hostname = Dns.GetHostName();
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);
        ipAddr = adrList[0].ToString();
	}
	
	// Update is called once per frame
	void Update () {
	 switch(gameState)
        {
         case GameState.None:
               
                break;

         case GameState.Ready:
                SpawnBgAndRPS();
                CheckOpponentNickName();
                break;
         case GameState.SelectRPS:
               // SpawnBgAndRPS();
                break;
        }
	}

    void OnGUI()
    {
        switch(gameState)
        {
            case GameState.None:
                StartScreen();
                break;

            case GameState.Ready:
                PrintPlayersNickName();
                break;
            case GameState.SelectRPS:
                PrintPlayersNickName();
                break;
        }
    }


    void StartScreen()
    {
        float px = Screen.width*0.5f;
        float py = Screen.height*0.5f;

        if(networkController==null)
        {
            nickName = GUI.TextField(new Rect(px-40f, py-50, 100, 20), nickName);

            if(GUI.Button(new Rect(px-40f,py+10f,100,50),"서버 시작"))
            {
                networkController = new NetworkController();
                gameState = GameState.Ready;
                isGameOver = false;

            }

            if(GUI.Button(new Rect(px-40f,py+100f,100,50),"서버 접속"))
            {
                networkController = new NetworkController(ipAddr);
                gameState = GameState.Ready;
                isGameOver = false;
            }

            GUI.Label(new Rect(px - 20f, py + 170f, 100, 50), "상대방 ip");

            ipAddr = GUI.TextField(new Rect(px - 65f, py + 200f, 150, 20), ipAddr);
        }
    }

    
    void CheckOpponentNickName()
    {
        if(networkController.IsConnected()  )
        {
            sendTime -= Time.deltaTime;

           
            if (oppNickName.Equals(""))
            {
                networkController.SendNickName(nickName);
                oppNickName = networkController.ReceiveNickName();
            }

            else
            {
                waitTime -= Time.deltaTime;
                if (waitTime < 0)
                {
                    gameState = GameState.SelectRPS;
                    Debug.Log("gamestate change ready to selectRPS");
                }
            }
        }
    }



    void PrintPlayersNickName()
    {
        GUI.Label(new Rect(width - 100f, 30, 120, 20), "이름 : "+nickName);
       // Debug.Log(nickName);
        GUI.Label(new Rect(20f, 30, 120, 20), "상대방 : "+ oppNickName);
        Debug.Log("printopp  :"+oppNickName);
    }

    void SpawnBgAndRPS()
    {
        if (!isBgCreated)
        {
            Instantiate(RPSBackGroundPrefab).GetComponent<Transform>().position=Vector3.one*3;
            Instantiate(RPSSlectorPrefab).GetComponent<Transform>().position=new Vector3(0, -3,0);
            isBgCreated = true;
        }
    }


    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void EventCallback(NetEventState state)
    {
        switch (state.type)
        {
            case NetEventType.Disconnect:
                if (gameState < GameState.EndGame && isGameOver == false)
                {
                    gameState = GameState.Disconnect;
                }
                break;
        }
    }

}
