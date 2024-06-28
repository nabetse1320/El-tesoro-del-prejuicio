using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 position;
    private bool cambiarPosicion;
    [SerializeField] private Player otherPlayer;

    private void Start()
    {
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

    private void Update()
    {
        if (cambiarPosicion)
        {
            this.gameObject.transform.position = position;
            cambiarPosicion = false;
        }
    }
    public void Die()
    {
        cambiarPosicion = true;
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
