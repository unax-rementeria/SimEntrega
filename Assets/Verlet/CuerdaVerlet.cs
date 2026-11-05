using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]

public class CuerdaVerlet : MonoBehaviour
{
    [Header("Cuerda")]
    [Min(2)]
    [SerializeField] int numPuntos = 10;
    [SerializeField] float distanciaEntrePuntos = 0.2f;

    [Header("Simulacion")]
    [SerializeField] Vector3 gravedad = new Vector3(0f, -9.8f, 0f);
    [Range(1, 500)]
    [SerializeField] int numIteraciones = 10; // Más iteraciones = cuerda más rígida (menos elástica) pero más coste
    [SerializeField] float damping = 0.95f;   // Pérdida de velocidad por frame, simula rozamiento
    [SerializeField] float radioCast = 0.2f;  // Grosor virtual de la cuerda para detectar colisiones
    [SerializeField] LayerMask mascaraColisiones;

    [SerializeField] Transform esquina; // Opcional: si se asigna, el último punto de la cuerda queda anclado aquí


    List<VerletParticle> listaParticulas;

    LineRenderer lineRenderer;  
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numPuntos;
        listaParticulas = new List<VerletParticle>();
        
        for (int i = 0; i < numPuntos; i++) {
            // La cuerda empieza recta, extendida a lo largo del eje X
            Vector3 distanciaAInicio = new Vector3(i * distanciaEntrePuntos, 0, 0);
            listaParticulas.Add(new VerletParticle( transform.position + distanciaAInicio, false));
        }
        listaParticulas[0].puntoFijo = true;
        // El último punto solo se fija si hay una esquina a la que engancharlo
        if (esquina != null) listaParticulas[listaParticulas.Count - 1].puntoFijo = true;
        

    }
    private void Update()
    {
        // Solo se dibuja aquí; la simulación va en FixedUpdate para que sea estable
        DibujarLista();
    }

    private void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;

        // Los extremos fijos siguen a sus transforms (por si se mueven), antes de simular
        listaParticulas[0].posicion = transform.position;
        if(esquina != null) listaParticulas[listaParticulas.Count -1].posicion = esquina.position;

        //Simular las particulas
        for (int i = 0; i < numPuntos; i++) {
            listaParticulas[i].Simulacion(gravedad, fdt, damping);
        }

        //Aplicar constrains
        // Se repite varias veces porque corregir una pareja descoloca a las vecinas;
        // iterando, el conjunto converge hacia una cuerda con la longitud correcta
        for (int iter = 0; iter < numIteraciones; iter++)
        {
            // Empieza en 1 porque la partícula 0 siempre está fija y no se corrige
            for (int i = 1; i < numPuntos; i++)
            {
                ResolucionConstrainsDistance(listaParticulas[i - 1], listaParticulas[i]);
                ResolucionConstrainFloor(listaParticulas[i]);
                ResolucionColisiones(listaParticulas[i]);
            }
        }

    }




    private void ResolucionConstrainsDistance(VerletParticle particleA, VerletParticle particleB)
    {
        Vector3 vAB = particleB.posicion - particleA.posicion;
        float distancia = vAB.magnitude;
        if (distancia == distanciaEntrePuntos) return;

        // error > 0: los puntos están demasiado separados; error < 0: demasiado juntos
        float error = distancia - distanciaEntrePuntos;
        Vector3 correccion = vAB.normalized * error;

        // Si un punto es fijo, el otro absorbe toda la corrección;
        // si ninguno lo es, cada uno se mueve la mitad
        if (particleA.puntoFijo)
        {
            particleB.posicion -= correccion;
        }
        else if (particleB.puntoFijo)
        {
            particleA.posicion += correccion;
        }
        else
        {
            particleB.posicion -= correccion * 0.5f;
            particleA.posicion += correccion * 0.5f;
        }
    }
    private void ResolucionConstrainFloor(VerletParticle particleA)
    {
        // Suelo en y = 0
        if(particleA.posicion.y < 0)
        {
            particleA.posicion.y = 0;
        }
    }

    private void ResolucionColisiones(VerletParticle particleA) {
        if (particleA.puntoFijo) return;

        // En Verlet la velocidad es implícita: es la diferencia entre posición actual y pasada
        Vector3 movimiento = particleA.posicion - particleA.posicionPasada;
        float distancia = movimiento.magnitude;

        // Si apenas se ha movido, no hace falta lanzar el cast (y evita normalizar un vector casi nulo)
        if (distancia < 0.0001f)
        {
            return;
        }

        // Se lanza una esfera desde donde estaba hasta donde está ahora para detectar
        // choques incluso si la partícula ha atravesado el collider en un solo paso
        if (Physics.SphereCast(particleA.posicionPasada, radioCast, movimiento.normalized, out RaycastHit hitInfo, distancia, mascaraColisiones))
        {
            Debug.Log("Collision con " + hitInfo.collider.gameObject.name);

            // Se saca la partícula del collider siguiendo la normal; el 0.005 evita que quede justo en la superficie
            particleA.posicion = hitInfo.point + hitInfo.normal * (radioCast + 0.005f);
            
            // Igualar posición pasada a la actual anula la velocidad, así que la partícula se frena en seco
            particleA.posicionPasada = particleA.posicion;
        }
    }


    void DibujarLista()
    {
        for (int i = 0; i < listaParticulas.Count; i++) {
            lineRenderer.SetPosition(i, listaParticulas[i].posicion);
        }
    }


}