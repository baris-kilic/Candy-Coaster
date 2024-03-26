using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Hint : Singleton<Hint>
{
    SpriteRenderer hintIndicator;
    private Coroutine hintCoroutine;

    protected override void Init()
    {
        hintIndicator = GetComponent<SpriteRenderer>();
        hintIndicator.enabled = false;
    }

    public void IndicateHint(Transform location)
    {
        if (PlayerPrefs.GetInt("Hint") == 2)
            return;
        // Check if a hint coroutine is already running
        if (hintCoroutine != null)
        {
            // If a coroutine is running, stop it before starting a new one
            StopCoroutine(hintCoroutine);
        }

        // Start a new coroutine
        hintCoroutine = StartCoroutine(ShowHint(location));
    }

    private IEnumerator ShowHint(Transform location)
    {
        // Wait for the delay
        yield return new WaitForSeconds(6f);

        // Perform the operation after the delay
        transform.position = location.position;
        hintIndicator.enabled = true;
    }

    public void CancelHint()
    {
        if (PlayerPrefs.GetInt("Hint") == 2)
            return;
        // Check if a hint coroutine is running
        if (hintCoroutine != null)
        {
            // If a coroutine is running, stop it
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        hintIndicator.enabled = false;
    }
}
