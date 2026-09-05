using System.Collections.Generic;
using UnityEngine;

namespace Raccoon.Effect
{
    public class RandomEffectWeather : MonoBehaviour
    {
        [SerializeField] private List<GameObject> randomEffects = new List<GameObject>();
        [SerializeField] private GameObject effectObj;

        private float timer;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            effectObj = Instantiate(randomEffects[Random.Range(0, randomEffects.Count)], transform);
            effectObj.transform.localPosition = Vector3.zero;
            effectObj.SetActive(Random.value > 0.3f);
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (!(timer > 30f)) return;
            timer = 0;
            effectObj.SetActive(Random.value > 0.3f);
        }
    }
}