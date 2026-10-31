using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 position;
    [SerializeField] private Player otherPlayer;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        position = transform.position;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Respawn"))
        {
            position = transform.position;
            otherPlayer.position = position;
            Destroy(other.gameObject);
        }

    }

    // Reaparece en el ultimo checkpoint. Se mueve el Rigidbody (y no solo el transform) porque
    // con interpolacion la fisica pisaba el cambio y el personaje seguia cayendo un rato
    public void Die()
    {
        // Suelta el gancho si estaba enganchado (las cuerdas y tirolinas lo sueltan solas al morir)
        GrapplingGun gancho = GetComponentInChildren<GrapplingGun>();
        if (gancho != null && gancho.isActiveAndEnabled)
        {
            gancho.Canceled();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = position;
        }
        transform.position = position;
    }

    private void OnEnable()
    {
        Eventos.eve.MuertePlayer.AddListener(Die);
    }
    private void OnDisable()
    {
        Eventos.eve.MuertePlayer.RemoveListener(Die);
    }
}
