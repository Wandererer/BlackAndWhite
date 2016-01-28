using UnityEngine;
using System.Collections;

public class NetworkController  {
    const int PORT = 50765;
    TCP network;
    public bool istrue;
    //서버 클라이언트 판정용.
    public enum HostType
    {
        Server,
        Client,
    };
    HostType hostType;


    public NetworkController()
    {
       
        hostType = HostType.Server;

        GameObject nObj = GameObject.Find("Network");
        network = nObj.GetComponent<TCP>();
        istrue=network.StartServer(PORT, 1);
    }

    //클라이언트에서 사용할 때.
    public NetworkController(string serverAddress)
    {
        hostType = HostType.Client;

        GameObject nObj = GameObject.Find("Network");
        network = nObj.GetComponent<TCP>();
        network.Connect(serverAddress, PORT);
    }


    public bool IsConnected()
    {
        return network.IsConnected();
    }


    public void SendRPSData(RPSKind rpsKind)
    {
        // 구조체를 byte배열로 변환합니다.
        byte[] data = new byte[1];
        data[0] = (byte)rpsKind;

        // 데이터를 송신합니다.
        network.Send(data, data.Length);
    }

    public RPSKind ReceiveRPSData()
    {
        byte[] data = new byte[1024];

        // 데이터를 수신합니다.
        int recvSize = network.Receive(ref data, data.Length);
        if (recvSize < 0)
        {
            // 입력 정보를 수신하지 않음.
            return RPSKind.None;
        }

        // byte 배열을 구조체로 변환합니다.
        RPSKind rps = (RPSKind)data[0];

        Debug.Log("rps-" + data+"-rps");

        if (data[0] > 2 || data[0] < -1)
            return RPSKind.None;

   
        return rps;
    }

    public void SendNickName(string nickName)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(nickName);

        network.Send(data, data.Length);

    }

    public string ReceiveNickName()
    {
        byte[] data = new byte[1024];
        string name="";

       

        int recvSize = network.Receive(ref data, data.Length);
        if(recvSize>0)
        {
            name = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log(data +"   recevie data nickname");
        }



        return name;
        
    }

    public void SendPoints(int point)
    {
        byte[] data = new byte[1];

        data[0] = (byte)point;

        network.Send(data, data.Length);
    }

    public int ReceivePoint()
    {
        byte[] data = new byte[1024];
       

        int recvSize = network.Receive(ref data, data.Length);
       if(recvSize<0)
       {
           return -1;
       }
       int point = (int)data[0];

       Debug.Log("point comming " + point);

       return point;
    }

    public void StopServer()
    {
        network.StopServer();
    }
	

}
