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
        Win,
        Loss,	
        Draw,			// 무승부.
    };


    class ResultChecker
    {
        public static Winner GetRPSWinner(RPSKind my, RPSKind opp)
        {
            // 1P와 2P의 수를 수치화합니다.
            int myRPS = (int)my;
            int oppRPS = (int)opp;

            Debug.Log(myRPS + " 데이터  " + oppRPS);

            if (myRPS == oppRPS)
            {
                return Winner.Draw; //무승부.
            }

            // 수치의 차이를 이용해 처리 판정을 합니다.
            if (myRPS == (oppRPS + 1) % 3)  //내가 바위이고 상대가 가위이면 +1하고 3으로 나머지하면 0이 나오므로 같아지므로 상대가 지게됨
            {
                return Winner.Win;  //내가 승리
            }
            return Winner.Loss; //내가짐
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
            Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Rock) == Winner.Win);
            Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Scissor) == Winner.Loss);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Paper) == Winner.Loss);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Rock) == Winner.Draw);
            Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Scissor) == Winner.Win);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Paper) == Winner.Win);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Rock) == Winner.Loss);
            Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Scissor) == Winner.Draw);
        }






    }

