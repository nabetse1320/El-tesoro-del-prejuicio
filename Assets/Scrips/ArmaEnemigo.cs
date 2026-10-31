using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArmaEnemigo : MonoBehaviour
{
    private Vector3 direccion;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform pivote;
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject armaSprite;
    [SerializeField] private float rotationSpeed;
    [Tooltip("Velocidad constante de la bala en unidades por segundo")]
    [SerializeField] private float velocidadBala;
    private bool proyectilDisparado = false;
    private Enemigo enemigoComp;

    private Enemigo EnemigoComp
    {
        get
        {
            if (enemigoComp == null && enemy != null) enemigoComp = enemy.GetComponent<Enemigo>();
            return enemigoComp;
        }
    }

    void Update()
    {
        
        Enemigo e = EnemigoComp;
        if (e == null || e.hits == null) return;
        foreach (var hit in e.hits)
        {
            if (hit.gameObject.CompareTag("Player")) 
            {
                if ((e.playerDetectado && !proyectilDisparado) || e.detectandoPlayer)
                {
                    direccion = hit.bounds.center - pivote.transform.position;
                    float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.AngleAxis(angulo, Vector3.forward);
                    if (angulo > 90 || angulo < -90)
                    {
                        pivote.transform.localScale = new Vector3(16.31f, -16.31f, 16.31f);
                    }
                    else
                    {
                        pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);

                    }
                    // Ajusta la velocidad de rotación según ea necesario
                    Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                    pivote.transform.rotation = new Quaternion(0, 0, newRotation.z, newRotation.w);
                }
                else if(e.detectandoPlayer|| !e.playerDetectado)
                {
                    if (enemy.transform.rotation.y==0)
                    {
                        Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                        pivote.transform.rotation = new Quaternion(0, 0, newRotation.z, newRotation.w);
                    }
                    else
                    {
                        Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                        pivote.transform.rotation = new Quaternion(0, 180, newRotation.z, newRotation.w);
                    }
                    // Aquí se elimina la rotación en los ejes X y Y
                                                                                                    //armaSprite.transform.localRotation = Quaternion.Euler(0, armaSprite.tra1nsform.rotation.y, armaSprite.transform.rotation.z);
                    pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);

                    //pivote.transform.rotation = Quaternion.Lerp(pivote.transform.rotation, enemy.transform.rotation, Time.deltaTime * rotationSpeed);
                    ////armaSprite.transform.localRotation = Quaternion.Euler(0, armaSprite.tra1nsform.rotation.y, armaSprite.transform.rotation.z);
                    //pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);
                }
            }
            
        }
    }

    // Armas activas, para que cada pirata dispare solo la suya
    private static readonly List<ArmaEnemigo> armas = new List<ArmaEnemigo>();

    public static void DispararDe(GameObject enemigo)
    {
        for (int i = 0; i < armas.Count; i++)
        {
            if (armas[i] != null && armas[i].enemy == enemigo)
            {
                armas[i].Shoot();
            }
        }
    }

    private void Shoot()
    {
        // Apunta al jugador que esta viendo; si no ve a ninguno, dispara hacia delante
        Enemigo e = EnemigoComp;
        if (e != null && e.hits != null)
        {
            foreach (var hit in e.hits)
            {
                if (hit != null && hit.CompareTag("Player"))
                {
                    direccion = hit.bounds.center - pivote.transform.position;
                    break;
                }
            }
        }
        if (direccion.sqrMagnitude < 0.0001f)
        {
            direccion = enemy.transform.right;
        }

        var balaa = Instantiate(bala, transform.position, pivote.transform.rotation);
        balaa.GetComponent<Proyectil>().SetDireccion(direccion,velocidadBala); // Pasa la dirección al proyectil
        balaa.transform.parent = null; // Desvincula el proyectil del objeto padre
        Destroy(balaa, 2);
    }

    private void OnEnable()
    {
        if (!armas.Contains(this)) armas.Add(this);
    }
    private void OnDisable()
    {
        armas.Remove(this);
    }
}
