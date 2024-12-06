using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorPuntos : MonoBehaviour
{
    public static ControladorPuntos Instancia;

    [SerializeField] private float cantidadPuntos;

    public float CantidadPuntos { get => cantidadPuntos; set => cantidadPuntos = value; }

    private void Awake()
    {

    }

    public void SumarPuntos(float tPuntos)
    {

        CantidadPuntos += tPuntos;
    }


    // Start is called before the first frame update
    void Start()
    {

    }
    public bool RestarPuntos(float cantidad)
    {
        if (CantidadPuntos >= cantidad)
        {
            CantidadPuntos -= cantidad;
            return true; // Resta exitosa
        }
        else
        {
            return false; // No hay suficientes puntos
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}