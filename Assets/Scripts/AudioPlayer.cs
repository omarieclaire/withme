using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    [Tooltip("The current index of the audio source that will play the next sound.")]
    public int playID;

    [Tooltip("The previous index of the audio source that played a sound.")]
    public int oPlayID;

    [Tooltip("The total number of audio sources available for playing sounds.")]
    public int numSources;

    [Tooltip("The total number of audio sources dedicated to looping sounds.")]
    public int numLoopSources;

    [Tooltip("The total number of global audio sources dedicated to looping sounds.")]
    public int numGlobalLoopSources;

    [Tooltip("The master audio mixer controlling overall sound output.")]
    public AudioMixer master;

    [Tooltip("The primary audio source used for playing main audio clips.")]
    public AudioSource mainAudio;

    [Tooltip("Singleton instance of the AudioPlayer class.")]
    public static AudioPlayer Instance { get; private set; }

    [Tooltip("Array of game objects that hold the audio sources.")]
    public GameObject[] objects;

    [Tooltip("Array of game objects dedicated to looping audio sources.")]
    public GameObject[] loopObjects;

    [Tooltip("Array of game objects dedicated to global looping audio sources.")]
    public GameObject[] globalLoopObjects;

    [Tooltip("Array of audio sources used for playing sounds.")]
    public AudioSource[] sources;

    [Tooltip("Array of audio sources dedicated to looping sounds.")]
    public AudioSource[] loopSources;

    [Tooltip("Array of global audio sources dedicated to looping sounds.")]
    public AudioSource[] globalLoopSources;

    [Tooltip("Beats per minute for the looping audio.")]
    public float loopBPM;

    [Tooltip("Number of bars in the looping audio.")]
    public int loopBars;

    [Tooltip("Number of beats per bar in the looping audio.")]
    public int loopBPB;

    [Tooltip("Transform for positioning regular audio sources.")]
    public Transform sourceTransform;

    [Tooltip("Transform for positioning looping audio sources.")]
    public Transform loopTransform;

    [Tooltip("Transform for positioning global looping audio sources.")]
    public Transform globalLoopTransform;

    [Tooltip("Prefab for creating new audio source game objects.")]
    public GameObject sourcePrefab;


    public void OnEnable()
    {
        if (objects == null || objects.Length != numSources ||
            loopSources.Length != numLoopSources || loopSources == null ||
            globalLoopSources.Length != numGlobalLoopSources || globalLoopSources == null)
        {

            if (objects != null)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    Object.DestroyImmediate(objects[i]);//.Destroy();
                }
            }

            if (loopObjects != null)
            {
                for (int i = 0; i < loopObjects.Length; i++)
                {
                    Object.DestroyImmediate(loopObjects[i]);//.Destroy();
                }
            }

            if (globalLoopObjects != null)
            {
                for (int i = 0; i < globalLoopObjects.Length; i++)
                {
                    Object.DestroyImmediate(globalLoopObjects[i]);//.Destroy();
                }
            }

            sources = new AudioSource[numSources];
            objects = new GameObject[numSources];

            for (int i = 0; i < numSources; i++)
            {
                objects[i] = Instantiate(sourcePrefab);
                objects[i].transform.parent = sourceTransform;

                sources[i] = objects[i].GetComponent<AudioSource>();
                sources[i].dopplerLevel = 0;
                sources[i].playOnAwake = false;
            }

            loopSources = new AudioSource[numLoopSources];
            loopObjects = new GameObject[numLoopSources];
            for (int i = 0; i < numLoopSources; i++)
            {
                loopObjects[i] = new GameObject();
                loopObjects[i].transform.parent = loopTransform;

                loopSources[i] = loopObjects[i].AddComponent<AudioSource>();
                loopSources[i].volume = 0;
                loopSources[i].dopplerLevel = 0;
                loopSources[i].playOnAwake = false;
                loopSources[i].loop = true; // Ensure looping is enabled
                print(master.FindMatchingGroups("Loops"));
                loopSources[i].outputAudioMixerGroup = master.FindMatchingGroups("Loops")[0];
            }

            globalLoopSources = new AudioSource[numGlobalLoopSources];
            globalLoopObjects = new GameObject[numGlobalLoopSources];
            for (int i = 0; i < numGlobalLoopSources; i++)
            {
                globalLoopObjects[i] = new GameObject();
                globalLoopObjects[i].transform.parent = globalLoopTransform;

                globalLoopSources[i] = globalLoopObjects[i].AddComponent<AudioSource>();
                globalLoopSources[i].volume = 0;
                globalLoopSources[i].dopplerLevel = 0;
                globalLoopSources[i].playOnAwake = false;
                globalLoopSources[i].loop = true; // Ensure looping is enabled
                globalLoopSources[i].outputAudioMixerGroup = master.FindMatchingGroups("GlobalLoops")[0];
            }
        }
    }

    public float loopStartTime = 0;

    public float loopTime
    {
        get { return (loopBPM / 60) * loopBPB * loopBars; }
    }

    public float timeTilLoop
    {
        get
        {
            float fadeTime = ((loopStartTime + loopTime) - Time.time);
            return fadeTime;
        }
    }

        public bool IsClipPlaying(AudioClip clip)
{
    foreach (var source in sources)
    {
        if (source.isPlaying && source.clip == clip)
        {
            return true;
        }
    }
    return false;
}

    public void FadeLoop(int i, float v, float t)
    {
        StartCoroutine(Fade(loopSources[i], loopSources[i].volume, v, t));
    }

    public void FadeGlobalLoop(int i, float v, float t)
    {
        StartCoroutine(Fade(globalLoopSources[i], globalLoopSources[i].volume, v, t));
    }

    public void FadeValue(string valueName, float v, float t)
    {
        float sv;
        master.GetFloat(valueName, out sv);
        StartCoroutine(FadeVal(valueName, sv, v, t));
    }

    IEnumerator Fade(AudioSource a, float sv, float v, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            a.volume = Mathf.SmoothStep(sv, v, (i / time));
            yield return null;
        }
    }

    IEnumerator FadeIn(AudioSource a, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            a.volume = i / time;
            yield return null;
        }
    }

    IEnumerator FadeOut(AudioSource a, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            a.volume = 1 - (i / time);
            yield return null;
        }
    }

    IEnumerator FadeVal(string valueName, float sv, float v, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            float newVal = Mathf.SmoothStep(sv, v, (i / time));
            master.SetFloat(valueName, newVal);
            yield return null;
        }
    }

    public void Update()
    {
        if (Time.time - loopStartTime > (loopBPM / 60) * loopBPB * loopBars)
        {
            NewLoop();
        }
    }

    public void Start()
    {
        NewLoop();
        NewGlobalLoop();
    }

    public void NewLoop()
    {
        loopStartTime = Time.time;
        for (int i = 0; i < loopSources.Length; i++)
        {
            if (loopSources[i].clip != null)
            {
                loopSources[i].Play();
            }
        }
    }

    public void NewGlobalLoop()
    {
        for (int i = 0; i < globalLoopSources.Length; i++)
        {
            if (globalLoopSources[i].clip != null)
            {
                globalLoopSources[i].Play();
            }
        }
    }

    public void Next()
    {
        oPlayID = playID;
        playID++;
        playID %= numSources;
    }

    /* Play Methods */

    public void Play(AudioClip clip)
    {
        sources[playID].clip = clip;
        sources[playID].Play();

        oPlayID = playID;
        playID++;
        playID %= numSources;
    }

    public void Play(AudioClip clip, float pitch)
    {
        sources[playID].volume = 1;
        sources[playID].time = 0;
        sources[playID].pitch = pitch;
        Play(clip);
    }

    public void Play(AudioClip clip, float pitch, float volume)
    {
        sources[playID].time = 0;
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        Play(clip);
    }

    public void Play(AudioClip clip, int step, float volume)
    {
        float p = Mathf.Pow(1.05946f, (float)step);
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        Play(clip);
    }

    public void Play(AudioClip clip, int step, float volume, float location, AudioMixer mixer, string group)
    {
        float p = Mathf.Pow(1.05946f, (float)step);
        sources[playID].volume = volume;
        sources[playID].time = location;
        sources[playID].pitch = p;
        Play(clip);
    }

    public void Play(AudioClip clip, int step, float volume, float location)
    {
        float p = Mathf.Pow(1.05946f, (float)step);
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].time = location;
        Play(clip);
    }

    public void Play(AudioClip clip, int step, float volume, float location, float length)
    {
        float p = Mathf.Pow(1.05946f, (float)step);
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].time = location;
        sources[playID].SetScheduledEndTime(AudioSettings.dspTime + .25f);
        Play(clip);
    }

    public void Play(AudioClip clip, float pitch, float volume, float location, float length)
    {
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        sources[playID].time = location;

        Play(clip);

        sources[oPlayID].SetScheduledEndTime(AudioSettings.dspTime + length);
    }

    public void Play(AudioClip clip, float pitch, float volume, float location, float length, AudioMixer mixer, string group)
    {
        sources[playID].clip = clip;
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        sources[playID].time = location;

        Play(clip);

        sources[oPlayID].SetScheduledEndTime(AudioSettings.dspTime + length);
    }

    public void Play(AudioClip clip, int step, float volume, Vector3 location, float falloff)
    {
        float p = Mathf.Pow(1.05946f, (float)step);
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].spatialize = true;
        sources[playID].spatialBlend = 1;
        sources[playID].maxDistance = falloff;
        sources[playID].minDistance = falloff / 10;

        objects[playID].transform.position = location;
        Play(clip);
    }
}
