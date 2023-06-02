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

    QuestManager questManager;
    float yFloor = 0.0f;

    // Start is called before the first frame update
    void Start()
    {   
        yFloor = transform.position.y;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        questManager = player.GetComponent<QuestManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // will only follow player if quest is not initiated
        if (questState != QuestState.Initiate) {
            return;
        }

        // if player is in quest menu, don't act
        if (questManager.showGUI) {
            // making player look at npc, lerp over time
            var lookPos = transform.position - player.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rotation, Time.deltaTime * 5);
            return;
        }

        // if this quest is active
        if (questManager.activeQuest != null && questManager.activeQuest.questName == quest.questName) {
            questState = questManager.questState;
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
        } else if (distance <= minDistance) {
            // look at player, lock x and z axis
            transform.LookAt(player.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, yFloor, transform.position.z);
        }
    }
}
