using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;

public class Movable : MonoBehaviour
{
    private bool idle = true;
    public bool Idle => idle;

    [SerializeField] private float speed = 0.1f;

    public IEnumerator MoveToPosition(Vector3 targetPos, bool isPowerUp)
    {
        if (speed < 0)
        {
            Debug.LogWarning("Speed must be a positive number");
            yield break; // Exit the coroutine if speed is negative
        }

        float distance = Vector3.Distance(transform.position, targetPos);
        float duration;
        if (isPowerUp)
        {
            duration = 0.1f;
        }
        else
            duration = 0.5f;
        
        Tween moveTween = transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear);

        idle = false;

        yield return moveTween.WaitForCompletion();

        idle = true;
    }

    public IEnumerator SwapToPosition(Vector3 targetPos)
    {
        if (speed < 0)
        {
            Debug.LogWarning("Speed must be a positive number");
            yield break; // Exit the coroutine if speed is negative
        }

        float distance = Vector3.Distance(transform.position, targetPos);
        float duration = distance / speed;

        Tween moveTween = transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.Linear);

        idle = false;

        yield return moveTween.WaitForCompletion();

        idle = true;
    }

}
