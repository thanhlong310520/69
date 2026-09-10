using UnityEngine;

[CreateAssetMenu(fileName = "BossDataSO", menuName = "Raccoon/Boss/BossDataSO")]
public class BossDataSO : ScriptableObject
{
    public GameObject prefab;
    public float speed;
}
