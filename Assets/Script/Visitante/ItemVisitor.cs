using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemVisitor
{
    void VisitPocion(pocionItem item);
    
}