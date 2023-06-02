using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum QuestState {
    Initiate,
    Accept,
    Reject,
    InProgress,
    Complete
}

public class QuestManager : MonoBehaviour
{
    public Quest quest;
    public Quest activeQuest;
    public bool showGUI = false;
    public float maxInteractDistance = 2.0f;
    public QuestState questState = QuestState.Initiate;
    public PlayerInventory inventory;
<<<<<<< Updated upstream
    
    Rect titleWindow = new Rect(0, 0, 200, 20);
    Rect textWindow = new Rect(0, 20, 200, 130);
=======
    Rect titleWindow = new Rect(50, 40, 200, 20);
    Rect textWindow = new Rect(50, 60, 200, 130);
>>>>>>> Stashed changes

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (questState == QuestState.Accept)
            {
                questState = QuestState.InProgress;
            }

            ToggleUI(false);
        }

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
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
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
                case QuestState.Complete:
                    GUI.TextField(textWindow, quest.questCompleteDialogue, style);
                    break;
            }
        }
    }

    void InitiateGUI(GUIStyle style) {
        GUI.TextField(textWindow, quest.questDialogue + "\n\nRequirement: " + quest.requiredPotion, style);
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
        GUI.TextField(textWindow, activeQuest.questInProgressDialogue, style);

        // submitting code
        if (GUI.Button(new Rect(0, 150, 95, 20), "Complete"))
        {
            Debug.Log("complete button pressed");
            // do something with the inventory here or smthn

            List<StoredItem> ingredients = inventory.GetItemsInInventory();
            StoredItem potion = CheckInventory(ingredients, activeQuest.requiredPotion);
            if (potion != null) {
                Debug.Log("quest complete!");
                questState = QuestState.Complete;
                inventory.RemoveFromInventory(potion);
            } else {
                Debug.Log("missing ingredients!");
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
        Debug.Log("quest initiated!");

        ToggleUI();

        questState = QuestState.Initiate;

        quest = npcQuest;
    }

    void ToggleUI(bool flag = true) {
        // gui stuff
        showGUI = flag;
        MouseLook.isUIActive = flag;
        Cursor.visible = flag;
        Cursor.lockState = flag ? CursorLockMode.Confined : CursorLockMode.Locked;
        
        // disabling character movement
        gameObject.GetComponent<CharacterController>().enabled = !flag;
    }
}
