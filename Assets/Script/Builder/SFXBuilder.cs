using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SFXBuilder : MonoBehaviour
{
    public AudioSource _audioSource;

    static SFXBuilder()
    {

    }

    public void Create(int sound)
    {
        this._audioSource = gameObject.AddComponent<AudioSource>();
        this._audioSource.clip = MusicController.instancia.tracklist[sound];
    }

    public void Play()
    {
        this._audioSource.Play();

    }

    public void AgregarTono(int tono)
    {
        this._audioSource.pitch = tono;
    }
    public void AgregarVolumen(float volumen)
    {
        this._audioSource.volume = volumen;
    }

    public void AgregarPaneo(float pan)
    {
        this._audioSource.panStereo = pan;
    }

    public AudioSource GetAudioSource()
    {
        return this._audioSource;
    }

}