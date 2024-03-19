using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using Unity.Mathematics;

public class Movable : MonoBehaviour
{
    private Vector3 from, to;
    private float howfar;

    private bool idle = true;
    public bool Idle
    {
        get
        {
            return idle;
        }
    }

    [SerializeField] private float speed = 1;

    public IEnumerator MoveToPosition(Vector3 targetPos)
    {
        if (speed <= 0)
        {
            Debug.LogWarning("Speed must be a positive number");
            yield break; // Exit the coroutine if speed is negative
        }
        from = transform.position;
        to = targetPos;
        howfar = 0;
        idle = false;
        do
        {
            howfar += speed * Time.deltaTime;
            if (howfar > 1)
                howfar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));
            yield return null;
        } while (howfar != 1);
        idle = true;

    }

    private float Easing(float f)
    {
        float c1 = 1.70158f,
            c2 = c1 * 1.525f;
        return f < 0.5f
            ? (Mathf.Pow(f * 2, 2) * ((c2 + 1) * 2 * f - c2)) / 2
            : (Mathf.Pow(f * 2 - 2, 2) * ((c2 + 1) * (f * 2 - 2) + c2) + 2) / 2;
    }

    public IEnumerator MoveToPositionTween(Vector3 targetPos, bool isPowerUp)
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

        Tween moveTween = transform.DOMove(targetPos, 0.2f)
            .SetEase(Ease.Linear);

        idle = false;

        yield return moveTween.WaitForCompletion();

        idle = true;
    }


}
