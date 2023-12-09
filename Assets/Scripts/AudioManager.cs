using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private string currScene = "";
    private bool[] oldIa = { false, false, false, false, false };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        Play("Menu");
    }

    void Update()
    {
        GameObject[] prefabInstances = GameObject.FindGameObjectsWithTag("AI");
        if (prefabInstances.Length == 0)
            return;


        bool[] currIa = { false, false, false, false};
        string[] idInstruments = { "Triangle", "Marching Cymbals", "Timpani", "Tubular Bells" };

        for (int i = 0; i < prefabInstances.Length; i++)
        {
            //Debug.Log(prefabInstances[i].name);
            if (prefabInstances[i].name == "AITankBrown")
                currIa[0] = true;
            if (prefabInstances[i].name == "AITankAsh")
                currIa[1] = true;
            if (prefabInstances[i].name == "AITankGreen")
                currIa[2] = true;
            if (prefabInstances[i].name == "AITankMarine")
                currIa[3] = true;
        }

        if (currScene != SceneManager.GetActiveScene().name)
        {
            oldIa = currIa;
            StopEverything();
            Play("Flute");
            Play("Marching Snare Drums");
            Play("Piccolo");
            Play("Trumpet");

            for (int i = 0; i < currIa.Length; i++)
                if (currIa[i])
                    Play(idInstruments[i]);
        }

        for (int i = 0; i < currIa.Length; i++)
        {
            if (oldIa[i] != currIa[i])
            {
                oldIa[i] = currIa[i];
                Stop(idInstruments[i]);
            }
        }

        currScene = SceneManager.GetActiveScene().name;
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;

        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;

        }
        s.source.Stop();
    }

    public void PauseEverything()
    {
        foreach (Sound s in sounds)
        {
            s.source.Pause();
        }
    }

    public void StopEverything()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }
}
