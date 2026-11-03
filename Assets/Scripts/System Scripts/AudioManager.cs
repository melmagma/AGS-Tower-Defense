using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; //one copy of this in the entire game

    public AudioSource menuMusic;
    public AudioSource[] bgm; //notes for self, atm bgm is basically our level music. though i might compose 1 track for all the levels but we will see what works better in terms of game design

    private int currentBGM;
    private bool playingBGM;

    public AudioSource[] sfx;

    private void Awake()
    {
        if (instance == null)
        {

            instance = this; 

            DontDestroyOnLoad(gameObject); //ensures that after scene load/etc, the audiomanager doesnt get destroyed
        }
        else if (instance != this)
        {
            Destroy(gameObject); //prevent copies of this game object
        }
    }

    private void Update()
    {
        if (playingBGM)
        {
            if (bgm[currentBGM].isPlaying == false) //if the current bgm is done, +1 to the int to advance into track 2 in the array.
            {
                currentBGM++;
                if (currentBGM >= bgm.Length) // when the int is = to the length in the array of bgm, reset back to 0.
                {
                    currentBGM = 0; //resets the playlist bck to the first track
                }

                bgm[currentBGM].Play();
            }
        }
    }
    public void StopMusic()
    {
        menuMusic.Stop();

        foreach (AudioSource track in bgm) //stops all audiosource tracks in the bgm array
        {
            track.Stop();
        }
        playingBGM = false;
    }

    public void PlayMenuMusic()
    {
        StopMusic();
        menuMusic.Play();
    }

    public void PlayBGM()
    {
        StopMusic();

        currentBGM = Random.Range(0, bgm.Length);

        bgm[currentBGM].Play();
        playingBGM = true;
    }

    public void PlaySFX(int sfxToPlay)
    {
        AudioSource s = sfx[sfxToPlay];
        s.Stop(); //make sure sfx plays from the start
        s.Play();

        Debug.Log($"sfx[{sfxToPlay}] -> object '{s.name}', clip '{(s.clip != null ? s.clip.name : "NONE")}', " +
             $"vol {s.volume}, mute {s.mute}, enabled {s.isActiveAndEnabled}, playing {s.isPlaying}");
    }
}