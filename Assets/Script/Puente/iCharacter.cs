using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  iCharacter : MonoBehaviour
{
    public iSpecialAction SpecialAction{get; set;}


    public void ejecutarAccionSpecial(){
        SpecialAction.Ejecutar();
    }
}
