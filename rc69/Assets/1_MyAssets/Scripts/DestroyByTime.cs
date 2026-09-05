using Raccoon.Controller;
using System.Collections;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{

    public float timeDestroy = 2;

    private void OnEnable()
    {
        StartCoroutine(PutToPool());
    }
    

    IEnumerator PutToPool()
    {
        yield return new WaitForSeconds(timeDestroy);

        GamePlayControl.instance.RespawnItem(gameObject);
    }
}
