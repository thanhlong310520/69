using UnityEngine;

namespace Raccoon
{
    public class RandomParticleColor : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;

        void Start()
        {
            if (particleSystem == null)
                particleSystem = GetComponent<ParticleSystem>();

            var main = particleSystem.main;

            // Random color
            main.startColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f // alpha
            );
        }
    }
}