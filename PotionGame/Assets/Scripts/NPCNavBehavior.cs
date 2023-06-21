using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavBehavior : MonoBehaviour
{
    public enum FSMStates
    {
        Patrol,
        Chase,
        Interact
    }

    [SerializeField] public Quest quest;
    // NPC Movement Variables
    [SerializeField] FSMStates currentState;
    [SerializeField] GameObject player;

    [SerializeField] float chaseDistance = 20f;
    [SerializeField] float interactDistance = 2f;

    // NPC Vision
    [SerializeField] Transform enemyEyes;
    [SerializeField] float fieldOfView = 45f;

    private NavMeshAgent agent;
    private Vector3 nextDestination;
    private Animator anim;
    private int currentDestinationIndex = 0;
    private float distanceToPlayer;
    private GameObject[] wanderPoints; 
    private QuestManager questManager;
    private bool isInteracting = false; 

    // Start is called before the first frame update
    void Start()
    {
        // initalize variables
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        wanderPoints = GameObject.FindGameObjectsWithTag("WanderPoint");
        questManager = player.GetComponent<QuestManager>();

        // Set the current state/anim
        currentState = FSMStates.Patrol;
        anim = GetComponent<Animator>();

        FindNextPoint();
    }

    private void FindNextPoint()
    {
        nextDestination = wanderPoints[currentDestinationIndex].transform.position;
        // cycle from 0, 1, 2, ...u
        currentDestinationIndex = (currentDestinationIndex + 1) % wanderPoints.Length;
    }

    // Update is called once per frame
    void Update()
    {      
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case FSMStates.Patrol:
                UpdatePatrolState();
                break;
            case FSMStates.Chase:
                UpdateChaseState();
                break;
            case FSMStates.Interact:
                UpdateInteractState();
                break;
        }
    }

    private void UpdatePatrolState()
    {
        print("patrolling");
        if (questManager.questState != QuestState.Initiate)
        {
            currentState = FSMStates.Patrol;
        }

        agent.isStopped = false;
        anim.SetInteger("animState", 1);
        if (Vector3.Distance(transform.position, nextDestination) < 3f)
        {
            print("finding next point");
            FindNextPoint();
        }
        else if ((distanceToPlayer <= chaseDistance) && IsPlayerInClearFOV() && questManager.questState != QuestState.InProgress)
        {
            currentState = FSMStates.Chase;
        } else if ((distanceToPlayer <= interactDistance) && IsPlayerInClearFOV())
        {
            currentState = FSMStates.Interact;
        }

        FaceTarget(nextDestination);
        agent.SetDestination(nextDestination);
    }

    private void UpdateChaseState()
    {
        agent.isStopped = false;
        print("chasing player");

        nextDestination = player.transform.position;

        if (questManager.questState == QuestState.InProgress)
        {
            currentState = FSMStates.Patrol;
            FindNextPoint();
        }
        else if (distanceToPlayer <= interactDistance)
        {
            currentState = FSMStates.Interact;
        }
        else if (distanceToPlayer > chaseDistance)
        {
            FindNextPoint();
            currentState = FSMStates.Patrol;
        }

        FaceTarget(nextDestination);
        agent.SetDestination(nextDestination);
    }

    private void UpdateInteractState()
    {
        print("interacting with player");
        agent.isStopped = true;
        anim.SetInteger("animState", 0);

        nextDestination = player.transform.position;

        // Initaite quest GUI, use isInteracting to display GUI once
        if (!isInteracting)
        {
            //questManager.HandleInteraction(quest);
            isInteracting = true;
        }


        // After quest is accept resume to Patrol state
        if (questManager.questState == QuestState.InProgress)
        {
            currentState = FSMStates.Patrol;
        }

        if (distanceToPlayer <= interactDistance)
        {
            currentState = FSMStates.Interact;
        }
        else if (distanceToPlayer > interactDistance && distanceToPlayer <= chaseDistance)
        {
            currentState = FSMStates.Chase;
        }
        else if (distanceToPlayer > chaseDistance)
        {
            currentState = FSMStates.Patrol;
        } 
        FaceTarget(nextDestination);
    }

    private void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
    }

    private bool IsPlayerInClearFOV()
    {
        RaycastHit hit;

        Vector3 directionToPlayer = player.transform.position - enemyEyes.position;

        // check if player is within field of view
        if (Vector3.Angle(directionToPlayer, enemyEyes.forward) <= fieldOfView)
        {
            // Offset for the starting position of the raycast
            Vector3 raycastStartPos = enemyEyes.position + (Vector3.up * 2); // Add Vector3.up to move the starting position up by one unit

            if (Physics.Raycast(raycastStartPos, directionToPlayer, out hit, chaseDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        return false;
    }
}
