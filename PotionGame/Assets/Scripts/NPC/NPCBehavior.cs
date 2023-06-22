using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public Vector3[] waypoints = new Vector3[4];
    public float waypointSearchRadius = 10.0f;
    int targetWaypointIndex = 0;

    NavMeshAgent agent;
    Animator anim;
    QuestManager questManager;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        questManager = player.GetComponent<QuestManager>();
        // generate random waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = Random.insideUnitSphere * waypointSearchRadius + transform.position;
            waypoints[i].y = 0;
        }

        anim.SetInteger("animState", 0);

        GetQuestStateFromPrefs();

        if (questState != QuestState.Initiate && questState != QuestState.Complete)
        {
            questManager.activeQuest = quest;
            questManager.questState = questState;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if this NPC's quest is active, update quest state to match quest manager
        if (questManager.activeQuest != null && questManager.activeQuest.questName == quest.questName && questState != questManager.questState)
        {
            questState = questManager.questState;
            SetQuestStateToPrefs();
        }

        if (questState != QuestState.Initiate)
        {
            // this will need to be improved no doubt
            Vector3 targetWaypoint = waypoints[targetWaypointIndex];
            targetWaypoint.y = transform.position.y;
            var distanceTo = Vector3.Distance(transform.position, targetWaypoint);
            if (distanceTo <= 1.0f)
            {
                targetWaypointIndex++;
                if (targetWaypointIndex >= waypoints.Length)
                {
                    targetWaypointIndex = 0;
                }
            }
            transform.LookAt(waypoints[targetWaypointIndex]);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            anim.SetInteger("animState", 1);
            agent.SetDestination(waypoints[targetWaypointIndex]);
            return;
        }

        // if player is in quest menu, and this NPC's quest is active, make player look at npc
        if (questManager.showGUI && questManager.quest == quest)
        {
            var lookPos = transform.position - player.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            anim.SetInteger("animState", 0);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rotation, Time.deltaTime * 5);
            return;
        }
        else if (UIController.Instance.isUIActive())
        {
            return;
        }

        // cooldown for following player if player rejects quest
        if (rejectTimer > 0.0f)
        {
            rejectTimer -= Time.deltaTime;
            return;
        }

        // following player/activating quest
        var distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= activateQuestDistance)
        {
            questManager.InitiateQuest(quest);
            rejectTimer = rejectCooldown;
        }
        else if (distance <= minDistance && questManager.activeQuest == null)
        {
            transform.LookAt(player.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            anim.SetInteger("animState", 1);
            agent.SetDestination(player.transform.position);
        }
    }

    void GetQuestStateFromPrefs() {
        int state = PlayerPrefs.GetInt(quest.questName, 0);

        switch(state) {
            case 0:
                questState = QuestState.Initiate;
                break;
            case 1:
                questState = QuestState.InProgress;
                break;
            case 2:
                questState = QuestState.Complete;
                break;
            case 3:
                questState = QuestState.Initiate;
                break;
            default:
                questState = QuestState.Initiate;
                break;
        }
    }

    void SetQuestStateToPrefs() {
        int state = 0;

        switch(questState) {
            case QuestState.Initiate:
                state = 0;
                break;
            case QuestState.InProgress:
                state = 1;
                break;
            case QuestState.MissingIngredients:
                state = 1;
                break;
            case QuestState.Accept:
                state = 1;
                break;
            case QuestState.Complete:
                state = 2;
                break;
            case QuestState.Reject:
                state = 0;
                break;
            default:
                state = 0;
                break;
        }

        PlayerPrefs.SetInt(quest.questName, state);
    }
}