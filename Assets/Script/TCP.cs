using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TCP : MonoBehaviour {

    private Socket listener = null; //리스닝 소켓

    private Socket socket = null;  //클라이언트 접속용 소켓

    private PacketQueue sendQueue; //송신 버퍼

    private PacketQueue recvQueue; //수신 버퍼

    private bool isServer = false;  //서버 인가

    private bool isConnected = false; //연결 되었나?..

    public delegate void EventHandler(NetEventState state); //이벤트 통지 델리게이트

    private EventHandler handler;

    protected bool threadLoop = false;

    protected Thread thread = null;

    private static int mtu = 1400;

	// Use this for initialization
	void Start () {
        sendQueue = new PacketQueue();
        recvQueue = new PacketQueue();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public bool StartServer(int port, int connectionNum)
    {
        Debug.Log("start server called");

        try
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //지정된 주소 패밀리, 소켓 종류, 프로토콜 사용하여 새인스턴스 초기화
            //                  ipv4                                                 tcp
            //SocketType.Stream
            /*
            데이터 중복이나 경계유지 없이 신뢰성 있는 양방향 연결 기반의 바이트 스트림을 지원합니다. 단일 피어와 통신하며 이 소켓을 사용하 ㄹ경우 통신을 시작하기전
             원격 호스트에 연결해야하고 ipv4랑 tcp를 반드시 사용해야함
             
            */
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            //IPEndPoint
            //네트워크의 끝점을 ip주소와 포트번호로 나타냄


            //IPAddress.Any
            //서버에서 모든 네트워크 인터페이스의 클라이언트 동작을 수신 대기해야 함을 나타내는 IP 주소를 제공합니다.이 필드는 읽기 전용입니다.


            listener.Listen(connectionNum);
        }
        catch
        {
            Debug.Log("StartServer fail");
            return false;
        }

        isServer = true;

        return LaunchThread();
    }


    public void StopServer()
    {
        threadLoop = false;

        if(thread!=null)
        {
            thread.Join();
            thread = null;
        }

        Disconnect();

        if(listener!=null)
        {
            listener.Close();
            listener = null;
        }

        isServer = false;

        Debug.Log("Server stopped");
    }

    public bool Connect(string address, int port)
    {
        Debug.Log("TransportTCP connect called.");

        if (listener != null)
            return false;

        bool ret = false;

        try
        {
            socket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay=true;
            socket.Connect(address, port);
            ret = LaunchThread();
        }
        catch
        {
            socket = null;
        }


        if(ret==true)
        {
            isConnected = true;
            Debug.Log("Connection success.");
        }

        if(handler!=null)
        {
            NetEventState state = new NetEventState();//접속 결과 통지해야함
            state.type = NetEventType.Connect;
            state.result = (isConnected == true) ? NetEventResult.Success : NetEventResult.Failure;
            handler(state);
            Debug.Log("event handler called");


        }

        return isConnected;
    }


    public void Disconnect()
    {
        //접속 종료
        isConnected = false;

        if(socket!=null)
        {
            socket.Shutdown(SocketShutdown.Both);
            /* Receive 받기에 대한 Socket을 비활성화합니다. 이 필드는 상수입니다.  
            Send 보내기에 대한 Socket을 비활성화합니다. 이 필드는 상수입니다.  
             Both 보내기와 받기 모두에 대한 Socket을 비활성화합니다. 이 필드는 상수입니다.  
            */
            socket.Close();
            socket = null;

            if(handler!=null)
            {
                NetEventState state = new NetEventState();
                state.type = NetEventType.Disconnect;
                state.result = NetEventResult.Success;
                handler(state);
            }
        }
    }


    public int Send(byte[] data,int size)
    {
        if (sendQueue == null)
            return 0;

        return sendQueue.Enqueue(data, size);
    }

    public int Receive(ref byte[] buffer,int size)
    {
        if (recvQueue == null)
            return 0;

        return recvQueue.Dequeue(ref buffer, size);
    }

    public void RegisterEventHandler(EventHandler thandler)
    {
        handler += thandler;
    }

    public void UnregisterEventHandler(EventHandler thandler)
    {
        handler -= thandler;
    }

    bool LaunchThread()
    {
        try
        {
            threadLoop = true;
            thread = new Thread(new ThreadStart(Dispatch));
            thread.Start();
        }

        catch
        {
            Debug.Log("Cannot launch Thread.");
            return false;
        }

        return true;
    }

    public void Dispatch()
    {
        Debug.Log("Dispatch thread started.");

        while(threadLoop)
        {
            AcceptClient();

            if(socket!=null && isConnected==true)
            {
                DispatchSend();


                DispatchReceive();
            }

            Thread.Sleep(5);
        }
    }


    //SelectMode
    //Socket.Poll 메서드에 대한 폴링 모드를 정의합니다.
    /*
     * SelectError 

오류 상태 모드입니다.
 
 SelectRead 

읽기 상태 모드입니다.
 
 SelectWrite 

쓰기 상태 모드입니다.
 
*/

    void AcceptClient()
    {
        if(listener!=null && listener.Poll(0,SelectMode.SelectRead))
        {
            //z클라이언트에 접속됨
            socket = listener.Accept();
            isConnected = true;
            if (handler != null)
            {
                NetEventState state = new NetEventState();
                state.type = NetEventType.Connect;
                state.result = NetEventResult.Success;
                handler(state);
            }

            Debug.Log("Connected from client.");

       }
    }


    void DispatchSend()
    {
        try
        {
            if (socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] buffer = new byte[mtu];

                int sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);

                while (sendSize > 0)
                {
                    socket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }

        }
        catch
        {
            return;
        }
    }

    void DispatchReceive()
    {
        try
        {
            if (socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[mtu];

                int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if(recvSize==0)
                { 
                    //졉속 종료
                    Debug.Log("Disconnect recv from client.");
                    Disconnect();
                }
                else if(recvSize>0)
                {
                    recvQueue.Enqueue(buffer, recvSize);
                }
            }

        }
        catch
        {
            return;
        }
    }



    public bool IsServer()
    {
        return isServer;
    }

    // 접속 확인.
    public bool IsConnected()
    {
        return isConnected;
    }




}
