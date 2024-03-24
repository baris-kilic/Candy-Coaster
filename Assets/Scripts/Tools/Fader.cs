using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Image))]
public class Fader : MonoBehaviour
{
    private Image toFade;

    [SerializeField] private float fadeSpeed = 1;

    private void Awake()
    {
        toFade = GetComponent<Image>();
    }

    public void Hide(bool hidden)
    {
        toFade.enabled = !hidden;
    }

    public IEnumerator Fade(float targetAlpha)
    {
        Color currentColor = toFade.color;
        currentColor.a = targetAlpha;

        Tween fadeTween = toFade.DOFade(targetAlpha, fadeSpeed).OnComplete(() =>
        {
           
        });
        yield return fadeTween.WaitForCompletion();
    }
}