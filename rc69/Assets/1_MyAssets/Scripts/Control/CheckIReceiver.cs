using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;

public class CheckIReceiver
{
    public List<IReceive> CheckAreaPoint(Vector3 posCheck, float radius,Transform user)
    {
        List<IReceive> receives = new List<IReceive>();

        var colliders = Physics.OverlapSphere(posCheck, radius);

        foreach (var collider in colliders)
        {
            if (collider.transform == user) continue;
            IReceive receive = collider.GetComponent<IReceive>(); 
            if(receive != null) receives.Add(receive);
        } 
        return receives;
    }
}
