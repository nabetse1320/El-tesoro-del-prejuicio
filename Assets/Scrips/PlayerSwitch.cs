using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerSwitch : MonoBehaviour
{
    [HideInInspector] public PlayerController player1;
    [HideInInspector] public PlayerController2 player2;
    [HideInInspector] public bool player1Active = true;
    [SerializeField] private GameObject grappelGun;
    [SerializeField] private SpriteRenderer grappelGunSprite;
    private Mapa inputs;
    
    [SerializeField] public Transform Player1;
    [SerializeField] public Transform Player2;
    [SerializeField] public CinemachineVirtualCamera virtualCamera;

    public GameObject p1;
    public GameObject p2;
    public Material skin;

    [HideInInspector] public bool isGrounded;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 1f;

    [SerializeField] private GameObject caraLiam;
    [SerializeField] private GameObject caraLizzy;
    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipCambioPersonajeHombre;
    [SerializeField] private AudioClip clipCambioPersonajeMujer;

    private void Awake()
    {
        inputs = new Mapa();
        inputs.Enable();

    }
    private void Start()
    {
        player1 = p1.GetComponent<PlayerController>();
        player2 = p2.GetComponent<PlayerController2>();
        p1.GetComponent<SpriteRenderer>().sortingOrder = 1;
        player2.tag = "Untagged";
        Material materialP2 = p2.GetComponent<Renderer>().material;
        Color colorP2 = materialP2.color;
        colorP2.a = 0f;
        materialP2.color = colorP2;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        isGrounded = CheckGrounded();

        // Mantener el desplazamiento constante entre los personajes
        if (player1Active)
        {

            Vector3 targetPosition = Player1.position;
            Player2.position = targetPosition;
        }
        else
        {
            Vector3 targetPosition = Player2.position;
            Player1.position = targetPosition;
        }

    }

    private void OnEnable()
    {
        //inputs.Player.Switch.started += OnSwitchStarted;
    }
    private void OnDisable()
    {
        //inputs.Player.Switch.started -= OnSwitchStarted;
    }

    public void OnSwitchStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (((player1.isGrounded) || (player2.isGrounded)))
            {
                SwitchPlayer();
            }
        }
        
    }

    private bool CheckGrounded()
    {
        Vector2 raycastOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, raycastDistance, groundLayer);
        return (hit.collider != null);
    }

    public void SwitchPlayer()
    {
        if (player1Active)
        {
            virtualCamera.Follow = Player2; // Cambiar el objetivo de la cámara al personaje 2

        }
        else
        {
            virtualCamera.Follow = Player1; // Cambiar el objetivo de la cámara al personaje 1
        }
         

        if (player1Active)
        {
            //grappelGun.GetComponent<GrapplingGun>().grappleRope.EnableReverse();
            
            grappelGun.GetComponent<GrapplingGun>().Canceled();
            grappelGun.GetComponent<GrapplingGun>().inCoolDown = false;
            grappelGun.SetActive(false);
            player2.enabled = true;
            //p2.GetComponent<CapsuleCollider2D>().enabled = true;
            player2.isActive = true;
            caraLiam.SetActive(true);
            player2.tag = "Player";
            p2.GetComponent<SpriteRenderer>().sortingOrder = 1;
            audioSource.clip = clipCambioPersonajeHombre;
            audioSource.Play();

            


            Material materialP1 = p1.GetComponent<Renderer>().material;
            Color colorP1 = materialP1.color;
            colorP1.a = 0f;
            materialP1.color = colorP1;
            grappelGunSprite.material = materialP1;

            Material materialP2 = p2.GetComponent<Renderer>().material;
            Color colorP2 = materialP2.color;
            colorP2.a = 1.0f;
            materialP2.color = colorP2;
            player1Active = false;
            player1.isActive = false;
            player1.tag = "Untagged";
            p1.GetComponent<SpriteRenderer>().sortingOrder = 0;
            //p1.GetComponent<CapsuleCollider2D>().enabled = false;
        }
        else
        {
            grappelGun.SetActive(true);
            player1.enabled = true;
            //p1.GetComponent<CapsuleCollider2D>().enabled = true;
            player1Active = true;
            player1.isActive = true;
            caraLiam.SetActive(false);
            player1.tag = "Player";
            p1.GetComponent<SpriteRenderer>().sortingOrder = 1;
            audioSource.clip = clipCambioPersonajeMujer;
            audioSource.Play();




            Material materialP2 = p2.GetComponent<Renderer>().material;
            Color colorP2 = materialP2.color;
            colorP2.a = 0f;
            materialP2.color = colorP2;

            Material materialP1 = p1.GetComponent<Renderer>().material;
            Color colorP1 = materialP1.color;
            colorP1.a = 1.0f;
            materialP1.color = colorP1;


            // Teletransportar al personaje inactivo (pj1) al lugar correcto con el desfase
            Vector3 targetPosition = Player2.position;
            Player1.position = targetPosition;
            Player1.rotation = Player2.rotation;
            player2.isActive = false;
            player2.tag = "Untagged";
            p2.GetComponent<SpriteRenderer>().sortingOrder = 0;
            //p2.GetComponent<CapsuleCollider2D>().enabled = false;
        }

    }
}
