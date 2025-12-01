using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class MovementController : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Configuración de Dash")]
    [SerializeField] public float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    public float dashSpeedMultiplier = 1f;

    [Header("Efectos Visuales")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private bool enableTrail = true;
    [Header("Dash Settings")]
    public float dashForce = 10f; // Hacerlo público
    public Color trailColor = Color.white; // Hacerlo público
    public GameObject trailObject; 
    private Rigidbody2D _rb;
    private PlayerInputHandler _input;

    private Vector2 _moveInput;
    private Vector2 _dashDirection;
    private Vector2 _pushDirection;
    private float _pushSpeed;

    private bool _isDashing = false;
    private bool _isBeingPushed = false;

    public bool IsDashing => _isDashing;
    public bool IsBeingPushed => _isBeingPushed;
    [Header("Shield Immunity")]
    private bool isShielded = false;
    private float shieldTimer = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInputHandler>();
        _rb.gravityScale = 0;

        SetupTrailRenderer();
    }
    private void Update()
    {
        if (isShielded)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                isShielded = false;
                Debug.Log("Escudo DESACTIVADO");
            }
        }
        if (_isDashing && trailRenderer != null && !trailRenderer.emitting)
        {
            Debug.LogWarning("Trail debería estar activo pero no lo está!");
            EnableTrail();
        }
    }
    public void ActivateShield(float duration)
    {
        if (!isShielded)
        {
            isShielded = true;
            shieldTimer = duration;
            Debug.Log($"Escudo ACTIVADO - Inmune por {duration}s");
        }
    }

    private void SetupTrailRenderer()
    {
        // Buscar TrailRenderer si no está asignado
        if (trailRenderer == null)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        // Si aún es null, intentar crearlo automáticamente
        if (trailRenderer == null && enableTrail)
        {
            CreateTrailRenderer();
        }

        // Configurar trail
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.enabled = enableTrail;
        }
    }

    private void CreateTrailRenderer()
    {
        GameObject trailObj = new GameObject("DashTrail");
        trailObj.transform.SetParent(transform);
        trailObj.transform.localPosition = Vector3.zero;

        trailRenderer = trailObj.AddComponent<TrailRenderer>();

        // Configuración básica del trail
        trailRenderer.time = 0.3f;
        trailRenderer.startWidth = 0.4f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.minVertexDistance = 0.1f;

        // Material simple
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Gradiente de color
        Gradient gradient = new Gradient();
        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0f),
            new GradientColorKey(Color.gray, 1f)
        };
        gradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        };
        trailRenderer.colorGradient = gradient;
    }

    private void OnEnable() => _input.OnMoveInput += HandleMoveInput;
    private void OnDisable() => _input.OnMoveInput -= HandleMoveInput;

    private void HandleMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    private void FixedUpdate()
    {
        // En modo online, solo el owner controla el movimiento
        // En modo local, todos los jugadores se controlan
        if (IsSpawned && !IsOwner) return;

        Move();
    }

    private void Move()
    {
        Vector2 finalVelocity = Vector2.zero;

        if (_isDashing) finalVelocity = _dashDirection * dashSpeed;
        else if (_isBeingPushed) finalVelocity = _pushDirection * _pushSpeed;
        else finalVelocity = _moveInput * moveSpeed;

        _rb.linearVelocity = finalVelocity;
    }

    public void PerformDash(Vector2 direction)
    {
        if (_isDashing || _isBeingPushed) return;
        _dashDirection = direction.normalized;
        StartCoroutine(DashCoroutine());
    }

    [ClientRpc]
    public void ApplyPushClientRpc(Vector2 direction, float force, float duration)
    {
        if (IsOwner) GetPushed(direction, force, duration);
    }

    public void GetPushed(Vector2 direction, float force, float duration)
    {
        if (isShielded)
        {
            Debug.Log($"¡ESCUDO BLOQUEÓ empujón! Fuerza: {force}");
            return; // No empujar si tiene escudo
        }

        if (_isBeingPushed) return;

        if (_isDashing)
        {
            StopAllCoroutines();
            _isDashing = false;
            DisableTrail();
        }

        _pushDirection = direction;
        _pushSpeed = force;
        StartCoroutine(PushedCoroutine(duration));
    }

    private IEnumerator DashCoroutine()
    {
        _isDashing = true;
        EnableTrail();

        // Usar velocidad modificada
        float currentDashSpeed = dashSpeed * dashSpeedMultiplier;

        float timer = 0f;
        while (timer < dashDuration)
        {
            _rb.linearVelocity = _dashDirection * currentDashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
        DisableTrail();
    }
    public void SetDashSpeedMultiplier(float multiplier, float duration)
    {
        StartCoroutine(ApplyDashSpeedMultiplier(multiplier, duration));
    }

    private IEnumerator ApplyDashSpeedMultiplier(float multiplier, float duration)
    {
        float originalSpeed = dashSpeed;
        dashSpeed *= multiplier;
        dashSpeedMultiplier = multiplier;

        Debug.Log($"Dash speed aumentado a: {dashSpeed} (x{multiplier})");

        yield return new WaitForSeconds(duration);

        dashSpeed = originalSpeed;
        dashSpeedMultiplier = 1f;
        Debug.Log($"Dash speed restaurado a: {dashSpeed}");
    }
    private IEnumerator PushedCoroutine(float duration)
    {
        _isBeingPushed = true;
        yield return new WaitForSeconds(duration);
        _isBeingPushed = false;
    }

    private void EnableTrail()
    {
        if (trailRenderer != null && enableTrail)
        {
            trailRenderer.emitting = true;
            trailRenderer.enabled = true;
        }
    }

    private void DisableTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
           
        }
    }

    public void ForceTrailUpdate()
    {
        SetupTrailRenderer();
    }
}