using System;
using UnityEngine.Audio;
using Experiments.Global.Managers;
using UnityEngine;

namespace Experiments.Global.Audio
{
    public class AudioManager : Manager<AudioManager>
    {
        [Space]
        public AudioMixer MasterMixer;
        public bool Mute;

        [Header("Sound Effects")]
        public SoundEffect[] SoundEffects;
        public AudioMixerGroup SFXOutputGroup;

        [Header("Music")]
        public AudioClip[] MusicClips;
        public AudioMixerGroup MusicOutputGroup;
        SoundEffect MusicOutput;

        // Start is called before the first frame update
        void Awake()
        {
            Init(this);

            MusicOutput = new SoundEffect();
            MusicOutput.Name = "Music And Background";
            MusicOutput.IsBackground = true;
            MusicOutput.Volume = 1f;
            MusicOutput.Loop = true;
            MusicOutput.UpdateSettings(transform);

            Update();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (SoundEffect SFX in SoundEffects)
            {
                SFX.UpdateSettings(transform);
            }
            MusicOutput.UpdateSettings(transform);
        }

        public void InteractWithSFX(string SFXName, SoundEffectBehaviour behaviour)
        {
            if(Mute) { return; }

            SoundEffect soundEffect = Array.Find(SoundEffects, SoundEffect => SoundEffect.Name == SFXName);
            if(soundEffect == null)
            {
                Debug.LogError("Sound Effect " + SFXName + " Does Not Exist In The Audio Manager!");
                return;
            }

            soundEffect.Interact(behaviour);
        }
        public void InteractWithSFXOneShot(string SFXName, SoundEffectBehaviour behaviour)
        {
            if(Mute) { return; }

            SoundEffect soundEffect = Array.Find(SoundEffects, SoundEffect => SoundEffect.Name == SFXName);
            if (soundEffect == null)
            {
                Debug.LogError("Sound Effect " + SFXName + " Does Not Exist In The Audio Manager!");
                return;
            }

            soundEffect.InteractOneShot(behaviour);
        }
        public void InteractWithAllSFX(SoundEffectBehaviour behaviour)
        {
            if(Mute) { return; }

            foreach (SoundEffect SFX in SoundEffects)
            {
                if(!SFX.IgnoresAllInteraction)
                {
                    SFX.Interact(behaviour);
                }
            }
        }
        public void InteractWithAllSFXOneShot(SoundEffectBehaviour behaviour)
        {
            if(Mute) { return; }

            foreach (SoundEffect SFX in SoundEffects)
            {
                if (!SFX.IgnoresAllInteraction)
                {
                    SFX.InteractOneShot(behaviour);
                }
            }
        }

        public void SetMusicTrack(string TrackName)
        {
            AudioClip Clip = Array.Find(MusicClips, AudioClip => AudioClip.name == TrackName);
            if(Clip == null)
            {
                Debug.LogError("Music Clip " + TrackName + " Does Not Exist In The Audio Manager!");
                MuteMusic();
                return;
            }
            else if(Clip == MusicOutput.Clip)
            {
                Debug.LogError("Music Clip " + TrackName + " Is Already Playing!");
                return;
            }

            MusicOutput.Interact(SoundEffectBehaviour.Stop);
            MusicOutput.Clip = Clip;
            MusicOutput.UpdateSettings(transform);
            MusicOutput.Interact(SoundEffectBehaviour.Play);
        }
        public void MuteMusic()
        {
            MusicOutput.Interact(SoundEffectBehaviour.Stop);
            MusicOutput.Clip = null;
        }
        public void InteractWithMusic(SoundEffectBehaviour behaviour)
        {
            MusicOutput.Interact(behaviour);
        }

        public void SetChannelVolume(string Channel, float Value)
        {
            MasterMixer.SetFloat(Channel + "Vol", Value);
        }
        public float GetChannelVolume(string Channel)
        {
            float Result = 0f;
            MasterMixer.GetFloat(Channel + "Vol", out Result);
            return Result;
        }
    }
}