using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState {
    Initiate,
    Accept,
    Reject,
    InProgress,
    MissingIngredients,
    Complete
}

public class QuestManager : MonoBehaviour, IGUIScreen
{
    public Quest quest;
    public Quest activeQuest;
    public bool showGUI = false;
    public float maxInteractDistance = 2.0f;
    public QuestState questState = QuestState.Initiate;
    public PlayerInventory inventory;
    public string missingIngredientsDialogue = "You don't have what I need!\nCome back when you do!";
    Rect titleWindow = new Rect(50, 40, 200, 20);
    Rect textWindow = new Rect(50, 60, 200, 130);

    // Start is called before the first frame update
    void Start()
    {
        if (inventory == null)
        {
            inventory = gameObject.GetComponent<PlayerInventory>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // send out raycast to see if player is looking at npc
        if (Input.GetMouseButtonDown(1) && !showGUI)
        {
            Interact();
        }
    }

    void Interact() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxInteractDistance))
        {
            if (hit.collider.gameObject.CompareTag("NPC"))
            {
                NPCBehavior npc = hit.collider.gameObject.GetComponent<NPCBehavior>();

                // if player interacts with npc with active quest
                if (npc.quest != null && activeQuest && activeQuest.questName == npc.quest.questName)
                {
                    questState = npc.questState;
                    quest = npc.quest;

                    ToggleUI();
                } 
                // if player interacts with npc, with no active quest
                else if (npc.quest != null && npc.questState == QuestState.Initiate) 
                {
                    InitiateQuest(npc.quest);
                }
                else
                {
                    Debug.Log("how'd you get here?");
                }
            }
        }
    }

    private void OnGUI() {
        GUIStyle style = new GUIStyle(GUI.skin.textArea);
        style.wordWrap = true;
        
        if (showGUI) {
            GUI.TextField(titleWindow, "Quest: " + quest.questName, style);
            switch (questState) {
                case QuestState.Initiate:
                    InitiateGUI(style);
                    break;
                case QuestState.Accept:
                    GUI.TextField(textWindow, quest.questAcceptDialogue, style);
                    break;
                case QuestState.Reject:
                    GUI.TextField(textWindow, quest.questRejectDialogue, style);
                    break;
                case QuestState.InProgress:
                    InProgressGUI(style);
                    break;
                case QuestState.MissingIngredients:
                    GUI.TextField(textWindow, missingIngredientsDialogue, style);
                    break;
                case QuestState.Complete:
                    GUI.TextField(textWindow, quest.questCompleteDialogue, style);
                    break;
            }
        }
    }

    void InitiateGUI(GUIStyle style) {
        GUI.TextField(textWindow, quest.questDialogue + "\n\nRequirement: " + quest.requiredPotion.CommonName, style);
        if (GUI.Button(new Rect(50, 200, 95, 20), "Accept"))
        {
            Debug.Log("accept button pressed");
            questState = QuestState.Accept;
            activeQuest = quest;
        }

        if (GUI.Button(new Rect(155, 200, 95, 20), "Reject"))
        {
            Debug.Log("reject button pressed");
            questState = QuestState.Reject;
        }
    }

    void InProgressGUI(GUIStyle style) {
        string infoText = activeQuest.questInProgressDialogue + "\n\nRequirement: " + activeQuest.requiredPotion.CommonName;
        GUI.TextField(textWindow, infoText, style);

        // submitting code
        if (GUI.Button(new Rect(50, 200, 95, 20), "Complete"))
        {
            bool success = inventory.TryTakeOutItem(activeQuest.requiredPotion);
            if (success) {
                Debug.Log("quest complete!");
                questState = QuestState.Complete;

                Invoke("GiveReward", 2.0f);
            } else {
                Debug.Log("missing ingredients!");
                questState = QuestState.MissingIngredients;
            }
        }
    }

    StoredItem CheckInventory(List<StoredItem> ingredients, string potionName) {
        foreach (StoredItem ingredient in ingredients) {
            if (ingredient.Details.CommonName == potionName) {
                return ingredient;
            }
        }

        return null;
    }

    // first screen when seeing quest
    public void InitiateQuest(Quest npcQuest)
    {

        if(ToggleUI())
        {
            Debug.Log("quest initiated!");
            questState = QuestState.Initiate;

            quest = npcQuest;
        } else
        {
            Debug.Log("Tried to initiate quest but UI couldn't open.");
        }
    }

    private bool ToggleUI(bool flag = true) {
        // gui stuff
        if(flag)
        {
            // Tell UIController to activate this UI
            bool succeeded = UIController.Instance.ActivateQuestManager();
            if(succeeded)
            {
                // We successfully activated the UI.
            } else
            {
                // The UI could not be activated (something else is probably open already)
            }

            return succeeded;
        } else
        {
            // Tell UIController to deactivate this UI
            UIController.Instance.DeactivateQuestManager();
            return true;
        }
    }

    void GiveReward() {
        ToggleUI(false);
        inventory.OpenInventoryWithItem(activeQuest.rewardItem);
    }


    //  IGUIScreen implementation
    public bool isGUIActive()
    {
        return showGUI;
    }

    public void activateGUI()
    {
        showGUI = true;
    }

    public void deactivateGUI()
    {
        if (questState == QuestState.Accept)
        {
            questState = QuestState.InProgress;
        }
        else if (questState == QuestState.MissingIngredients)
        {
            questState = QuestState.InProgress;
        }
        else
        {
            Debug.Log("how'd you get here?");
        }

        deactivateGUI();
    }
}
