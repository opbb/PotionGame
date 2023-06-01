using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public string questInfo;
    public float questTime = 100;

    bool inQuest = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    void QuestAccept()
    {
        
    }

    // Rejecting quest ui
    void QuestReject()
    {

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
