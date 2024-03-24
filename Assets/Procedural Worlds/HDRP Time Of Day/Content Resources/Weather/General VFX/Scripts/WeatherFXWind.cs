#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public class WeatherFXWind : WeatherFXInstance, IHDRPWeatherVFX
    {
        public float m_maxWindSpeed = 1f;

        private bool m_windZoneFound = false;
        private float m_currentWindSpeed = 0f;

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
                WindZone.windMain = m_maxWindSpeed;
            }
        }

        public void StartWeatherFX(HDRPTimeOfDayWeatherProfile profile)
        {
            BaseSettings.SetWeatherProfile(profile);
            BaseSettings.GetActivePlayerTransform();
            BaseSettings.GetOrCreateAudioSource(gameObject);
            BaseSettings.SetAudioSourceSettings(null, 0f, true, true);
            BaseSettings.SetDuration(0f);
            BaseSettings.ChangeState(WeatherVFXState.FadeIn);
            m_windZoneFound = GetWindZone(true, out m_currentWindSpeed);
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
        public void DestroyInstantly()
        {
            WindZone.windMain = m_currentWindSpeed;
            DestroyImmediate(gameObject);
        }
        public void SetVFXPlayState(bool state)
        {
            
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
        public List<VisualEffect> CanBeControlledByUnderwater()
        {
            return GetVFXThatUnderwaterCanInteractWith();
        }

        private void FadeIn()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(0f, BaseSettings.GetWeatherEffectVolume()));
            if (m_windZoneFound)
            {
                WindZone.windMain = BaseSettings.LerpFromDuration(m_currentWindSpeed, m_maxWindSpeed);
            }
            if (BaseSettings.CheckIsActive())
            {
                BaseSettings.SetNewCurrentMaxVolume(BaseSettings.GetWeatherEffectVolume());
            }
        }
        private void FadeOut()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(BaseSettings.GetCurrentVolume(), 0f));
            if (m_windZoneFound)
            {
                WindZone.windMain = BaseSettings.LerpFromDuration(m_maxWindSpeed, m_currentWindSpeed);
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