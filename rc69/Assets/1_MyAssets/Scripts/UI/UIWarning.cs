using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIWarning : MonoBehaviour
{
    [SerializeField] private Image overlay;

    private Sequence sequence;

    private void Awake()
    {
        overlay.color = new Color(1, 0, 0, 0);
        overlay.gameObject.SetActive(false);
    }

    public void StartWarning()
    {
        StopWarning();
        overlay.gameObject.SetActive(true);
        StartCoroutine(asyncStartWarning());
    }

    IEnumerator asyncStartWarning()
    {
        sequence = DOTween.Sequence()
            .Append(overlay.DOFade(0.45f, 0.35f).SetEase(Ease.InOutSine))
            .Append(overlay.DOFade(0f, 0.35f).SetEase(Ease.InOutSine))
            .SetLoops(-1);
        yield return new WaitForSeconds(2);
        StopWarning();

    }
    public void StopWarning()
    {
        sequence?.Kill();

        overlay.DOFade(0, 0.2f);
        overlay.gameObject.SetActive(false);
    }
}
