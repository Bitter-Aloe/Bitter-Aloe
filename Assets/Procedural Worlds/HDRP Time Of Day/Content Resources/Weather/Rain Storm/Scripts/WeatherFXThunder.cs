#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public class WeatherFXThunder : WeatherFXInstance, IHDRPWeatherVFX
    {
        public ThunderData m_thunderData;
        public Vector2 m_thunderStrikeTime = new Vector2(5f, 10f);

        private float m_thunderTimer;
        private AudioSource m_thunderSoundFX;
        private HDRPTimeOfDay HDRPTimeOfDay;
        public float m_lightResetTimer = 0.125f;
        private float m_currentStrikeTimer = 0.05f;
        private int m_strikeCount = 0;
        private bool m_processStrikes = false;
        private bool m_waitOnTimer = false;

        private void Update()
        {
            if (BaseSettings.m_state == WeatherVFXState.InActive)
            {
                return;
            }
            else if (BaseSettings.m_state == WeatherVFXState.FadeIn)
            {
                FadeIn();
            }
            else if (BaseSettings.m_state == WeatherVFXState.FadeOut)
            {
                FadeOut();
            }
            else
            {
                Active();
            }
        }

        public void StartWeatherFX(HDRPTimeOfDayWeatherProfile profile)
        {
            HDRPTimeOfDay = BaseSettings.GetTimeOfDaySystem();
            m_thunderTimer = BaseSettings.GetRandomValue(m_thunderStrikeTime.x, m_thunderStrikeTime.y);
            BaseSettings.SetWeatherProfile(profile);
            BaseSettings.SetDuration(0f);
            BaseSettings.ChangeState(WeatherVFXState.FadeIn);
        }
        public void StopWeatherFX(float duration)
        {
            BaseSettings.SetDuration(duration);
            BaseSettings.ChangeState(WeatherVFXState.FadeOut);
        }
        public void SetDuration(float value)
        {
            BaseSettings.SetDuration(value);
        }
        public float GetCurrentDuration()
        {
            return BaseSettings.m_duration;
        }
        public void DestroyVFX()
        {
            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                visualEffect.m_visualEffect.Stop();
                StartCoroutine(visualEffect.StopVFX());
            }

            StartCoroutine(StopAndDestroyVFX());
        }
        public void SetVFXPlayState(bool state)
        {
            
        }
        public void DestroyInstantly()
        {
            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                if (visualEffect != null)
                {
                    BaseSettings.DestroyVisualEffect(visualEffect.m_visualEffect);
                }
            }
            DestroyImmediate(gameObject);
        }
        public List<VisualEffect> CanBeControlledByUnderwater()
        {
            return GetVFXThatUnderwaterCanInteractWith();
        }

        private void FadeIn()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
            if (BaseSettings.CheckIsActive())
            {
                BaseSettings.SetNewCurrentMaxVolume(BaseSettings.GetWeatherEffectVolume());
            }
        }
        private void FadeOut()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
        }
        private void Active()
        {
            m_thunderTimer -= Time.deltaTime;
            if (m_thunderTimer <= 0f)
            {
                m_thunderTimer = BaseSettings.GetRandomValue(m_thunderStrikeTime.x, m_thunderStrikeTime.y);
                if (HDRPTimeOfDay != null)
                {
                    m_processStrikes = true;
                    m_waitOnTimer = false;
                    m_currentStrikeTimer = m_lightResetTimer;
                    m_strikeCount = BaseSettings.GetRandomValue(m_thunderData.m_thunderStrikeCountMinMax.x, m_thunderData.m_thunderStrikeCountMinMax.y);
                    m_thunderSoundFX = gameObject.GetComponent<AudioSource>();
                    if (m_thunderSoundFX == null)
                    {
                        m_thunderSoundFX = gameObject.AddComponent<AudioSource>();
                    }

                    m_thunderSoundFX.loop = false;
                }
            }

            if (m_processStrikes)
            {
                if (m_strikeCount <= 0)
                {
                    m_processStrikes = false;
                }

                if (!m_waitOnTimer)
                {
                    for (int i = 0; i < m_strikeCount; i++)
                    {
                        float intensity = m_thunderData.m_intesity;
                        bool isDay = HDRPTimeOfDay.IsDayTime();
                        if (!isDay)
                        {
                            intensity = m_thunderData.m_intesity * m_thunderData.m_nightTimeIntensityMultiplier;
                        }
                
                        HDRPTimeOfDay.OverrideLightSource(m_thunderData.m_temperature, m_thunderData.m_thunderLight, intensity, m_thunderData.m_shadows, false);
                        if (i == m_strikeCount - 1)
                        {
                            HDRPTimeOfDay.PlaySoundFX(m_thunderData.m_thunderStrikeSounds[UnityEngine.Random.Range(0, m_thunderData.m_thunderStrikeSounds.Count - 1)], m_thunderSoundFX, false, m_thunderData.m_volume);
                            m_waitOnTimer = true;
                        }
                    }
                }
                else
                {
                    m_currentStrikeTimer -= Time.deltaTime;
                    if (m_currentStrikeTimer <= 0f)
                    {
                        HDRPTimeOfDay.m_lightSourceOverride = false;
                        HDRPTimeOfDay.SetCurrentIncrimentalFrameValue(HDRPTimeOfDay.IncrementalFrameCount + 1);
                        HDRPTimeOfDay.ProcessWeather(true);
                        m_processStrikes = false;
                        m_waitOnTimer = false;
                        HDRPTimeOfDay.RefreshLights();
                    }
                }
            }
        }
        private IEnumerator StopAndDestroyVFX()
        {
            while (true)
            {
                foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
                {
                    if (visualEffect != null)
                    {
                        if (visualEffect.ReadyToBeDestroyed())
                        {
                            BaseSettings.DestroyVisualEffect(visualEffect.m_visualEffect);
                        }
                    }
                }

                if (AllVisualEffectsDestroyed())
                {
                    DestroyImmediate(gameObject);
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
#endif