using UnityEngine;
using TMPro;

namespace Raccoon.Helper
{
    [RequireComponent(typeof(TMP_Text))]
    public class RainbowScrollText : MonoBehaviour
    {
        public float speed = 0.5f;   // Speed of rainbow scrolling
        public float scale = 0.002f; // Rainbow wavelength (smaller = more cycles across text)

        private TMP_Text tmpText;

        void Awake()
        {
            tmpText = GetComponent<TMP_Text>();
        }

        void Update()
        {
            if (tmpText == null) return;
            tmpText.ForceMeshUpdate();
            var textInfo = tmpText.textInfo;

            float time = Time.time * speed;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // Loop through each of the 4 vertices of this character quad
                for (int v = 0; v < 4; v++)
                {
                    float xPos = vertices[vertexIndex + v].x * scale;
                    float hue = Mathf.Repeat(time + xPos, 1f);
                    Color color = Color.HSVToRGB(hue, 1f, 1f);
                    vertexColors[vertexIndex + v] = color;
                }
            }

            // Apply updated colors
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.colors32 = meshInfo.colors32;
                tmpText.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}