using UnityEngine;
using System.Collections;

public class RPSPanel : MonoBehaviour {

    public RPSKind rpsKind;
    bool isSelected;  //선택됐을 때는 true.

    enum State
    {
      
        SelectWait, //선택 대기.
        OnSelected, //선택됨.
        UnSelected, //선택되지 않음.
        End,
    }
    State state;

	// Use this for initialization
	void Start () {
        state = State.SelectWait;
        isSelected = false;

        transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	
        switch(state)
        {
            case State.SelectWait:
                UpdateSelectWait();
                break;
        }
	}

    void UpdateSelectWait()
    {
        if (IsHit())
        {
            //커서가 올라갔을 때 효과음을 울립니다.
          
            //선택 범위에 들어있으면 확대 표시.
            transform.localScale = Vector3.one * 1.2f;
            if (Input.GetMouseButtonDown(0))
            {
                isSelected = true;    //클릭됨.    
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }


    bool IsHit()
    {
        GameObject obj = GameObject.Find("Camera");
        Ray ray = obj.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        return GetComponent<Collider>().Raycast(ray, out raycastHit, 100);
    }



    //클릭됐으면 true.
    public bool IsSelected()
    {
        return isSelected;
    }

    //종료됐으면true.
    public bool IsEnd()
    {
        return (state == State.End);
    }

    //가위바위보 결정 후 연출로 전환한다.
    public void ChangeSelectedState()
    {
        state = State.UnSelected;
        if (isSelected)
        {
            state = State.OnSelected;
        }
    }
}
