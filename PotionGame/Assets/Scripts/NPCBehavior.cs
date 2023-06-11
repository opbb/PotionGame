using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public string npcName;
    public string[] dialogue;
    public Quest quest;
    public QuestState questState = QuestState.Initiate;
    public GameObject player;
    public float minDistance = 20.0f;
    public float activateQuestDistance = 2.0f;
    public float moveSpeed = 1;
    public float rejectCooldown = 30.0f;
    float rejectTimer = 0.0f;

    Rigidbody rb;
    QuestManager questManager;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        questManager = player.GetComponent<QuestManager>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // if this NPC's quest is active, update quest state to match quest manager
        if (questManager.activeQuest != null && questManager.activeQuest.questName == quest.questName) {
            questState = questManager.questState;
        }

        // will only follow player if quest is not initiated
        if (questState != QuestState.Initiate) {
            return;
        }

        // if player is in quest menu, and this NPC's quest is active, make player look at npc
        if (questManager.showGUI && questManager.quest == quest) {
            var lookPos = transform.position - player.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rotation, Time.deltaTime * 5);
            return;
        }

        // cooldown for following player if player rejects quest
        if (rejectTimer > 0.0f) {
            rejectTimer -= Time.deltaTime;
            return;
        }

        // following player/activating quest
        var distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= activateQuestDistance) {
            questManager.InitiateQuest(quest);
            rejectTimer = rejectCooldown;
        } 
        else if (distance <= minDistance) {
            transform.LookAt(player.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            rb.velocity = transform.forward * moveSpeed;
        }
    }
}
