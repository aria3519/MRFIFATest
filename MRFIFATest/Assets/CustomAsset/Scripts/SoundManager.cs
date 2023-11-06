using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SingletonPunBase;
using UnityEngine.Audio;

namespace Appnori.Util
{
    public class SoundManager : Singleton<SoundManager>
    {
        public AudioMixerGroup mixerGroup_Effect;
        private AudioSource audio;

        [System.Serializable]
        public class AudioClips
        {
            public string categoriy;
            public AudioClip[] clips;
        }

        public List<AudioClips> listAudioClips = new List<AudioClips>();

        protected override void Awake()
        {
            base.Awake();

            audio = gameObject.AddComponent<AudioSource>();
            audio.outputAudioMixerGroup = mixerGroup_Effect;
            AudioSetting(audio);
        }

        //// Update is called once per frame
        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
            //    Play("Test", "button 1");
            //}
        }

        public AudioClip GetClip(string _kategorie, string keyword)
        {
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;

            int _index = -1;

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name.Contains(keyword))
                    _index = i;                
            }

            if (_index != -1)
                return clips[_index];
            else
            {
                Debug.LogError("일치하는 사운드 클립이 없습니다!");
                return null;
            }
        }

        public AudioClip GetClip(string _kategorie, int _index)
        {
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;

            try
            {
                return clips[_index];                
            }

            catch
            {                
                Debug.LogError("클립배열에 없는 인덱스 입니다!");
                return null;
            }
        }

        public void Play(string _kategorie)
        {
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;

            int random = Random.Range(0, clips.Length);
            audio.PlayOneShot(clips[random]);
        }

        public void Play(string _kategorie, int _index)
        {
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;

            try
            {
                audio.PlayOneShot(clips[_index]);
            }

            catch
            {
                Debug.LogError("클립배열에 없는 인덱스 입니다!");                
            }                        
        }

        int index = -1;
        public void Play(string _kategorie, string _name)
        {
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;
 
            for(int i=0; i<clips.Length; i++)
            {
                if (clips[i].name == _name)
                    index = i;
            }

            if(index != -1)
            {
                audio.PlayOneShot(clips[index]);
                index = -1;
            }
        }

        public void Play(GameObject go, string _kategorie, bool is3D = false)
        {
            AudioSource audio;
            if (go.GetComponent<AudioSource>() == null)
            {
                audio = go.AddComponent<AudioSource>();
                AudioSetting(audio);
            }
            else
                audio = go.GetComponent<AudioSource>();

            if (is3D)
                audio.spatialBlend = 1;

            audio.outputAudioMixerGroup = mixerGroup_Effect;
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;
            int random = Random.Range(0, clips.Length);
            audio.PlayOneShot(clips[random]);
        }

        public void Play(GameObject go, string _kategorie, string _name, bool is3D = false, bool isLoop = false)
        {
            AudioSource audio;
            if (go.GetComponent<AudioSource>() == null)
            {
                audio = go.AddComponent<AudioSource>();
                AudioSetting(audio);
            }
            else
                audio = go.GetComponent<AudioSource>();

            if (is3D)
                audio.spatialBlend = 1;

            audio.outputAudioMixerGroup = mixerGroup_Effect;
            AudioClip[] clips = listAudioClips.Find(x => x.categoriy == _kategorie).clips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == _name)
                    index = i;
            }

            if (index != -1)
            {
                audio.loop = isLoop;
                audio.maxDistance = 30;
                audio.clip = clips[index];
                audio.Play();
                index = -1;
            }
        }

        public void Stop(GameObject go)
        {

            AudioSource audio;
            if (go.GetComponent<AudioSource>() != null)
            {
                audio = go.GetComponent<AudioSource>();
                audio.Stop();
            }
        }

        private void AudioSetting(AudioSource _audio)
        {
            _audio.playOnAwake = false;
            _audio.rolloffMode = AudioRolloffMode.Linear;
            _audio.minDistance = 0;
        }
    }

}
