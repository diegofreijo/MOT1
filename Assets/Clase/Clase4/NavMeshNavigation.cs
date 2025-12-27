using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshNavigationInputSystem : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /* 1️⃣ Agent configuration                                           */
    /* ------------------------------------------------------------------ */
    [Header("Agent Settings")]
    public float agentSpeed = 3.5f;
    public float agentAngularSpeed = 120f;
    public float stoppingDistance = 0.5f;

    /* ------------------------------------------------------------------ */
    /* 2️⃣ Patrol (optional)                                            */
    /* ------------------------------------------------------------------ */
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;      // Drag waypoints into the inspector
    public float waitTimeAtPoint = 2f;    // Seconds to pause at each point

    /* ------------------------------------------------------------------ */
    /* Internals – you can ignore them if you only want the public API   */
    /* ------------------------------------------------------------------ */
    private NavMeshAgent _agent;
    private Vector2 _moveInput;            // Updated by callbacks
    private int _currentPatrolIndex;
    private float _patrolTimer;

    /* ------------------------------------------------------------------ */
    /* 4️⃣ Awake: bind callbacks & initialise                                     */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        // Grab the agent & set base parameters
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = agentSpeed;
        _agent.angularSpeed = agentAngularSpeed;
        _agent.stoppingDistance = stoppingDistance;

        /* --- Patrol bootstrap ---------------------------------------------------- */
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            SetDestination(patrolPoints[0].position);
        }
    }

    /* ------------------------------------------------------------------ */
    /* 7️⃣ Update loop – process movement & patrol                      */
    /* ------------------------------------------------------------------ */
    private void Update()
    {
        HandleMoveInput();
        HandlePatrolLogic();
    }

    /* ------------------------------------------------------------------ */
    /* 8️⃣ Move input (polled)                                            */
    /* ------------------------------------------------------------------ */
    private void HandleMoveInput()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return; // No input

        // Convert input from camera space to world space
        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Vector3 worldDir = Camera.main.transform.TransformDirection(direction).normalized;

        // Aim 10 m ahead (you can tweak the multiplier)
        Vector3 targetWorld = transform.position + worldDir * 10f;

        // Sample the NavMesh to get a valid destination
        if (NavMesh.SamplePosition(targetWorld, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
        {
            SetDestination(navHit.position);
        }
    }

    /* ------------------------------------------------------------------ */
    /* 9️⃣ Patrol logic                                                   */
    /* ------------------------------------------------------------------ */
    private void HandlePatrolLogic()
    {

        Debug.Log($"{_agent.remainingDistance} <= {_agent.stoppingDistance}");

        // If we are almost at the current point, start waiting
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= waitTimeAtPoint)
            {
                _patrolTimer = 0f;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                SetDestination(patrolPoints[_currentPatrolIndex].position);
            }
        }
    }

    /* ------------------------------------------------------------------ */
    /* 10️⃣ Public API – handy for other scripts                        */
    /* ------------------------------------------------------------------ */
    public void SetDestination(Vector3 dest)
    {
        _agent.SetDestination(dest);
    }
}
