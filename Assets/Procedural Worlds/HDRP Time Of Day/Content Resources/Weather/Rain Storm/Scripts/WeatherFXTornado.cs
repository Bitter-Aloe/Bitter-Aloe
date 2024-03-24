#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public class WeatherFXTornado : WeatherFXInstance, IHDRPWeatherVFX
    {
        public GameObject m_tornadoVFX;
        public Vector2 m_changeDirectionMinMaxTime = new Vector2(45f, 80f);
        public float m_tornadoSpeed = 0.2f;
        public Vector2 m_startPointOnDuration = new Vector2(0.4f, 0.8f);
        [Range(0f, 1f)]
        public float m_volume = 1f;

        private float m_changeDirectionTimer = 0f;
        private Vector3 m_currentDirection = Vector3.zero;
        private bool m_vfxPlayingOrStopping = false;
        private float m_startStopValue;

        [SerializeField] private Vector2 RandomValue = new Vector2(-300f, 300f);

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
            BaseSettings.SetWeatherProfile(profile);
            BaseSettings.SetDuration(0f);
            BaseSettings.GetActivePlayerTransform();
            BaseSettings.GetOrCreateAudioSource(gameObject);
            BaseSettings.SetAudioSourceSettings(null, 0f, true, false);
            BaseSettings.ChangeState(WeatherVFXState.FadeIn);
            m_vfxPlayingOrStopping = false;
            m_startStopValue = BaseSettings.GetRandomValue(m_startPointOnDuration.x, m_startPointOnDuration.y);
            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                BaseSettings.SetPlayState(visualEffect.m_visualEffect, false);
            }
            m_changeDirectionTimer = UnityEngine.Random.Range(m_changeDirectionMinMaxTime.x, m_changeDirectionMinMaxTime.y);
            m_currentDirection = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, UnityEngine.Random.Range(-10f, 10f));
            if (BaseSettings.m_player != null)
            {
                if (m_tornadoVFX != null)
                {
                    m_tornadoVFX.transform.localPosition = BaseSettings.GetRandomPosition(RandomValue, true, 150f);
                }
            }
        }
        public void StopWeatherFX(float duration)
        {
            m_vfxPlayingOrStopping = false;
            m_startStopValue = BaseSettings.GetRandomValue(m_startPointOnDuration.x, m_startPointOnDuration.y);
            BaseSettings.SetDuration(duration);
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(0f, BaseSettings.GetWeatherEffectVolume()));
            BaseSettings.ChangeState(WeatherVFXState.FadeOut);
        }
        public void SetVFXPlayState(bool state)
        {
            SetVFXState(state);
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
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(0f, m_volume));
            if (BaseSettings.CheckIsActive())
            {
                BaseSettings.SetNewCurrentMaxVolume(BaseSettings.GetWeatherEffectVolume());
            }

            if (BaseSettings.m_duration >= m_startStopValue && !m_vfxPlayingOrStopping)
            {
                m_vfxPlayingOrStopping = true;
                BaseSettings.SetAudioSourceSettings(null, BaseSettings.LerpFromDuration(0f, BaseSettings.GetWeatherEffectVolume()), true, true);
                foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
                {
                    BaseSettings.SetPlayState(visualEffect.m_visualEffect, true);
                }
            }
        }
        private void FadeOut()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(m_volume, 0f));
            if (BaseSettings.m_duration >= m_startStopValue && !m_vfxPlayingOrStopping)
            {
                m_vfxPlayingOrStopping = true;
                foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
                {
                    BaseSettings.SetPlayState(visualEffect.m_visualEffect, false);
                }
            }
        }
        private void Active()
        {
            if (m_tornadoVFX == null)
            {
                return;
            }

            m_changeDirectionTimer -= Time.deltaTime;
            if (m_changeDirectionTimer <= 0f)
            {
                m_changeDirectionTimer = UnityEngine.Random.Range(m_changeDirectionMinMaxTime.x, m_changeDirectionMinMaxTime.y);
                if (BaseSettings.m_player != null)
                {
                    m_currentDirection = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, UnityEngine.Random.Range(-10f, 10f));
                }
            }

            Vector3 direction = m_currentDirection * (Time.deltaTime * m_tornadoSpeed);
            direction.y = m_tornadoVFX.transform.localPosition.y;
            m_tornadoVFX.transform.localPosition += direction;
            m_tornadoVFX.transform.localPosition = new Vector3(m_tornadoVFX.transform.localPosition.x, BaseSettings.GetGroundPosition(m_tornadoVFX.transform.localPosition, 100f), m_tornadoVFX.transform.localPosition.z);
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