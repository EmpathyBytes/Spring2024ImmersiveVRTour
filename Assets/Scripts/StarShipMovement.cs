using UnityEngine;
using UnityEngine.AI;

public class StarShipMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform robotVisual;

    [Header("Blocked Detection")]
    public float minDesiredSpeed = 0.2f;
    public float maxActualSpeedWhenBlocked = 0.05f;
    public float blockedTimeToTrigger = 1.2f;
    public float forwardCheckDistance = 0.8f;
    public LayerMask obstacleMask;
    public float blockedCooldown = 1.0f;

    [Header("Roaming")]
    public float roamRange = 10f;
    public float pauseAtPoint = 1.0f;

    [Header("Visual Turning")]
    public float visualTurnSpeed = 180f;
    public float facingThreshold = 5f;
    public float scanDuration = 0.7f;
    public float scanAngle = 35f;

    [Header("Escape Reroute")]
    public float escapeSampleRadius = 0.6f;
    public int escapeSamples = 12;

    float blockedTimer = 0f;
    float blockedCooldownTimer = 0f;
    bool isHandlingBlock = false;
    bool isPausingAtPoint = false;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        agent.enabled = false;

        NavMeshHit hit;
        bool found = NavMesh.SamplePosition(transform.position, out hit, 20f, NavMesh.AllAreas);
        if (found)
            transform.position = hit.position;

        agent.enabled = true;

        // Let the agent control the ROOT orientation on slopes.
        agent.updateRotation = true;
        agent.updateUpAxis = true;

        PickRandomDestination();
    }

    void Update()
    {
        if (blockedCooldownTimer > 0f)
            blockedCooldownTimer -= Time.deltaTime;

        UpdateVisualFacing();

        if (isHandlingBlock || isPausingAtPoint)
            return;

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            StartCoroutine(PauseAndPickNextDestination());
            return;
        }

        if (blockedCooldownTimer <= 0f && IsBlocked())
        {
            StartCoroutine(HandleBlocked());
        }
    }

    void UpdateVisualFacing()
    {
        if (robotVisual == null)
            return;

        if (isHandlingBlock || isPausingAtPoint)
            return;

        Vector3 moveDir = agent.desiredVelocity;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
        robotVisual.rotation = Quaternion.RotateTowards(
            robotVisual.rotation,
            targetRot,
            visualTurnSpeed * Time.deltaTime
        );
    }

    System.Collections.IEnumerator PauseAndPickNextDestination()
    {
        isPausingAtPoint = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(pauseAtPoint);

        Vector3 nextPoint = RandomNavMeshPoint(transform.position, roamRange);

        if (robotVisual != null)
        {
            Vector3 toNext = nextPoint - transform.position;
            toNext.y = 0f;

            if (toNext.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toNext.normalized);

                while (Quaternion.Angle(robotVisual.rotation, targetRotation) > facingThreshold)
                {
                    robotVisual.rotation = Quaternion.RotateTowards(
                        robotVisual.rotation,
                        targetRotation,
                        visualTurnSpeed * Time.deltaTime
                    );
                    yield return null;
                }
            }
        }

        agent.SetDestination(nextPoint);
        agent.isStopped = false;
        isPausingAtPoint = false;
    }

    void PickRandomDestination()
    {
        Vector3 randomPoint = RandomNavMeshPoint(transform.position, roamRange);
        agent.SetDestination(randomPoint);
    }

    Vector3 RandomNavMeshPoint(Vector3 center, float range)
    {
        float minDistanceFromEdge = 0.4f;

        for (int i = 0; i < 40; i++)
        {
            Vector2 circle = Random.insideUnitCircle * range;
            Vector3 randomPos = new Vector3(center.x + circle.x, center.y, center.z + circle.y);

            if (!NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                continue;

            NavMeshHit edgeHit;
            if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
            {
                if (edgeHit.distance < minDistanceFromEdge)
                    continue;
            }

            if (Vector3.Distance(center, hit.position) < 1.5f)
                continue;

            return hit.position;
        }

        return center;
    }

    bool IsBlocked()
    {
        bool wantsToMove = agent.desiredVelocity.magnitude > minDesiredSpeed;
        bool notMoving = agent.velocity.magnitude < maxActualSpeedWhenBlocked;

        Vector3 forwardDir = agent.desiredVelocity;
        forwardDir.y = 0f;

        if (forwardDir.sqrMagnitude < 0.001f)
            forwardDir = transform.forward;

        forwardDir.Normalize();

        bool forwardHit = Physics.Raycast(
            transform.position + Vector3.up * 0.2f,
            forwardDir,
            forwardCheckDistance,
            obstacleMask
        );

        if (wantsToMove && notMoving && forwardHit)
            blockedTimer += Time.deltaTime;
        else
            blockedTimer = Mathf.Max(0f, blockedTimer - Time.deltaTime * 2f);

        return blockedTimer >= blockedTimeToTrigger;
    }

    System.Collections.IEnumerator HandleBlocked()
    {
        isHandlingBlock = true;
        blockedTimer = 0f;

        agent.isStopped = true;

        if (robotVisual != null)
        {
            Quaternion baseRot = robotVisual.rotation;
            Quaternion leftRot = baseRot * Quaternion.Euler(0f, -scanAngle, 0f);
            Quaternion rightRot = baseRot * Quaternion.Euler(0f, scanAngle, 0f);

            float halfScan = scanDuration * 0.5f;
            float timer = 0f;

            while (timer < halfScan)
            {
                robotVisual.rotation = Quaternion.RotateTowards(
                    robotVisual.rotation,
                    leftRot,
                    visualTurnSpeed * Time.deltaTime
                );
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            while (timer < scanDuration)
            {
                robotVisual.rotation = Quaternion.RotateTowards(
                    robotVisual.rotation,
                    rightRot,
                    visualTurnSpeed * Time.deltaTime
                );
                timer += Time.deltaTime;
                yield return null;
            }

            while (Quaternion.Angle(robotVisual.rotation, baseRot) > facingThreshold)
            {
                robotVisual.rotation = Quaternion.RotateTowards(
                    robotVisual.rotation,
                    baseRot,
                    visualTurnSpeed * Time.deltaTime
                );
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(scanDuration);
        }

        Vector3 escape;
        bool found = TryFindEscapePoint(out escape);

        agent.isStopped = false;

        if (found)
        {
            agent.SetDestination(escape);

            float timeout = 1.5f;
            float timer = 0f;

            while (timer < timeout && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f))
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (agent.velocity.magnitude < 0.05f)
            {
                PickRandomDestination();
            }
        }
        else
        {
            PickRandomDestination();
        }

        blockedCooldownTimer = blockedCooldown;
        isHandlingBlock = false;
    }

    bool TryFindEscapePoint(out Vector3 escape)
    {
        escape = transform.position;
        float bestScore = float.NegativeInfinity;

        Vector3 preferredDirection = agent.desiredVelocity;
        preferredDirection.y = 0f;

        if (preferredDirection.sqrMagnitude < 0.001f)
            preferredDirection = transform.forward;

        preferredDirection.Normalize();

        float minEscapeDistance = 0.25f;
        float maxEscapeDistance = 0.75f;
        float minDistanceFromEdge = 0.25f;

        for (int i = 0; i < escapeSamples; i++)
        {
            float angle = Mathf.Lerp(-75f, 75f, i / (float)(escapeSamples - 1));
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * preferredDirection;

            Vector3 candidate = transform.position + dir * escapeSampleRadius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.4f, NavMesh.AllAreas))
                continue;

            Vector3 flatOffset = hit.position - transform.position;
            flatOffset.y = 0f;

            float dist = flatOffset.magnitude;

            if (dist < minEscapeDistance || dist > maxEscapeDistance)
                continue;

            NavMeshHit edgeHit;
            if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
            {
                if (edgeHit.distance < minDistanceFromEdge)
                    continue;
            }

            Vector3 candidateDir = flatOffset.normalized;

            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, candidateDir, dist, obstacleMask))
                continue;

            NavMeshHit navRayHit;
            if (NavMesh.Raycast(transform.position, hit.position, out navRayHit, NavMesh.AllAreas))
                continue;

            NavMeshPath testPath = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, testPath))
                continue;

            if (testPath.status != NavMeshPathStatus.PathComplete)
                continue;

            if (testPath.corners.Length > 1)
            {
                float directDist = Vector3.Distance(transform.position, hit.position);
                float pathDist = 0f;

                for (int c = 1; c < testPath.corners.Length; c++)
                    pathDist += Vector3.Distance(testPath.corners[c - 1], testPath.corners[c]);

                if (pathDist > directDist * 1.5f)
                    continue;
            }

            float alignmentScore = Vector3.Dot(preferredDirection, candidateDir);
            float distancePenalty = dist * 0.25f;
            float score = alignmentScore - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                escape = hit.position;
            }
        }

        return bestScore > -0.5f;
    }
}