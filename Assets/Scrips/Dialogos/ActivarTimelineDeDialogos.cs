using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActivarTimelineDeDialogos : MonoBehaviour
{
    private bool activado;
    private PlayableDirector director;
    [SerializeField] private Animator animatorMapaUOtro;
    // Start is called before the first frame update
    void Start()
    {
        activado = false;
        director = GetComponent<PlayableDirector>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //DesactivarAnimacionMapa();
            activado = true;
            director.Play();
        }
        
    }

    public void QuitarTimeLine()
    {
        this.gameObject.SetActive(false);
    }
    public void DesactivarAnimacionMapa()
    {
        if(animatorMapaUOtro != null)
        {
            animatorMapaUOtro.SetBool("OnMovement", false);
        }
    }
    public void ActivarAnimacionMapa()
    {
        if (animatorMapaUOtro != null)
        {
            animatorMapaUOtro.SetBool("OnMovement", true);
        }
    }
}
