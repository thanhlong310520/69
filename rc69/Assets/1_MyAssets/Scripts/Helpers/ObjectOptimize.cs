using System;
using UnityEngine;

namespace Raccoon.Helper
{
    public class ObjectOptimize : MonoBehaviour
    {
        private void Start()
        {
            //Application.lowMemory += OnLowMemory;
            //if (GameOptimize.Get != null)
            //{
            //    if (GameOptimize.Get.IsLowQuality || GameOptimize.Get.IsLowMemory)
            //    {
            //        OnLowMemory();
            //    }
            //}
        }

        private void OnLowMemory()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            Application.lowMemory -= OnLowMemory;
        }
    }
}