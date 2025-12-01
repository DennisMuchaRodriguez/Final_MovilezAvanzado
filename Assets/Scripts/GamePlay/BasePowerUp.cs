using UnityEngine;
using UnityEngine.Events;

public class BasePowerUp : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpEvent : UnityEvent<PlayerLifeManager> { }

    [Header("Configuración")]
    [SerializeField] protected PowerUpManager.PowerUpType powerUpType;
    [SerializeField] protected float duration = 5f;

    [Header("Visuales")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected GameObject pickupEffect;

    [Header("Animación")]
    [SerializeField] protected float floatSpeed = 1f;
    [SerializeField] protected float floatHeight = 0.5f;
    [SerializeField] protected float rotationSpeed = 50f;

    [Header("Sonido")]
    [SerializeField] protected AudioClip pickupSound;

    public UnityEvent<PlayerLifeManager> OnApplied = new PowerUpEvent();
    public UnityEvent OnCollected = new UnityEvent();

    protected Vector3 startPosition;
    protected bool isCollected = false;

    protected virtual void Start()
    {
        startPosition = transform.position;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        if (isCollected) return;

        // Animación flotante
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotación
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        PlayerLifeManager player = other.GetComponent<PlayerLifeManager>();
        if (player != null)
        {
            Collect(player);
        }
    }

    protected virtual void Collect(PlayerLifeManager player)
    {
        isCollected = true;

        // Efecto visual
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Sonido
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Aplicar efecto al jugador
        ApplyEffect(player);

        // Notificar recolección
        OnCollected?.Invoke();

        // Ocultar
        spriteRenderer.enabled = false;

        // Destruir después de un delay
        Destroy(gameObject, 2f);
    }

    protected virtual void ApplyEffect(PlayerLifeManager player)
    {
        Debug.Log($"Power-up {powerUpType} aplicado a {player.GetPlayerName()}");
        OnApplied?.Invoke(player);
    }

    public void SetType(PowerUpManager.PowerUpType type)
    {
        powerUpType = type;
        UpdateVisuals();
    }

    protected virtual void UpdateVisuals()
    {
        // Cambiar color según tipo
        switch (powerUpType)
        {
            case PowerUpManager.PowerUpType.MegaDash:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Naranja
                break;
            case PowerUpManager.PowerUpType.Shield:
                spriteRenderer.color = Color.blue;
                break;
            case PowerUpManager.PowerUpType.Teleport:
                spriteRenderer.color = Color.magenta;
                break;
            case PowerUpManager.PowerUpType.Shockwave:
                spriteRenderer.color = Color.yellow;
                break;
        }
    }
}