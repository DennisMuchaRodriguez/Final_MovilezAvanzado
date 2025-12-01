using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DashController : NetworkBehaviour
{
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] public float dashPushForce = 10f;
    [SerializeField] private float pushDuration = 0.2f;

    [Header("Efectos de Partículas")]
    [SerializeField] private GameObject collisionParticlePrefab;
    [SerializeField] private float particleDestroyDelay = 1f;
    [Header("Dash Stats")]
   
    public float dashSpeedMultiplier = 1f; // Para Mega Dash
    private MovementController _movement;
    private PlayerInputHandler _input;
    private bool _canDash = true;
    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = true;
    private void Awake()
    {
        _movement = GetComponent<MovementController>();
        _input = GetComponent<PlayerInputHandler>();


        if (collisionParticlePrefab == null)
        {
            collisionParticlePrefab = Resources.Load<GameObject>("Prefabs/Particles/CollisionParticles");
        }
    }

    private void OnEnable() => _input.OnDashPressed += HandleDashPressed;
    private void OnDisable() => _input.OnDashPressed -= HandleDashPressed;

    private void HandleDashPressed(Vector2 direction)
    {
        if (_canDash && (!IsSpawned || IsOwner))
        {
            _movement.PerformDash(direction);
            StartCoroutine(DashCooldownCoroutine());
        }
    }

    private IEnumerator DashCooldownCoroutine()
    {
        _canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (IsSpawned && !IsOwner) return;
        if (!_movement.IsDashing) return;  
        if (!collision.gameObject.CompareTag("Player")) return;

        MovementController enemy = collision.gameObject.GetComponent<MovementController>();
        if (enemy != null && !enemy.IsBeingPushed)
        {
           
            Vector2 pushDir = (collision.transform.position - transform.position).normalized;

          
            Vector2 contactPoint = collision.contacts[0].point;

           
            if (IsSpawned) 
            {
                NetworkObject enemyNet = collision.gameObject.GetComponent<NetworkObject>();
                if (enemyNet != null)
                {
                    RequestPushEnemyServerRpc(enemyNet.NetworkObjectId, pushDir, contactPoint);
                }
            }
            else 
            {
             
                enemy.GetPushed(pushDir, dashPushForce, pushDuration);

            
                CreateCollisionParticles(contactPoint, pushDir);
            }
     
        }
     
        CameraShakeOnDashHit.Shake(0.7f);

        if (TryGetComponent<CameraShakeOnDashHit>(out var shaker))
        {
            shaker.DoShake();
        }
 
            CameraShakeEvents.TriggerDashHitShake();
        

    }

    [ServerRpc]
    private void RequestPushEnemyServerRpc(ulong enemyId, Vector2 direction, Vector2 contactPoint)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out NetworkObject enemyObj))
        {
            var enemyMove = enemyObj.GetComponent<MovementController>();
            if (enemyMove != null)
            {
                enemyMove.ApplyPushClientRpc(direction, dashPushForce, pushDuration);


            }
        }
    }


    private void CreateCollisionParticles(Vector2 position, Vector2 direction)
    {
        if (collisionParticlePrefab == null) return;

        GameObject particles = Instantiate(collisionParticlePrefab, position, Quaternion.identity);


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        particles.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

   
        Destroy(particles, particleDestroyDelay);
    }
    public void SetDashForceMultiplier(float multiplier, float duration)
    {
        StartCoroutine(ApplyDashForceMultiplier(multiplier, duration));
    }

    private IEnumerator ApplyDashForceMultiplier(float multiplier, float duration)
    {
        float originalForce = dashPushForce;
        dashPushForce *= multiplier;
        dashSpeedMultiplier = multiplier;

        Debug.Log($"Dash force aumentado a: {dashPushForce} (x{multiplier})");

        yield return new WaitForSeconds(duration);

        dashPushForce = originalForce;
        dashSpeedMultiplier = 1f;
        Debug.Log($"Dash force restaurado a: {dashPushForce}");
    }
}