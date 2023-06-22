using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCNametag : MonoBehaviour
{
    public NPCBehavior npc;
    Text nametag;

    // Start is called before the first frame update
    void Start()
    {
        nametag = GetComponent<Text>();
        nametag.text = npc.npcName;
    }
}
