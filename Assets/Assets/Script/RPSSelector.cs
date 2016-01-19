using UnityEngine;
using System.Collections;

public class RPSSelector : MonoBehaviour {

    RPSKind selected;
    private int selectedRPS = -1;


	// Use this for initialization
	void Start () {
        selected = RPSKind.None;
	}
	
	// Update is called once per frame
	void Update () {
        if (selected != RPSKind.None)
            return; //선택 됬으므로 아무것도 하지 않는다


        RPSPanel[] panels = transform.GetComponentsInChildren<RPSPanel>();
        foreach(RPSPanel p in panels)
        {
            if (p.IsSelected())
            {
                selected = p.rpsKind;
                Debug.Log("Clickedrps");
                Debug.Log(selected);

              
            }

        }

        if(selected!=RPSKind.None)
        {
            foreach (RPSPanel p in panels)
                p.ChangeSelectedState();
        }
	}



    public RPSKind GetRPSKind()
    {
        RPSPanel[] panels = transform.GetComponentsInChildren<RPSPanel>();
        foreach (RPSPanel p in panels)
        {
            if (p.IsSelected() == false)
            {   // 연출 대기일 때는 미결정으로 다룹니다.
                return RPSKind.None;
            }
        }

        return selected;
    }

    public RPSKind GetRps()
    {
        return selected;
    }


    
}
