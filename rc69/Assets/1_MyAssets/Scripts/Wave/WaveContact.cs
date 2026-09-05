using Raccoon.Controller;
using UnityEngine;

public class WaveContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var ic = other.GetComponent<ICharactor>();
        if (ic != null) ic.ContactWave();
    }
}
