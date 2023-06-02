using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest", order = 1)]

public class Quest : ScriptableObject
{
    public string questName;
    public ItemDefinition requiredPotion;
    public string questDialogue;
    public string questCompleteDialogue;
    public string questAcceptDialogue;
    public string questRejectDialogue;
    public string questInProgressDialogue;
}
