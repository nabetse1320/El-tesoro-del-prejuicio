using UnityEngine;

// Enemigos que el gancho de Izzy puede atraer. El boss y el fantasma no lo implementan,
// asi que el gancho no los engancha.
public interface IEnganchable
{
    Rigidbody2D Cuerpo { get; }

    // False si esta muerto o no se puede mover
    bool PuedeEngancharse { get; }

    // True mientras lo arrastra el gancho y durante el aturdimiento posterior
    bool Aturdido { get; }

    // El gancho lo ha alcanzado: deja de moverse y de atacar mientras se le arrastra
    void Enganchar();

    // Termina el arrastre: queda aturdido durante 'tiempoAturdido' segundos
    void SoltarEnganche(float tiempoAturdido);
}
