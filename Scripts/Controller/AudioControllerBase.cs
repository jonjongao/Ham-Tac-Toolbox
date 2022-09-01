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
        AudioSource m_bgmChannel;
        [SerializeField]
        AudioSource m_sfxChannel;

        [SerializeField]
        protected AudioMixer m_masterMixer;

        protected Source m_bgm;
        public Source bgm => m_bgm;

        protected Source m_sfx;
        public Source sfx => m_sfx;

        [SerializeField]
        AudioClipLookupTable m_table;

        protected virtual void Awake()
        {
            current = this;

            current.m_bgm = new Source()
            {
                type = AudioChannel.BGM,
                source = m_bgmChannel,
                isBusy = false
            };

            current.m_sfx = new Source()
            {
                type = AudioChannel.SFX,
                source = m_sfxChannel,
                isBusy = false
            };

            if (m_masterMixer == null)
            {
                current.m_masterMixer = m_bgm.source.outputAudioMixerGroup.audioMixer;
            }
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
            SetBGM(true, clipID);
        }

        public static void SetBGM(string isOn, string clipID)
        {
            var a = isOn.ToBool();
            SetBGM(a, clipID);
        }

        public static bool CanPlayBGM(string clipID = null)
        {
            var channel = AudioControllerBase.GetChannel(AudioChannel.BGM);
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

        public static void SetBGM(bool isOn, string clipID = null)
        {
            if (isOn)
            {
                JDebug.Log($"AudioController Stop Try play BGM:{clipID}");
                if (string.IsNullOrEmpty(clipID) == false)
                {
                    //var clip = AudioModel.FindAudioByKey(clipID);
                    var clip = m_current.m_table.FindByKey(clipID).clip ?? null;
                    if (clip != null)
                    {
                        AudioControllerBase.PlayClip(AudioChannel.BGM, clip, true);
                        current.m_playingBGM = clipID;
                    }
                }
            }
            else
            {
                AudioControllerBase.Stop(AudioChannel.BGM);
            }
        }


        public static void PlaySFX(string clipID)
        {
            Debug.LogWarningFormat("<color=cyan>Try play SFX:{0}</color>", clipID);
            //var clip = AudioModel.FindClipByKey(clipID);
            var clip = m_current.m_table.FindByKey(clipID).clip;
            Debug.LogWarningFormat("find result:{0}", clip);
            if (clip != null)
            {
                AudioControllerBase.PlayClip(AudioChannel.SFX, clip);
            }
        }

        public static void SetVolume(AudioChannel target, float value)
        {
            value = Mathf.Clamp(value, 0.01f, 0.99f);
            var vol = Mathf.Log10(value) * 20;
            Debug.LogWarningFormat("Set Vol:{0} to:{1} as {2}", target, value, vol);
            //GetChannel(target).volume = value;
            Debug.Log($"instance:{current} mixer:{current?.m_masterMixer}");
            switch (target)
            {
                case AudioChannel.BGM:
                    current.m_masterMixer.SetFloat("BGMVol", vol);
                    break;
                case AudioChannel.SFX:
                    current.m_masterMixer.SetFloat("SFXVol", vol);
                    break;
                default:
                    current.m_masterMixer.SetFloat("MasterVol", value <= 0.01f ? -80f : vol);
                    break;
            }
        }

        public static void PlayClip(AudioChannel target, AudioClip clip) { PlayClip(target, clip, false); }
        public static void PlayClip(AudioChannel target, AudioClip clip, bool isLoop)
        {
            if (clip == null)
            {
                Debug.LogError($"Try PlayClip but clip is null");
                return;
            }
            var t = GetChannel(target);
            t.source.loop = isLoop;
            if (target == AudioChannel.SFX && isLoop == false)
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
                var s = DOTween.Sequence();
                s.Append(t.source.DOFade(0f, 0.5f).SetSpeedBased(true));
                s.AppendCallback(() =>
                {
                    t.source.clip = clip;
                    t.source.Play();
                });
                s.Append(t.source.DOFade(1f, 0.5f).SetSpeedBased(true));
                s.AppendCallback(() =>
                {
                    t.isBusy = false;
                    //Debug.LogWarningFormat("<color=red>2:Channel [{0}] is busy:{1}</color>", t.type, t.isBusy);
                });
#endif
                //t.clip = clip;
                //t.Play();
            }
        }

        public static async void Stop(AudioChannel target)
        {
            await StopAsync(target);
        }

        public static async Task StopAsync(AudioChannel target)
        {
            JDebug.Log($"AudioController Stop Audio stop, channel:{target.ToString()}");
            var t = GetChannel(target);
            if (t.isExist)
            {
                t.source.Stop();
                t.source.clip = null;
            }
            await Task.Yield();
        }

        public static Source GetChannel(AudioChannel channel)
        {
            if (current == null) return null;
            if (channel == AudioChannel.BGM) return current.bgm;
            else if (channel == AudioChannel.SFX) return current.sfx;
            return null;
        }
    }
}