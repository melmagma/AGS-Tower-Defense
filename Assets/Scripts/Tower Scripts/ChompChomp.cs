using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BaseTower))]
public class ChompChomp : MonoBehaviour
{
    [Header("References")]
    public Transform meshRoot; //the child mesh that lunges, so that the whole tower doesnt move and change its range radius.

    [Header("Lunge")]
    public float lungeSpeed = 1f;
    public float returnSpeed = 1f; // lunge speed is time taken to get to enemy, and return back to our position
    public float biteReach = 0.4f; // how close the mesh gets before chompchomp bites
    public float maxLungeTime = 0.6f; // safety timeout in case the enemy outruns the lunge
    public bool keepMeshHeight = true; // stops the mesh from diving into / flying above the path
    public float turnSpeed = 720f; // degrees per second, mesh faces the enemy while lunging
    public float returnDuration = 0.5f;

    [Header("Bite")]
    public float biteHoldTime = 0.05f;  // tiny pause on impact so the bite reads visually when we implement anim

    [Header("Events")]
    public UnityEvent onBite; // hook up SFX / VFX / animator trigger here

    private BaseTower tower;
    private Vector3 homeLocalPos;
    private Quaternion homeLocalRot;
    private float cooldown;
    private bool isAttacking;

    public float facingOffset = 180f; //chompchomp is currently facing the other way, so im adding an offset to coutner dat

    void Awake()
    {
        tower = GetComponent<BaseTower>();

        if (meshRoot == null)
        {
            Debug.LogWarning($"{name}: BiteAttack has no meshRoot assigned.", this);
            enabled = false;
            return;
        }

        homeLocalPos = meshRoot.localPosition;
        homeLocalRot = meshRoot.localRotation;
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (isAttacking || cooldown > 0f || tower.fireRate <= 0f) return;

        EnemyController target = tower.GetTarget(); // respects the first / last targeting
        if (target == null) return;

        cooldown = 1f / tower.fireRate; // fireRate = hits per second
        StartCoroutine(BiteRoutine(target));
    }

    private IEnumerator BiteRoutine(EnemyController target)
    {
        isAttacking = true;

        // 1. LUNGE: chase the enemy's current position (it keeps moving along the path)
        float timer = 0f;
        bool reached = false;

        while (timer < maxLungeTime)
        {
            if (target == null) break; // enemy died or reached the end mid-lunge

            Vector3 goal = target.transform.position;
            if (keepMeshHeight) goal.y = meshRoot.position.y;

            meshRoot.position = Vector3.MoveTowards(meshRoot.position, goal, lungeSpeed * Time.deltaTime);
            FaceTowards(goal);

            Vector3 flatDelta = goal - meshRoot.position;
            if (flatDelta.magnitude <= biteReach)
            {
                reached = true;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 2. BITE: only deals damage if the mesh actually caught the enemy
        if (reached && target != null)
        {
            DealDamage(target);
            onBite?.Invoke();
            yield return new WaitForSeconds(biteHoldTime);
        }

        // 3. RETURN: slide back to the resting spot (local space, so the root never moves)
        Vector3 startPos = meshRoot.localPosition;
        Quaternion startRot = meshRoot.localRotation;
        float duration = Mathf.Max(returnDuration, 0.01f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t); // gentle start and finish instead of a linear snap
            meshRoot.localPosition = Vector3.Lerp(startPos, homeLocalPos, eased);
            meshRoot.localRotation = Quaternion.Slerp(startRot, homeLocalRot, eased);
            yield return null;
        }

        meshRoot.localPosition = homeLocalPos;
        meshRoot.localRotation = homeLocalRot;
        isAttacking = false;
    }

    private void DealDamage(EnemyController primary)
    {
        int dmg = Mathf.RoundToInt(tower.damage);

        if (tower.hitType == BaseTower.HitType.Single)
        {
            primary.TakeDamage(dmg);
            return;
        }

        // Multi: everything inside the splash radius around the bite point
        Collider[] hits = Physics.OverlapSphere(meshRoot.position, tower.splashRadius, tower.whatIsEnemy);
        HashSet<EnemyController> alreadyHit = new HashSet<EnemyController>(); // one enemy can have several colliders

        foreach (Collider col in hits)
        {
            EnemyController e = col.GetComponentInParent<EnemyController>();
            if (e != null && alreadyHit.Add(e)) e.TakeDamage(dmg);
            Debug.Log("took damage" + dmg);
        }
    }

    private void FaceTowards(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - meshRoot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, facingOffset, 0f);
        meshRoot.rotation = Quaternion.RotateTowards(meshRoot.rotation, look, turnSpeed * Time.deltaTime);
    }

    void OnDisable()
    {
        // if the tower is sold / path script swapped mid-lunge, don't leave the mesh stranded
        if (meshRoot != null)
        {
            meshRoot.localPosition = homeLocalPos;
            meshRoot.localRotation = homeLocalRot;
        }
        isAttacking = false;
    }
}
