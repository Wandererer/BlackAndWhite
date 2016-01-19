using UnityEngine;
using System.Collections;
using System.Net;

public class BlackAndWhite : MonoBehaviour {

    public GameObject RPSBackGroundPrefab; //가위바위보 표시시 메인게임 흐릿하게 보이기 위해
    public GameObject RPSSelectorPrefab;  //가위바위보 표시하는거
    public GameObject OppSelectSingleRPS;
    public GameObject oppRPsSelectorPrefab;
    public GameObject[] rpsPanelPrefab;


    GameObject findObject; //찾아서 적용 시킬거

    RPSKind selectedRPS=RPSKind.None;
    RPSKind selectedOppRPS=RPSKind.None;

    ArrayList nameList;

    bool isBgCreated = false;  //가위바위보를 위한 게임화면 흐릿하게
    bool isRPSSelected = false; //내가 가위바위보 골랐나
    bool isOppSendRPS = false; //상대가 가위바위보를 골랐나
    bool isMyTurn = false;
    bool isCheckWinner = false;

    const int PLAYER_NUM = 2;
    const int PLAY_MAX = 3;

    ArrayList array;

    GUIStyle font;  //폰트 설정

    InputData[] m_inputData = new InputData[PLAYER_NUM];
    NetworkController networkController = null;  //네트워크 연결용

    string ipAddr = "";

    string nickName = "닉네임"; //내 닉네임

    string oppNickName = ""; //상대편 닉네임

    bool isGameOver = false;  //게임이 끝났는지 

    float width;
    float height;

    float waitTime = 5.0f; //기다리는 시간
    float waitThreeSec = 3.0f;
    float waitForPlay = 3.0f;
    float sendTime = -1.0f;  //보내는 시간



    Winner winner = Winner.None;  //누가 이겼나

   

    enum GameState
    {
        None = 0,
        Ready,      // 게임 상대의 로그인 대기.
        SelectRPS,  //가위바위보 선택.
        ChooseWinner,    //가위바위보 서로 확인.
        StartGame,   //게임 시작
        EndGame,    //끝.
        Disconnect,	//오류.
    };

    GameState gameState;

	// Use this for initialization
	void Start () {
        width = Screen.width;
        height = Screen.height;

        gameState = GameState.None;

        nameList = new ArrayList();

        font = new GUIStyle();

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
               // SpawnBgAndRPS();
              //  ProcessingRPSSelect();
                CheckOpponentNickName();
                break;
         case GameState.SelectRPS:
               SpawnBgAndRPS();
                ProcessingRPSSelect();
                Debug.Log(gameState + "sdfasdf");
                WaitOppRPS();
                CheckBothPlayersSelectRPS();
                IsConnectedFalse();
                break;

         case GameState.ChooseWinner:
                ShowOppRPS();
                IsConnectedFalse();
                CalculateWinner();
             CheckWinnerState();
                break;

         case GameState.StartGame:
                IsConnectedFalse();
                break;

         default:
                Debug.Log(gameState + "   sdfasdf");
                IsConnectedFalse();
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
                PrintOppRpsState();
                break;

            case GameState.ChooseWinner:
                PrintPlayersNickName();
                WaitFiveSecondsForShowRPSResult();
                WaitThreeSecondsForShowRPSResultState();
                break;

            case GameState.StartGame:
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

            oppNickName = networkController.ReceiveNickName();
            if (oppNickName.Equals("") && sendTime<0f)
            {
                networkController.SendNickName(nickName);
                sendTime = 2f;

            }

            else if(!oppNickName.Equals(""))
            {
               
                    gameState = GameState.SelectRPS;
            }
        }
    }



    void PrintPlayersNickName()
    {
        GUI.Label(new Rect(width - 100f, 30, 120, 20), "이름 : "+nickName);
       // Debug.Log(nickName);
        GUI.Label(new Rect(20f, 30, 120, 20), "상대방 : "+ oppNickName);
       // Debug.Log("printopp  :"+oppNickName);
    }

    void SpawnBgAndRPS()
    {
        if (!isBgCreated)
        {
            Instantiate(RPSBackGroundPrefab).GetComponent<Transform>().position=Vector3.one*3;
            nameList.Add(RPSBackGroundPrefab.name+"(Clone)");
           GameObject obj= Instantiate(RPSSelectorPrefab) as GameObject;
           obj.GetComponent<Transform>().position = new Vector3(0, -4, 0);
           obj.name = "RPSSelector";
           nameList.Add(obj.name);

           obj = Instantiate(oppRPsSelectorPrefab) as GameObject;
           obj.GetComponent<Transform>().position = new Vector3(0, 3, 0);
           obj.name = "oppSelector";
           nameList.Add(obj.name);
          
            isBgCreated = true;
        }
        
    }

    void ProcessingRPSSelect()
    {
       // 

        try
        {
            //RPSSelector selector = GameObject.Find("RPSSelector").GetComponent<RPSSelector>();
            RPSPanel[] panels = GameObject.Find("RPSSelector").GetComponentsInChildren<RPSPanel>(); //주의 : null이 되버리면 프로그램 멈춰버리므로 try catch 문 이용할것

            foreach (RPSPanel p in panels)
            {
                if (p == null)
                { }

                if (p.IsSelected())
                {
                    selectedRPS = p.rpsKind;
                    isRPSSelected = true;
                    Debug.Log("isselctedchange");
                    Debug.Log(p.rpsKind + " is selected");
                    GameObject obj = GameObject.Find("RPSSelector");
                    Debug.Log("Destroy");
                    Destroy(obj);
                }
            }
        }
        catch
        {

        }
    

     
    
       
        GameObject rps = GameObject.Find("SelectRPS");

        if(rps==null && isRPSSelected)
        {
            int instNum = CastIntFromRPS(selectedRPS);
            rps = Instantiate(rpsPanelPrefab[instNum]) as GameObject;
            rps.name = "SelectRPS";
            nameList.Add(rps.name);
            Debug.Log("select and make one");
           // networkController.SendRPSData(selectedRPS);
            rps.GetComponent<Transform>().position = new Vector3(0, -2, 0);
        }
        sendTime -= Time.deltaTime;

        if (isRPSSelected && sendTime < 0f)
        {
            sendTime = 2f;
            Debug.Log("send rps data");
            networkController.SendRPSData(selectedRPS);

        }
      //  Debug.Log(selectedRps);

    }

    private int CastIntFromRPS(RPSKind select)
    {
        int result=-1;

        switch(select)
        {
            case RPSKind.Rock:
                result = 0;
                break;
                
            case RPSKind.Paper:
                result = 1;
                break;

            case RPSKind.Scissor:
                result = 2;
                break;
        }

        return result;


    }

    void CheckBothPlayersSelectRPS()
    {
        if(isOppSendRPS==true && isRPSSelected==true)
        {
            gameState = GameState.ChooseWinner;
        }
    }

    void WaitOppRPS()
    {
       if(selectedOppRPS==RPSKind.None)
        selectedOppRPS = networkController.ReceiveRPSData();


        Debug.Log("waitRPS");
        GameObject obj = GameObject.Find("oppSelector");
        GameObject obj2 = GameObject.Find("OppSelectRPS");
        if (selectedOppRPS == RPSKind.None)
        {
            Debug.Log("Receive none");
        }

        else if (selectedOppRPS != RPSKind.None && obj != null && obj2 == null) 
        {
            
            Destroy(obj);
            isOppSendRPS = true;
            Debug.Log(selectedOppRPS + " wtf");
             obj2 = Instantiate(OppSelectSingleRPS) as GameObject;
            obj2.name="OppSelectRPS";
            nameList.Add(obj2.name);
            obj2.GetComponent<Transform>().position=  new Vector3(0, 4, 0);

        }

    }

    void CalculateWinner()
    {
           if(selectedRPS!=RPSKind.None && selectedOppRPS!=RPSKind.None)
        {
            isCheckWinner = true;
            winner = ResultChecker.GetRPSWinner(selectedRPS, selectedOppRPS);
            Debug.Log("계산완료  " + winner);
        }
    }


           
    void CheckWinnerState()
    {
        if(winner!=Winner.None)
        {
            switch(winner)
            {
                case Winner.Win:

                    isMyTurn = true;
                    if (waitForPlay < 0.5f)
                    {
                        gameState = GameState.StartGame;
                        ClearPrefab();
                    }
                    break;
                case Winner.Draw:

                    if (waitForPlay < 0.5f)
                    {
                        foreach (string name in nameList)
                        {
                            try
                            {
                                GameObject obj = GameObject.Find(name);
                                Destroy(obj);
                                Debug.Log(name);
                            }

                            catch
                            {

                            }


                        }

                        nameList.Clear();
                        ClearForDraw();
                        gameState = GameState.SelectRPS;

                    }


                    break;

                case Winner.Loss:

                    if (waitForPlay < 0.5f)
                    {
                        gameState = GameState.StartGame;
                        ClearPrefab();
                    }
                    break;
            }
        }
    }

    void WaitFiveSecondsForShowRPSResult()
    {
        if (waitTime > 0.5f)
        {
            font.fontSize = 100;
            waitTime -= Time.deltaTime;
            if(waitTime>=1.3f)
            GUI.Label(new Rect(Screen.width / 2-20, Screen.height / 2-70, 200, 50),( (int)waitTime).ToString(), font);
        }
    }

    void WaitThreeSecondsForShowRPSResultState()
    {
        if(waitThreeSec>0.5f && waitTime<0.6f)
        {
            waitTime -= Time.deltaTime;
            WinnerCheckAndPrint();

        }
    }

    void WinnerCheckAndPrint()
    {
        switch(winner)
        {
            case Winner.None:

                break;

            case Winner.Draw:
                       font.fontSize = 100;
                GUI.Label(new Rect(Screen.width / 2 - 176, Screen.height / 2 - 70, 200, 50), "무 승 부", font);
                Debug.Log("결과 출력");
                waitForPlay -= Time.deltaTime;

                break;

            case Winner.Win:
                font.fontSize = 100;
                GUI.Label(new Rect(Screen.width / 2 - 120, Screen.height / 2 - 70, 200, 50), "승  리", font);
                Debug.Log("결과 출력");

                waitForPlay -= Time.deltaTime;
                break;

            case Winner.Loss:
                       font.fontSize = 100;
                GUI.Label(new Rect(Screen.width / 2 - 120, Screen.height / 2 - 70, 200, 50), "패  배", font);
                Debug.Log("결과 출력");
  
                waitForPlay -= Time.deltaTime;
                break;
        }
    }

    void PrintOppRpsState()
    {
        if (networkController.IsConnected())
        {
            if (isOppSendRPS &&  !isCheckWinner)
            {
                font.fontSize = 32;
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 50), "상대방 선택 완료", font);

            }

            else if (!isOppSendRPS && !isCheckWinner)
            {
                font.fontSize = 32;
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 50), "상대방 선택중", font);


            }
        }
    }

    void ShowOppRPS()
    {
        GameObject obj = GameObject.Find("oppRPS");
        if (waitTime < 0.5f && obj==null)
        {
            obj = GameObject.Find("OppSelectRPS");
            Destroy(obj);
            Debug.Log(selectedOppRPS+" opp select");
            int selected = CastIntFromRPS(selectedOppRPS);
            obj = Instantiate(rpsPanelPrefab[selected]) as GameObject;
            obj.GetComponent<Transform>().rotation = new Quaternion(0, 0, -180, 0);
            obj.GetComponent<Transform>().position = new Vector3(0, 4, 0);
            obj.name="oppRPS";
            nameList.Add(obj.name);
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

    void IsConnectedFalse()
    {

       
        if(networkController.IsConnected()==false)
        {
            Debug.Log("GameState Connedted False");

            ClearPrefab();
            gameState = GameState.None;
            networkController.StopServer();
            networkController = null;
            ClearAll();
        

        }
    }

    void ClearPrefab()
    {

        foreach (string name in nameList)
        {
            try
            {
                GameObject obj = GameObject.Find(name);
                Destroy(obj);
                Debug.Log(name);
            }

            catch
            {

            }


        }

        nameList.Clear();
    }


    void ClearAll()
    {
        isBgCreated = false;
        isRPSSelected = false;
        isOppSendRPS = false;
        isCheckWinner = false;
        nickName = "닉네임"; //내 닉네임


        winner = Winner.None;
        selectedRPS = RPSKind.None;
        selectedOppRPS = RPSKind.None;
        oppNickName = ""; //상대편 닉네임
        waitTime = 5.0f; //기다리는 시간

        sendTime = -1.0f;  //보내는 시간
        isGameOver = false;  //게임이 끝났는지 

    }

    void ClearForDraw()
    {
        isRPSSelected = false;
        isOppSendRPS = false;
        isBgCreated = false;
        isCheckWinner = false;
        selectedRPS = RPSKind.None;
        selectedOppRPS = RPSKind.None;

        winner = Winner.None;
    waitTime = 5.0f; //기다리는 시간
         waitThreeSec = 3.0f;
         waitForPlay = 3.0f;
         sendTime = -1.0f;  //보내는 시간


    }
 
}
