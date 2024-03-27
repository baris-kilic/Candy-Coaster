using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffects
{
    land,
    swap,
    resolve,
    upgrade,
    powerup,
    score,
    win,
    lose
}

//Singleton AudioManager class for handle sound effects and music.
[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private AudioSource music,
                        soundEffects;

    [SerializeField]
    private AudioClip backgroundMusic;

    [SerializeField]
    private AudioClip[] sounds;

    protected override void Init()
    {
        soundEffects = GetComponent<AudioSource>();
        music = GetComponent<AudioSource>();
        music.clip = backgroundMusic;
        music.loop = true;
    }

    public void PlayMusic()
    {
        music.Play();
    }

    public void StopMusic()
    {
        music.Stop();
    }

    public void PauseMusic(bool pause)
    {
        if (pause)
            music.Pause();
        else
            music.UnPause();
    }

    public void PlaySound(SoundEffects effect)
    {
        soundEffects.PlayOneShot(sounds[(int) effect]);
    }

    public IEnumerator PlayDelayedSound(SoundEffects effect, float t)
    {
        yield return new WaitForSeconds(t);
        PlaySound(effect);
    }
}
