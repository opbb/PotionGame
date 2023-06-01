using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public string questInfo;
    public float questTime = 100;

    public Text questText;

    bool inQuest = false;
    Rect textWindow = new Rect(0, 0, 200, 150);


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MouseLook.isUIActive = false;
            Cursor.visible = false;

        }
    }

    // first screen when seeing quest
    void SeeQuest()
    {
        if (inQuest == true)
        {
            QuestReject();
        }
        else
        {
            // if player accepts, run accept ui
            // if player rejects, run reject ui
        }

    }

    // Accepting quest ui
    public void QuestAccept()
    {
        
    }

    // Rejecting quest ui
    public void QuestReject()
    {
        if (inQuest == true)
        {
            GUI.TextField(textWindow, "Seems that you're too busy for this task at the moment." +
                "Finish what you're doing and come back when you have the time!");
        }
        else
        {
            GUI.TextField(textWindow, "You don't want to help me? Ok bye.");
        }
    }

    // Quest succeeded!
    void QuestWin()
    {
        inQuest = false;
    }

    // Quest lost :(
    void QuestLost()
    {
        inQuest = false;
    }
}
