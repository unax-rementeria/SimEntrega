using UnityEngine;

public class VerletPoint : MonoBehaviour
{
    Vector3 posicionActual;
    Vector3 posicionPasada;
    [SerializeField]
    Vector3 velocidadInicial = new Vector3(1, 1, 0);
    [SerializeField]
    Vector3 aceleracion = new Vector3 (0, -9.8f, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posicionActual = transform.position;
        posicionPasada = posicionActual - velocidadInicial*Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        Vector3 movimiento = posicionActual - posicionPasada;
        Vector3 posicionNueva = posicionActual + movimiento + aceleracion * fdt * fdt;
        posicionPasada = posicionActual;
        posicionActual = posicionNueva;
        transform.position = posicionActual;
    }
}
