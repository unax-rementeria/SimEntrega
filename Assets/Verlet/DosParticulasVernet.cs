using UnityEngine;

public class DosParticulasVernet : MonoBehaviour
{
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    VerletParticle particleA;
    VerletParticle particleB;
    [SerializeField] Vector3 gravedad = new Vector3 (0f, -9.8f, 0f);
    [SerializeField] float distanciaPuntos = 2f;

    void Start()
    {
        particleA = new VerletParticle(pointA.position, true);
        particleB = new VerletParticle(pointB.position, false);
    }

    private void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        particleA.Simulacion(gravedad, fdt, 1);
        particleB.Simulacion(gravedad, fdt, 1);
        ResolucionConstrains();
        pointA.position = particleA.posicion;
        pointB.position = particleB.posicion;
    }
    private void ResolucionConstrains()
    {
        Vector3 vAB = particleB.posicion - particleA.posicion;
        float distancia = vAB.magnitude;
        if (distancia == distanciaPuntos) return;
        float error = distancia - distanciaPuntos;
        Vector3 correccion = vAB.normalized * error;

        if (particleA.puntoFijo)
        {
            particleB.posicion -= correccion;
        }
        else if (particleB.puntoFijo) { 
            particleA.posicion += correccion;
        }
        else
        {
            particleB.posicion -= correccion * 0.5f;
            particleA.posicion += correccion * 0.5f;
        }
    }
}
