using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Text titleText;
    public Text descriptionText;
    public Button acceptButton;
    public Button rejectButton;
    public Button completeButton;

    private void OnGUI() {
        if (showGUI) {
            string title = "Quest: " + quest.questName;
            titleText.text = title;
            switch (questState) {
                case QuestState.Initiate:
                    InitiateGUI();
                    break;
                case QuestState.Accept:
                    descriptionText.text = quest.questAcceptDialogue;
                    break;
                case QuestState.Reject:
                    descriptionText.text = quest.questRejectDialogue;
                    break;
                case QuestState.InProgress:
                    InProgressGUI();
                    break;
                case QuestState.MissingIngredients:
                    descriptionText.text = missingIngredientsDialogue;
                    break;
                case QuestState.Complete:
                    descriptionText.text = quest.questCompleteDialogue;
                    break;
            }
        }
    }

    void InitiateGUI() {
        string infoText = quest.questDialogue + "\n\nRequirement: " + quest.requiredPotion.CommonName;
        descriptionText.text = infoText;

        bool loaded = acceptButton.gameObject.activeSelf;

        if (!loaded) {
            acceptButton.gameObject.SetActive(true);
            rejectButton.gameObject.SetActive(true);

            acceptButton.onClick.AddListener(() => {
                questState = QuestState.Accept;
                activeQuest = quest;

                acceptButton.gameObject.SetActive(false);
                rejectButton.gameObject.SetActive(false);

                acceptButton.onClick.RemoveAllListeners();
                rejectButton.onClick.RemoveAllListeners();
            });

            rejectButton.onClick.AddListener(() => {
                questState = QuestState.Reject;

                acceptButton.gameObject.SetActive(false);
                rejectButton.gameObject.SetActive(false);

                acceptButton.onClick.RemoveAllListeners();
                rejectButton.onClick.RemoveAllListeners();
            });
        }
    }

    void InProgressGUI() {
        string infoText = activeQuest.questInProgressDialogue + "\n\nRequirement: " + activeQuest.requiredPotion.CommonName;
        descriptionText.text = infoText;

        bool loaded = completeButton.gameObject.activeSelf;

        if (!loaded) {
            completeButton.gameObject.SetActive(true);

            completeButton.onClick.AddListener(() => {
                bool success = inventory.TryTakeOutItem(activeQuest.requiredPotion);
                if (success) {
                    questState = QuestState.Complete;

                    Invoke("GiveReward", 2.0f);
                } else {
                    questState = QuestState.MissingIngredients;
                }

                completeButton.gameObject.SetActive(false);
                completeButton.onClick.RemoveAllListeners();
            });
        }
    }

    // first screen when seeing quest
    public void InitiateQuest(Quest npcQuest)
    {

        if(ToggleUI())
        {
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
        titleText.transform.parent.gameObject.SetActive(true);
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

        showGUI = false;
        titleText.transform.parent.gameObject.SetActive(false);
    }
}
