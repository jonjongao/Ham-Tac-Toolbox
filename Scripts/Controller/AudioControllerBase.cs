using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HamTac
{
    public class AudioControllerBase : MonoBehaviour
    {
        protected static AudioControllerBase m_current;
        public static AudioControllerBase current
        {
            get
            {
                if (m_current == null)
                {
                    var i = FindObjectOfType<AudioControllerBase>();
                    m_current = i;
                }
                return m_current;
            }
            set => m_current = value;
        }

        [SerializeField]
        AudioSource[] m_bgmChannel;
        [SerializeField]
        AudioSource m_sfxChannel;

        [SerializeField]
        protected AudioMixer m_masterMixer;
        public AudioMixer masterMixer
        {
            get
            {
                if (m_bgmChannel.Length == 0 || m_sfxChannel == null)
                {
                    Debug.LogError("No valid audio source assigned");
                    return null;
                }
                if (m_masterMixer == null)
                {
                    var source = m_bgmChannel.Append(m_sfxChannel);
                    current.m_masterMixer = source.Where(x => x.outputAudioMixerGroup.audioMixer != null).FirstOrDefault()?.outputAudioMixerGroup.audioMixer;
                }
                return m_masterMixer;
            }
        }
        [SerializeField]
        AudioMixerGroup m_soundGroup;
        public AudioMixerGroup soundGroup => m_soundGroup;
        [SerializeField]
        AudioMixerGroup m_musicGroup;
        public AudioMixerGroup musicGroup => m_musicGroup;

        protected Source[] m_bgm;
        public Source[] bgm => m_bgm;

        protected Source m_sfx;
        public Source sfx => m_sfx;

        [SerializeField]
        AudioClipLookupTable m_table;

        protected virtual void Awake()
        {
            current = this;

            current.m_bgm = new Source[m_bgmChannel.Length];

            for (int i = 0; i < current.m_bgm.Length; i++)
            {
                current.m_bgm[i] = new Source()
                {
                    type = AudioChannel.BGM,
                    source = m_bgmChannel[i],
                    isBusy = false
                };
            }


            current.m_sfx = new Source()
            {
                type = AudioChannel.SFX,
                source = m_sfxChannel,
                isBusy = false
            };

            
        }

        protected Dictionary<string, float> m_oneShotHistory = new Dictionary<string, float>();

        [SerializeField] protected string m_playingBGM;


        [System.Serializable]
        public class Source
        {
            public AudioChannel type;
            public AudioSource source;
            public AudioClip clip => source != null ? source.clip : null;
            public bool isBusy
            {
                get => m_isBusy;
                set
                {
                    //Debug.LogWarningFormat("Set[{0}] isBusy:{1}", this.type, value);
                    m_isBusy = value;
                }
            }
            bool m_isBusy;
            public bool isExist => source != null;
        }

        public static void SetBGM(string clipID)
        {
            SetBGM(true, 0, 1f, 0, clipID);
        }

        public static void SetBGM(string isOn, string clipID)
        {
            var a = isOn.ToBool();
            SetBGM(a, 0, 1f, 0, clipID);
        }

        public static bool CanPlayBGM(string clipID = null)
        {
            var channel = current.bgm[0];
            //Debug.LogWarningFormat("Chk channel:{0} isbusy:{1} exist:{2}",
            //	"BGM", channel.isBusy,channel.isExist);
            if (channel.isExist == false)
            {
                //Debug.LogWarningFormat("bgm is not exist");
                return false;
            }
            if (channel.isBusy)
            {
                //Debug.LogWarningFormat("bgm is budy"); ;
                return false;
            }
            if (string.IsNullOrEmpty(clipID))
            {

            }
            else
            {
                //var clip = AudioModel.FindAudioByKey(clipID);
                var clip = m_current.m_table.FindByKey(clipID).clip;
                if (channel.isExist && channel.isBusy == false)
                {
                    if (clip != null && clip != channel.clip)
                        return true;
                }
            }
            return false;
        }

        public static bool GetBGMSourceVolume(int index,out float value)
        {
            try
            {
                value = current.m_bgmChannel[index].volume;
                return true;
            }catch(System.Exception err)
            {
                value = 0f;
            }
            return false;
        }
        public static void SetBGMSourceVolume(int index, float endValue, float duration)
        {
            if (Mathf.Abs(endValue - current.m_bgmChannel[index].volume) < 0.01f) return;
            var seq = DOTween.Sequence();
            seq.Append(current.m_bgmChannel[index].DOFade(endValue, duration));
        }

        public static void SetBGM(bool isOn)
        {
            if (isOn)
            {
                SetBGM(isOn, 0, 0, 0, null);
            }
            else
            {
                SetBGM(false, 0, 0, 0, null);
                SetBGM(false, 1, 0, 0, null);
            }
        }

        public static bool HasExistBGM(int index)
        {
            if (current == null) return false;
            try
            {
                return current.m_bgmChannel[index].clip != null;
            }catch(System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="index"></param>
        /// <param name="startVolume"></param>
        /// <param name="duration"></param>
        /// <param name="clipID">Can be null</param>
        public static void SetBGM(bool isOn, int index, float startVolume, float duration, string clipID)
        {
            JDebug.W($"SetBGM on:{isOn} index:{index} startVol:{startVolume} duration:{duration} clipID:{clipID}");
           
            AudioSource channel = null;
            try
            {
                channel = current.m_bgmChannel[index];
            }
            catch (System.IndexOutOfRangeException)
            {
                JDebug.E($"BGM dont have index:{index}"); return;
            }
            catch (System.NullReferenceException)
            {
                JDebug.E($"BGM index:{index} is null"); return;
            }
            if (channel == null)
            {
                JDebug.E($"BGM index:{index} is null"); return;
            }
            if (isOn)
            {
                JDebug.Log($"AudioController Stop Try play BGM:{clipID}");
                AudioClip clip = null;
                if (!string.IsNullOrEmpty(clipID)) clip = current.m_table.FindByKey(clipID).clip;
                if (clip == channel.clip) return;
                var seq = DOTween.Sequence();
                if (channel.isPlaying)
                {
                    JDebug.W($"BGM is playing, try fade between clips from:{channel.clip.name} to:{clip.name}");
                    seq.AppendCallback(() =>
                    {
                        JDebug.W($"Set volume of channel:{index} to:{0}");
                    });
                    seq.Append(channel.DOFade(0f, duration));
                }
                seq.AppendCallback(() =>
                {
                    AudioControllerBase.PlayClip(current.bgm[index], clip, true);
                    channel.volume = 0f;
                    current.m_playingBGM = clipID;
                });
                seq.AppendCallback(() =>
                {
                    JDebug.W($"Set volume of channel:{index} to:{startVolume}");
                });
                seq.Append(channel.DOFade(startVolume, duration));
                seq.SetId(current);
            }
            else
            {
                var seq = DOTween.Sequence();
                seq.AppendCallback(() =>
                {
                    JDebug.W($"Set volume of channel:{index} to:{0}");
                });
                seq.Append(channel.DOFade(0f, duration));
                seq.AppendCallback(() =>
                {
                    AudioControllerBase.Stop(current.bgm[index]);
                });
                seq.SetId(current);
            }
        }


        public static async void PlaySFX(string clipID, float delay = 0f)
        {
            //Debug.Log($"Try play SFX:{clipID}");
            //var clip = AudioModel.FindClipByKey(clipID);
            var clip = m_current.m_table.FindByKey(clipID)?.clip;
            //Debug.LogError($"find result:{clip}");
            if (clip != null)
            {
                await AudioControllerBase.PlayClip(current.sfx, clip, false, delay);
            }
        }

        public static void SetVolume(AudioChannel target, float value)
        {
            try
            {
                if(current.masterMixer==null)
                JDebug.W($"Set volume of channel:{target.GetType().Name} to:{value}");
                value = Mathf.Clamp(value, 0.01f, 0.99f);
                var vol = Mathf.Log10(value) * 20;
                //GetChannel(target).volume = value;
                switch (target)
                {
                    case AudioChannel.BGM:
                        current.masterMixer.SetFloat("BGMVol", vol);
                        break;
                    case AudioChannel.SFX:
                        current.masterMixer.SetFloat("SFXVol", vol);
                        break;
                    default:
                        current.masterMixer.SetFloat("MasterVol", value <= 0.01f ? -80f : vol);
                        break;
                }
            }catch(System.Exception err)
            {
                Debug.LogError(err);
            }
        }

        public static void PlayClip(AudioClip clip,float delay=0f) { PlayClip(current.sfx,clip,false,delay); }
        public static void PlayClip(Source source, AudioClip clip) { PlayClip(source, clip, false); }
        public static async Task PlayClip(Source source, AudioClip clip, bool isLoop, float delay=0f)
        {
            Debug.Log($"PlayClip:{clip?.name} delay:{delay}");
            if (clip == null)
            {
                Debug.LogError($"Try PlayClip but clip is null");
                return;
            }
            if (delay > 0f)
                await Extension.Async.Delay(delay);
            var t = source;
            t.source.loop = isLoop;
            if (t.type == AudioChannel.SFX && isLoop == false)
            {
                //Debug.LogWarningFormat("play clip:{0}", clip.name);
                if (current.m_oneShotHistory.ContainsKey(clip.name))
                {
                    var g = Time.time - current.m_oneShotHistory[clip.name];
                    if (g < 0.1f)
                    {
                        //Debug.LogWarningFormat("Skip sfx:{0} gap:{1}", clip.name, g);
                    }
                    else
                    {
                        t.source.PlayOneShot(clip, t.source.volume);
                        current.m_oneShotHistory[clip.name] = Time.time;
                    }
                }
                else
                {
                    t.source.PlayOneShot(clip, t.source.volume);
                    current.m_oneShotHistory.Add(clip.name, Time.time);
                }
            }
            else
            {
                if (t.isBusy)
                {
                    //Debug.LogWarningFormat("<color=red>Forced stop BGM easing</color>");
#if DOTWEEN_INSTALLED
                    t.source.DOKill(true);
#endif
                }
                t.isBusy = true;
                //Debug.LogWarningFormat("<color=red>Channel [{0}] is busy:{1}</color>", t.type, t.isBusy);
#if DOTWEEN_INSTALLED
                t.source.clip = clip;
                t.source.Play();
                //var s = DOTween.Sequence();
                //s.Append(t.source.DOFade(0f, 0.5f).SetSpeedBased(true));
                //s.AppendCallback(() =>
                //{
                //    t.source.clip = clip;
                //    t.source.Play();
                //});
                //s.Append(t.source.DOFade(1f, 0.5f).SetSpeedBased(true));
                //s.AppendCallback(() =>
                //{
                //    t.isBusy = false;
                //});
#endif
                //t.source.clip = clip;
                //t.source.Play();
            }
        }



        public static async void Stop(Source source)
        {
            await StopAsync(source);
        }

        public static async Task StopAsync(Source source)
        {
            JDebug.Log($"AudioController Stop Audio stop, channel:{source.ToString()}");
            var t = source;
            if (t.isExist)
            {
                t.source.Stop();
                t.source.clip = null;
            }
            await Task.Yield();
        }

    }
}