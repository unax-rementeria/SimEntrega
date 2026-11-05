using System;
using UnityEngine;

[Serializable]
public class VerletParticle 
{
    public Vector3 posicion;
    public Vector3 posicionPasada;
    public bool puntoFijo;

    public VerletParticle(Vector3 pos, bool puntoFijo)
    {
        posicion = pos;
        posicionPasada = pos;
        this.puntoFijo = puntoFijo;
    }

    public void Simulacion(Vector3 aceleracion, float fdt, float damping)
    {
        if (puntoFijo) return;
        Vector3 movimiento = (posicion - posicionPasada)*damping;
        Vector3 posicionNueva = posicion + movimiento + aceleracion * fdt * fdt;
        posicionPasada = posicion;
        posicion = posicionNueva;
    }
}
