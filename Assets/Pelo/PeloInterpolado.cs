using System.Collections.Generic;
using UnityEngine;

public class PeloInterpolado : MonoBehaviour
{
    [SerializeField] Pelos[] guias;          // Cuerdas simuladas (necesitan exponer sus posiciones)
    [SerializeField] int pelosPorGuia = 8;           // Pelos renderizados extra por cada guía
    [SerializeField] int subdivisiones = 4;          // Puntos extra entre partículas (spline)
    [SerializeField] float dispersion = 0.05f;       // Separación aleatoria entre pelos
    [SerializeField] Material material;

    class PeloRender
    {
        public int guiaA, guiaB;     // Las dos guías entre las que se interpola
        public float peso;           // 0 = igual a guiaA, 1 = igual a guiaB
        public Vector3 offset;       // Pequeña variación para que no se solapen
        public LineRenderer linea;
    }

    List<PeloRender> pelos = new List<PeloRender>();

    void Start()
    {
        for (int g = 0; g < guias.Length; g++)
        {
            int vecina = (g + 1) % guias.Length;
            for (int k = 0; k < pelosPorGuia; k++)
            {
                var go = new GameObject("Pelo");
                go.transform.SetParent(transform);
                var lr = go.AddComponent<LineRenderer>();
                lr.material = material;
                lr.widthMultiplier = 0.01f;
                lr.useWorldSpace = true;

                pelos.Add(new PeloRender {
                    guiaA = g,
                    guiaB = vecina,
                    peso = Random.value,                          // Posición aleatoria entre las dos guías
                    offset = Random.insideUnitSphere * dispersion,
                    linea = lr
                });
            }
        }
    }

    void LateUpdate() // Después de que FixedUpdate haya movido las guías
    {
        foreach (var p in pelos)
        {
            var a = guias[p.guiaA].Posiciones;
            var b = guias[p.guiaB].Posiciones;
            int n = a.Count;

            // Interpolación lineal punto a punto entre las dos guías
            Vector3[] puntos = new Vector3[n];
            for (int i = 0; i < n; i++)
                puntos[i] = Vector3.Lerp(a[i], b[i], p.peso) + p.offset;

            // Suavizado con Catmull-Rom para que se vea curvo con pocas partículas
            DibujarSpline(p.linea, puntos);
        }
    }

    void DibujarSpline(LineRenderer lr, Vector3[] c)
    {
        int n = c.Length;
        int total = (n - 1) * subdivisiones + 1;
        lr.positionCount = total;
        int idx = 0;
        for (int i = 0; i < n - 1; i++)
        {
            // Se duplican los extremos para tener 4 puntos de control en los bordes
            Vector3 p0 = c[Mathf.Max(i - 1, 0)];
            Vector3 p1 = c[i];
            Vector3 p2 = c[i + 1];
            Vector3 p3 = c[Mathf.Min(i + 2, n - 1)];
            for (int s = 0; s < subdivisiones; s++)
                lr.SetPosition(idx++, CatmullRom(p0, p1, p2, p3, s / (float)subdivisiones));
        }
        lr.SetPosition(idx, c[n - 1]);
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (
            2f * p1
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}