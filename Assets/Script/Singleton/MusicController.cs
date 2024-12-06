using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public static MusicController instancia;

    [SerializeField] public List<AudioClip> tracklist;

    [SerializeField] public AudioSource _audioSource;

    public AudioSource AudioSource { get => _audioSource; set => _audioSource = value; }
    void Start()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            PlayTrack(1);
        }


    }
    public void Awake()
    {
        if (MusicController.instancia == null)
        {
            MusicController.instancia = this;

            DontDestroyOnLoad(this.gameObject);



        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayTrack(int trackIndex)
    {
        if (_audioSource == null)
        {

            Debug.LogError("AudioSource component is missing. Please attach an AudioSource to the GameObject.");
            return;
        }
        if (trackIndex >= 0 && trackIndex < tracklist.Count)
        {
            _audioSource.clip = tracklist[trackIndex];
            _audioSource.Play();
        }
        else
        {
            Debug.LogError("Invalid track index");
        }
    }
}