using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoSecreto : MonoBehaviour, iSpecialAction
{
    public SonidoSecreto(int sonido){
        this.Sonido = sonido;
    }
    public bool CondicionSecreta{get; set;}
    public int Sonido{ get; set; }


    public void Ejecutar(){
        if (CondicionSecreta == true){
            SFXBuilder builder = gameObject.AddComponent<SFXBuilder>();
            builder.Create(this.Sonido);
            builder.AgregarTono(1);
            builder.AgregarVolumen(0.2f);
            builder.Play();
        }
      
    }
}