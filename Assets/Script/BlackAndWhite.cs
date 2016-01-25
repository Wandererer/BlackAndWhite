using UnityEngine;
using System.Collections;
using System.Net;

public class BlackAndWhite : MonoBehaviour {

    public GameObject RPSBackGroundPrefab; //가위바위보 표시시 메인게임 흐릿하게 보이기 위해
    public GameObject RPSSelectorPrefab;  //가위바위보 표시하는거
    public GameObject OppSelectSingleRPS;
    public GameObject oppRPsSelectorPrefab;
    public GameObject[] rpsPanelPrefab;
    public GameObject oppPointsPrefab;
    public GameObject myPointsPrefab;
    public GameObject myTurnShowPrefab;
    public GameObject oppTurnShowPrefab;
    public GameObject editAgainPrefab;
    public GameObject blackForPointResultPrefab;
    public GameObject whiteForPointResultPrefab;
    public GameObject waitingSignPrefab;


    GameObject findObject; //찾아서 적용 시킬거

    RPSKind selectedRPS=RPSKind.None;
    RPSKind selectedOppRPS=RPSKind.None;

    ArrayList nameList;
    ArrayList gameInitList;

    bool isBgCreated = false;  //가위바위보를 위한 게임화면 흐릿하게
   bool isRPSSelected = false; //내가 가위바위보 골랐나
   bool isOppSendRPS = false; //상대가 가위바위보를 골랐나
     bool isMyTurn = false;
     bool isCheckWinner = false;
     bool isFirst = false;
     bool isReceivePoint = false;


     RPointsController oppPointController;
     RPointsController myPointController;

    const int PLAYER_NUM = 2;
    const int PLAY_MAX = 3;
 

    GUIStyle font;  //폰트 설정
    NetworkController networkController = null;  //네트워크 연결용

    string ipAddr = "";

    string nickName = "닉네임"; //내 닉네임

    string oppNickName = ""; //상대편 닉네임

    string pointString = "";

    int myPoint=-1;
    int oppPoint = -1;

    bool isGameOver = false;  //게임이 끝났는지 

    float width;
    float height;
    float width2;

    float waitTime = 5.0f; //기다리는 시간
     float waitThreeSec = 3.0f;
    float waitForPlay = 4.0f;
    float waitTwoSecond = -1.0f;
    float sendTime = -1.0f;  //보내는 시간

    Winner winner = Winner.None;  //누가 이겼나

    private RPSSelector rpsSelector;

   

    enum GameState
    {
        None = 0,
        Ready,      // 게임 상대의 로그인 대기.
        SelectRPS,  //가위바위보 선택.
        ChooseWinner,    //가위바위보 서로 확인.
        StartGame,   //게임 시작
        Proceed, //게임 진행
        ShowResult, //라운드 결과
        EndGame,    //끝.
        Disconnect,	//오류.
    };

    GameState gameState;

	// Use this for initialization
	void Start () {
        width = Screen.width;
        height = Screen.height;

        Debug.Log(width + "  " + height + "크기");

        gameState = GameState.None;
       

        

        nameList = new ArrayList();
        gameInitList = new ArrayList();
       
     
        //Debug.Log("테스트  " + );
        font = new GUIStyle();

        oppPointController = oppPointsPrefab.GetComponent<RPointsController>();
        myPointController = myPointsPrefab.GetComponent<RPointsController>();

        oppPointController.SetPoint(99);
        myPointController.SetPoint(99);

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
                SpawnPoints();
                break;

         case GameState.Proceed:
                IsConnectedFalse();
                showMyTurnOrOppTurn();
                DestroyAgainTextAfterTwoSeconds();
                CreatePointResult();
                ReceiveOppPointSecondAttack();
                WaitingSignOppPointAndCreateOppResultForSecondAttacker();
                ChangeGameStateForSecondAttacker();
               
                break;

         case GameState.ShowResult:
                IsConnectedFalse();
                ReceiveOppPointFirstAttack();
                WaitingSignOppPointAndCreateOppResultForFirstAttacker();
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
                WaitFiveSeconds();
              //  WaitFiveSecondsForShowRPSResult();
                WaitThreeSecondsForShowRPSResultState();
                break;

            case GameState.StartGame:
                PrintPlayersNickName();

                break;

            case GameState.Proceed:
                PrintPlayersNickName();
                MakeTextAreaAndButtonForSend();
                break;

            case GameState.ShowResult:
                PrintPlayersNickName();
                break;
                  
                
      
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
 
                     if (waitForPlay < 0.5f)
                     {
                         gameState = GameState.StartGame;
                         isMyTurn = true;
                         isFirst = true;
                         ClearPrefab();
                     }
                     break;
                 case Winner.Draw:
 
                     if (waitForPlay < 0.5f)
                     {
                         ClearPrefab();
                         ClearForDraw();
                         gameState = GameState.SelectRPS;
 
                     }
 
 
                     break;
 
                 case Winner.Loss:
 
                     if (waitForPlay < 0.5f)
                    {
                        gameState = GameState.StartGame;
                        isMyTurn = false;
                        ClearPrefab();
                    }
                     break;
             }
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
 

    void StartScreen() //최초 시작 화면
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

    
    void CheckOpponentNickName()  //첫 통신시 상대편 닉네임 확인
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



    void PrintPlayersNickName()  // 각 플레이어 이름 생성
    {
        GUI.Label(new Rect(width - 100f, 30, 120, 20), "이름 : "+nickName);
       // Debug.Log(nickName);
        GUI.Label(new Rect(20f, 30, 120, 20), "상대방 : "+ oppNickName);
       // Debug.Log("printopp  :"+oppNickName);
    }

    void SpawnBgAndRPS()  //가위바위보 위에 뒷배경 회색과 가위바위보 생성
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

    void ProcessingRPSSelect()  //가위바위보 골랐으면 그거 없애고 선택한거만 생성
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

    private int CastIntFromRPS(RPSKind select) //RPS 중 하나 생성하기위해 바꾸기 용도
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

         void WaitFiveSeconds()
      {
          if (waitTime > 0.5f)
          {
              font.fontSize = 100;
              waitTime -= Time.deltaTime;
             if(waitTime>=1.3f)
              GUI.Label(new Rect(Screen.width / 2-20, Screen.height / 2-70, 200, 50),( (int)waitTime).ToString(), font);
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

    void showMyTurnOrOppTurn()
    {
        if (isMyTurn)
        {
            
          

            GameObject  obj = GameObject.Find("MYTURN");
            if (obj == null)
            {
                GameObject obj2 = GameObject.Find("OPPTURN");
                Destroy(obj2);


                obj = Instantiate(myTurnShowPrefab);
                obj.GetComponent<Transform>().position = new Vector3(7f, 4, 0);
                obj.name = "MYTURN";
            }


        }
        else
        {
           

            GameObject obj = GameObject.Find("OPPTURN");
            if (obj == null)
            {
                GameObject obj2 = GameObject.Find("MYTURN");
                Destroy(obj2);

                obj = Instantiate(oppTurnShowPrefab);
                obj.GetComponent<Transform>().position = new Vector3(-7f, 4, 0);
                obj.name = "OPPTURN";
            }
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
            nameList.Clear();

        }
    }
  

    void SpawnPoints()  //포인트 상황 표현
    {
        GameObject obj1 = GameObject.Find("oppPoints");
        GameObject obj2 = GameObject.Find("myPoints");

        if (obj1 == null )
        {
            obj1 = Instantiate(oppPointsPrefab);


            obj1.GetComponent<Transform>().position = new Vector3(-7.72f, 3.22f, 0);
           // obj1.GetComponent<Transform>().position = tPos;
            obj1.name = "oppPoints";
            nameList.Add(obj1.name);

           
            Vector3 tmpPos = Camera.main.WorldToScreenPoint(new Vector3(-8.53f,3.22f,0));
            //Vector3 tmpPos = Camera.main.WorldToScreenPoint(new Vector3(0,0,0 ));
           // Vector3 tmpPos = Camera.main.ScreenToWorldPoint(new Vector3(839, 472, 0));
            Debug.Log(Camera.main.pixelWidth + " camera " + Camera.main.pixelHeight);
            Debug.Log(tmpPos + " screentoworld");

            

        }

         if(obj2==null)
          {
                obj2 = Instantiate(myPointsPrefab);
                Vector3 tmpPos = Camera.main.ScreenToWorldPoint(new Vector3(717.8f,340.8f,10.0f));
               obj2.GetComponent<Transform>().position = new Vector3(5.5f, 3.22f, 0);
               // obj2.GetComponent<Transform>().position = tmpPos;
                Debug.Log(tmpPos + " obj2 screentoworld");
                obj2.name = "myPoints";
                nameList.Add(obj2.name);
                
          }

    if(obj1!=null && obj2!=null)
        {
            gameState = GameState.Proceed;
        }
    }

    void MakeTextAreaAndButtonForSend()
    {
        if(isMyTurn)
        {
            pointString = GUI.TextField(new Rect(Screen.width / 2 - 40, Screen.height / 2 - 70, 100, 50), pointString);
            if(GUI.Button(new Rect(Screen.width / 2 - 40, Screen.height / 2, 100, 50), "보 내 기"))
            {

                try
                {
                    myPoint = int.Parse(pointString);
                }

                catch
                {
                    GameObject obj = GameObject.Find("Again");
                    obj = Instantiate(editAgainPrefab);
                    obj.name = "Again";
                    obj.GetComponent<Transform>().position = new Vector3(-3, 5, 0);
                    waitTwoSecond = 2.0f;
                }

                if (myPoint > 99 || myPoint < 0 || myPointController.points-myPoint<0)
                {
                    waitTwoSecond = 2.0f;
                    pointString = "";
                    myPoint = -1;
                    GameObject obj = GameObject.Find("Again");
                    if (obj == null)
                    {

                        obj = Instantiate(editAgainPrefab);
                        obj.name = "Again";
                        obj.GetComponent<Transform>().position = new Vector3(-3, 5, 0);
                    }

                }
                else
                {
                    networkController.SendPoints(myPoint);
                    myPointController.SetPoint(myPointController.points - myPoint);

                    Debug.Log(myPointController.getPoint() + " mypoint ");
                    isMyTurn = false;

                }
            }
        }
    }


    void CreatePointResult()
    {
        if(myPoint!=-1)
        {
            if(myPoint<10)
            {
                GameObject obj = GameObject.Find("myBlack");
                if(obj==null)
                {
                    obj = Instantiate(blackForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(4, 1, 0);
                    obj.name = "myBlack";
                    gameInitList.Add(obj.name);
                }
            }

            else
            {
                GameObject obj = GameObject.Find("myWhite");
                if (obj == null)
                {
                    obj = Instantiate(whiteForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(4, 1, 0);
                    obj.name = "myWhite";
                    gameInitList.Add(obj.name);
                }
            }
        }
    }

    void ReceiveOppPointSecondAttack()
    {
        if(oppPoint==-1 && isFirst==false)
        {
           oppPoint=networkController.ReceivePoint();
        }
    }

    void ReceiveOppPointFirstAttack()
    {
        if (oppPoint == -1 && isFirst == true)
        {
            oppPoint = networkController.ReceivePoint();
        }
    }

    void ChangeGameStateForSecondAttacker()
    {
        if (oppPoint != -1 && myPoint != -1)
            gameState = GameState.ShowResult;
    }

    void WaitingSignOppPointAndCreateOppResultForFirstAttacker()
    {
        if (oppPoint == -1 && isFirst)
        {
            GameObject obj = GameObject.Find("Waiting");
            if (obj == null)
            {
                obj = Instantiate(waitingSignPrefab);
                obj.GetComponent<Transform>().position = new Vector3(-2.5f, 5, 0);
                obj.name = "Waiting";
            }
        }
        else if(oppPoint!=-1 && isFirst)
        {
            if (isReceivePoint)
            {
                isReceivePoint = true;
                oppPointController.SetPoint(oppPointController.points - oppPoint);
             
               // oppPointController.CheckPoint();
            }

            try
            {
                GameObject obj = GameObject.Find("Waiting");
                Destroy(obj);
            }
            catch
            {

            }


            if (oppPoint < 10)
            {
                GameObject obj = GameObject.Find("oppBlack");
                if (obj == null)
                {
                    obj = Instantiate(blackForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(-4, 1, 0);
                    obj.name = "oppBlack";
                    gameInitList.Add(obj.name);
                }
            }

            else
            {
                GameObject obj = GameObject.Find("oppWhite");
                if (obj == null)
                {
                    obj = Instantiate(whiteForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(-4, 1, 0);
                    obj.name = "oppWhite";
                    gameInitList.Add(obj.name);
                }
            }
        }
    }

    void WaitingSignOppPointAndCreateOppResultForSecondAttacker()
    {
        if(oppPoint==-1 && isFirst==false)
        {
            GameObject obj = GameObject.Find("Waiting");
            if (obj == null)
            {
                obj = Instantiate(waitingSignPrefab);
                obj.GetComponent<Transform>().position = new Vector3(-2.5f, 5, 0);
                obj.name = "Waiting";
            }
        }
        else if(oppPoint!=-1 && isFirst==false)
        {
            if (!isMyTurn)
            {
                isMyTurn = true;
                oppPointController.points = oppPointController.points - oppPoint;
                oppPointController.CheckPoint();
            }
            try
            {
                GameObject obj = GameObject.Find("Waiting");
                Destroy(obj);
            }
            catch
            {

            }


            if (oppPoint < 10)
            {
                GameObject obj = GameObject.Find("oppBlack");
                if (obj == null)
                {
                    obj = Instantiate(blackForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(-5, 1, 0);
                    obj.name = "oppBlack";
                    gameInitList.Add(obj.name);
                }
            }

            else
            {
                GameObject obj = GameObject.Find("oppWhite");
                if (obj == null)
                {
                    obj = Instantiate(whiteForPointResultPrefab);
                    obj.GetComponent<Transform>().position = new Vector3(-5, 1, 0);
                    obj.name = "oppWhite";
                    gameInitList.Add(obj.name);
                }
            }
            
        }
    }

    void DestroyAgainTextAfterTwoSeconds()
    {
       

        if(waitTwoSecond>0.5f)
        {
            waitTwoSecond -= Time.deltaTime;
 
            
        }

        else
        {
            GameObject obj = GameObject.Find("Again");
            Destroy(obj);
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

        nickName = "닉네임"; //내 닉네임
        selectedRPS = RPSKind.None;
        selectedOppRPS = RPSKind.None;
        oppNickName = ""; //상대편 닉네임

        winner = Winner.None;
        waitTime = 5.0f; //기다리는 시간
         waitThreeSec = 3.0f;
         waitForPlay = 3.0f;
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
