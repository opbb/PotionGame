using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest", order = 1)]

public class Quests : ScriptableObject
{
    public string questName;
    public int questTime;
    public string requiredPotion;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
