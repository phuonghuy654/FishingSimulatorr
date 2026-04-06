using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    [Tooltip("Thời gian chờ (giây) trước khi có thể tương tác lại")]
    public float interactionCooldown = 1.0f;

    [Header("References")]
    [Tooltip("Kéo GameObject chứa script InGameUIManager vào đây")]
    public InGameUIManager uiManager;
    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;

    
    private AudioManager audioManager;
    

    private bool playerInRange = false;
    private Coroutine patrolCoroutine;
    private Coroutine interactionCoroutine;

    private Vector3 patrolCenter;
    private bool isInteractionOnCooldown = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        
        audioManager = AudioManager.instance;
        

        patrolCenter = transform.position;

        if (uiManager == null)
        {
            Debug.LogError("NPCController thiếu tham chiếu đến InGameUIManager!");
        }

        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (interactionCoroutine != null && uiManager.GetCurrentState() == InGameUIManager.InGameState.PanelOpen)
            {
                StopCoroutine(interactionCoroutine);
                interactionCoroutine = null;
                animator.SetBool("isTalking", false);
                ResumePatrolAfterInteraction();
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;

            StopPatrolForInteraction();

            if (uiManager != null && uiManager.GetCurrentState() == InGameUIManager.InGameState.Idle && interactionCoroutine == null && !isInteractionOnCooldown)
            {
                interactionCoroutine = StartCoroutine(LookAtAndOpenPanel());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionCoroutine != null)
            {
                animator.SetBool("isTalking", false);
                uiManager.HideAllPanels();
                interactionCoroutine = null;
            }

            ResumePatrolAfterInteraction();

            StartCoroutine(InteractionCooldownRoutine());
        }
    }


    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!playerInRange && interactionCoroutine == null)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }

                Vector3 randomDestination = GetRandomNavMeshPoint();
                agent.SetDestination(randomDestination);
                animator.SetBool("isWalking", true);

                while (agent.pathPending || agent.remainingDistance > 0.5f)
                {
                    if (playerInRange)
                    {
                        StopPatrolForInteraction();
                        goto ContinuePatrolLoop;
                    }
                    yield return null;
                }

                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                animator.SetBool("isWalking", false);
                yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            ContinuePatrolLoop:;
            }
            else
            {
                yield return null;
            }
        }
    }


    private Vector3 GetRandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + patrolCenter;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas);
        return hit.position;
    }

    private void StopPatrolForInteraction()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        else
        {
            agent.isStopped = true;
        }
        animator.SetBool("isWalking", false);
    }

    private void ResumePatrolAfterInteraction()
    {
        if (!playerInRange && interactionCoroutine == null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }

    private IEnumerator InteractionCooldownRoutine()
    {
        isInteractionOnCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        isInteractionOnCooldown = false;
    }

    private IEnumerator LookAtAndOpenPanel()
    {
        bool panelOpened = false;

        while (playerInRange && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (!panelOpened && Quaternion.Angle(transform.rotation, lookRotation) < 5f)
            {
                if (playerInRange && uiManager != null && uiManager.GetCurrentState() == InGameUIManager.InGameState.Idle)
                {
                    
                    audioManager?.PlaySFX("NPC");
                    

                    animator.SetBool("isTalking", true);
                    Debug.Log("NPC triggering UI Panel Toggle!");
                    uiManager.TogglePanels();
                    panelOpened = true;
                }
                else
                {
                    break;
                }
            }

            yield return null;
        }

        animator.SetBool("isTalking", false);

        if (playerInRange && panelOpened)
        {
        }
        interactionCoroutine = null;
        ResumePatrolAfterInteraction();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if (!Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        }
    }
}