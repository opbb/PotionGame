using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class QuestGiver : MonoBehaviour
{
    public NonScriptQuest quest;
    public GameObject player;

    public GameObject questWindow;
    public Text titleText;
    public Text descriptionText;
    public Text timerText;

    public void OpenQuestWindow()
    {
        questWindow.SetActive(true);
        titleText.text = quest.title;
        descriptionText.text = quest.description;
        timerText.text = (quest.time / 60).ToString() + "Minutes";
    }

    public void AcceptQuest()
    {
        questWindow.SetActive(false);
        quest.isActive = true;

        // give quest to player
        // make var in player script : Quest quest;

        //player.quest = quest;
    }
}
