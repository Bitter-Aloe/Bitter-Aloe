using UnityEngine;
using UnityEngine.Audio;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDayAmbientProfile : ScriptableObject
    {
        //Public
        public AudioMixerGroup m_audioMixer;
        public AudioClip m_morningAmbient;
        public AudioClip m_afternoonAmbient;
        public AudioClip m_eveningAmbient;
        public AudioClip m_nightAmbient;
        public float m_masterVolume = 0.55f;
        public float m_weatherVolumeMultiplier = 0.25f;
        public Vector4 m_timeOfDayIntervals = new Vector4(0f, 0.25f, 0.5f, 0.75f);
        public float m_audioTrackingRange = 300f;
        //Private
        private bool m_useAudioObjectTracking = false;
        private GameObject m_trackingGameObject;
#if HDPipeline && UNITY_2021_2_OR_NEWER
        private bool m_validated = false;

        /// <summary>
        /// Processes the ambient audio blending
        /// </summary>
        /// <param name="sourceActive"></param>
        /// <param name="sourceBlend"></param>
        /// <param name="time"></param>
        public bool ProcessAmbientAudio(AudioSource sourceActive, AudioSource sourceBlend, float time, float blendTime, bool initilized, HDRPTimeOfDay timeOfDay)
        {
            if (!m_validated)
            {
                m_validated = Validate();
            }

            float maxVolume = m_masterVolume;
            if (m_useAudioObjectTracking && m_trackingGameObject != null)
            {
                if (m_trackingGameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(m_trackingGameObject.transform.position, timeOfDay.Player.position);
                    float range = Mathf.InverseLerp(0f, m_audioTrackingRange, distance);
                    if (range >= 1f)
                    {
                        maxVolume = 0f;
                    }
                    else
                    {
                        maxVolume = Mathf.Lerp(m_masterVolume, 0f, range);
                    }
                }
                else
                {
                    maxVolume = 0f;
                }
            }

            sourceActive.outputAudioMixerGroup = m_audioMixer;
            sourceBlend.outputAudioMixerGroup = m_audioMixer;
            bool reset = false;
            if (m_validated)
            {
                GetCurrentClips(time, out AudioClip clipA, out AudioClip clipB);
                if (sourceActive.clip != clipA)
                {
                    sourceActive.Stop();
                    sourceActive.clip = clipA;
                    sourceActive.Play();
                    if (!initilized)
                    {
                        sourceActive.volume = maxVolume;
                    }
                    else
                    {
                        sourceActive.volume = 0f;
                        reset = true;
                    }
                }

                if (timeOfDay.WeatherActive())
                {
                    sourceActive.volume = Mathf.Lerp(0f, maxVolume * m_weatherVolumeMultiplier, blendTime);
                }
                else
                {
                    sourceActive.volume = Mathf.Lerp(0f, maxVolume, blendTime);
                }

                if (sourceBlend.clip != clipB)
                {
                    sourceBlend.Stop();
                    sourceBlend.clip = clipB;
                    sourceBlend.Play();
                    if (!initilized)
                    {
                        sourceActive.volume = 0f;
                    }
                    else
                    {
                        sourceActive.volume = maxVolume;
                        reset = true;
                    }
                }

                if (timeOfDay.WeatherActive())
                {
                    sourceBlend.volume = Mathf.Lerp(maxVolume * m_weatherVolumeMultiplier, 0f, blendTime);
                }
                else
                {
                    sourceBlend.volume = Mathf.Lerp(maxVolume, 0f, blendTime);
                }
            }

            return reset;
        }
#endif

        /// <summary>
        /// Updates the object tracking
        /// </summary>
        /// <param name="value"></param>
        /// <param name="trackingObject"></param>
        public void UpdateObjectTracking(bool value, GameObject trackingObject)
        {
            m_useAudioObjectTracking = value;
            m_trackingGameObject = trackingObject;
        }
        /// <summary>
        /// Gets the clips based on the time of day
        /// </summary>
        /// <param name="time"></param>
        /// <param name="clipA"></param>
        /// <param name="clipB"></param>
        private void GetCurrentClips(float time, out AudioClip clipA, out AudioClip clipB)
        {
            if (time >= m_timeOfDayIntervals.x && time < m_timeOfDayIntervals.y)
            {
                clipA = m_morningAmbient;
                clipB = m_nightAmbient;
            }
            else if (time >= m_timeOfDayIntervals.y && time < m_timeOfDayIntervals.z)
            {
                clipA = m_afternoonAmbient;
                clipB = m_morningAmbient;
            }
            else if (time >= m_timeOfDayIntervals.z && time < m_timeOfDayIntervals.w)
            {
                clipA = m_eveningAmbient;
                clipB = m_afternoonAmbient;
            }
            else
            {
                clipA = m_nightAmbient;
                clipB = m_eveningAmbient;
            }
        }
        /// <summary>
        /// Validates that the clips are present
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            if (m_audioMixer == null)
            {
                Debug.LogWarning("audio mixer is null, this can be left null but it's recommended to use a audio mixer to control the volume independently.");
            }
            if (m_morningAmbient == null)
            {
                Debug.LogWarning("Morning clip is null");
                return false;
            }
            if (m_afternoonAmbient == null)
            {
                Debug.LogWarning("Afternoon clip is null");
                return false;
            }
            if (m_eveningAmbient == null)
            {
                Debug.LogWarning("Evening clip is null");
                return false;
            }
            if (m_nightAmbient == null)
            {
                Debug.LogWarning("Night clip is null");
                return false;
            }

            return true;
        }
    }
}