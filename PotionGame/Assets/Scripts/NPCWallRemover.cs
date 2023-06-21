using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWallRemover : MonoBehaviour
{
    public GameObject wall;
    NPCBehavior npcBehavior;

    // Start is called before the first frame update
    void Start()
    {
        npcBehavior = GetComponent<NPCBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (npcBehavior.questState == QuestState.Complete)
        {
            wall.SetActive(false);
        }
    }
}
