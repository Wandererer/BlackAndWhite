using UnityEngine;
using System.Collections;


    public enum RPSKind
    {
        None = -1,		// 미결정.
        Rock = 0,		// 바위.
        Paper,			// 보.
        Scissor,		// 가위.
    };



    public struct InputData
    {
        public RPSKind rpsKind;
    }


    public enum Winner
    {
        None = 0,		// 미결정.
        ServerPlayer,	// 서버 쪽(1P) 승리.
        ClientPlayer,	// 클라이언트 쪽(2P) 승리.
        Draw,			// 무승부.
    };


    class ResultChecker
    {
        public static Winner GetRPSWinner(RPSKind server, RPSKind client)
        {
            // 1P와 2P의 수를 수치화합니다.
            int serverRPS = (int)server;
            int clientRPS = (int)client;

            if (serverRPS == clientRPS)
            {
                return Winner.Draw; //무승부.
            }

            // 수치의 차이를 이용해 처리 판정을 합니다.
            if (serverRPS == (clientRPS + 1) % 3)  //서버가 바위이고 클라이언트가 가위이면 +1하고 3으로 나머지하면 0이 나오므로 같아지므로 클라이언트가 지게됨
            {
                return Winner.ServerPlayer;  //1P 승리.
            }
            return Winner.ClientPlayer; //2P 승리.
        }



        static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new System.Exception();
            }
        }

        public static void WinnerTest()
        {

            Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Paper) == Winner.Draw);
            Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Rock) == Winner.ServerPlayer);
            Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Scissor) == Winner.ClientPlayer);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Paper) == Winner.ClientPlayer);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Rock) == Winner.Draw);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Scissor) == Winner.ServerPlayer);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Paper) == Winner.ServerPlayer);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Rock) == Winner.ClientPlayer);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Scissor) == Winner.Draw);
        }






    }

