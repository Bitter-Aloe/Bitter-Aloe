#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public class WeatherFXRain: WeatherFXInstance, IHDRPWeatherVFX
    {
        public AnimationCurve m_blendCurve = AnimationCurve.Constant(0f, 1f, 1f);
        public bool m_randomizeIntensity = true;
        public Vector2 m_rainSizeLimits = new Vector2(0.1f, 0.5f);

        private List<float> m_maxEmissionCount = new List<float>();

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

            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                if (visualEffect.m_followPlayer)
                {
                    BaseSettings.SetPlayerPosition(visualEffect.m_visualEffect, visualEffect.m_offset, "Camera_position");
                }

                BaseSettings.SetGradientColorBlend(visualEffect.m_visualEffect, "ColorBlend", m_blendCurve, BaseSettings.GetTimeOfDaySystem().ConvertTimeOfDay());
            }

            HDRPTimeOfDay timeOfDay = HDRPTimeOfDay.Instance;
            if (timeOfDay != null)
            {
                Transform player = timeOfDay.Player;
                if (player != null)
                {
                    BaseSettings.SetPauseState(player.transform.position.y < timeOfDay.TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel);
                }
            }
        }

        public void StartWeatherFX(HDRPTimeOfDayWeatherProfile profile)
        {
            BaseSettings.GetActivePlayerTransform();
            SetupParticleSystem();
            BaseSettings.SetWeatherProfile(profile);
            BaseSettings.GetOrCreateAudioSource(gameObject);
            BaseSettings.SetAudioSourceSettings(BaseSettings.GetWeatherAudioClip(), 0f, true, true);
            BaseSettings.SetDuration(0f);
            BaseSettings.ChangeState(WeatherVFXState.FadeIn);
            SyncWithWind(true);
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
            SyncWithWind(false);
        }
        public void SetVFXPlayState(bool state)
        {
            SetVFXState(state);
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

            if (!BaseSettings.m_isAdditionalEffect)
            {
                BaseSettings.SetRainPowerValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPower, BaseSettings.LerpShaderData.m_rainShaderData.m_rainPower));
                BaseSettings.SetRainPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPowerOnTerrain, BaseSettings.LerpShaderData.m_rainShaderData.m_rainPowerOnTerrain));
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
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(0f, BaseSettings.GetWeatherEffectVolume()));
            BaseSettings.SetRainPowerValue(BaseSettings.LerpFromDuration(BaseSettings.LerpShaderData.m_rainShaderData.m_rainPower, BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPower));
            BaseSettings.SetRainPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.LerpShaderData.m_rainShaderData.m_rainPowerOnTerrain, BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPowerOnTerrain));
            BlendParticleSystem(true);
            if (BaseSettings.CheckIsActive())
            {
                BaseSettings.SetNewCurrentMaxVolume(BaseSettings.GetWeatherEffectVolume());
            }
        }
        private void FadeOut()
        {
            BaseSettings.IncreaseDurationValue(Time.deltaTime / BaseSettings.GetTransitionDuration());
            BaseSettings.SetAudioVolume(BaseSettings.LerpFromDuration(BaseSettings.GetCurrentVolume(), 0f));
            BaseSettings.SetRainPowerValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPower, BaseSettings.LerpShaderData.m_rainShaderData.m_rainPower));
            BaseSettings.SetRainPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_rainShaderData.m_rainPowerOnTerrain, BaseSettings.LerpShaderData.m_rainShaderData.m_rainPowerOnTerrain));
            BlendParticleSystem(false);
        }
        private void SetupParticleSystem()
        {
            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                if (visualEffect.m_visualEffect != null)
                {
                    visualEffect.SetOverrideAsset();
                    m_maxEmissionCount.Add(BaseSettings.GetVisualEffectFloatValue(visualEffect.m_visualEffect, "ParticleAmount"));
                    BaseSettings.SetVisualEffectFloat(visualEffect.m_visualEffect, 0f, "ParticleAmount");
                    BaseSettings.ParentEffectToPlayer(visualEffect.m_visualEffect);
                    BaseSettings.SetPlayState(visualEffect.m_visualEffect, true);
                    UpdateRainIntensitySetup(UnityEngine.Random.Range(0f, 1f), visualEffect);
                }
            }
        }
        private void BlendParticleSystem(bool fadeIn)
        {
            if (VisualEffects.Count == m_maxEmissionCount.Count)
            {
                for (int i = 0; i < VisualEffects.Count; i++)
                {
                    if (fadeIn)
                    {
                        BaseSettings.SetVisualEffectFloat(VisualEffects[i].m_visualEffect, BaseSettings.LerpFromDuration(0f, m_maxEmissionCount[i]), "ParticleAmount");
                    }
                    else
                    {
                        BaseSettings.SetVisualEffectFloat(VisualEffects[i].m_visualEffect, BaseSettings.LerpFromDuration(m_maxEmissionCount[i], 0f), "ParticleAmount");
                    }
                }
            }
        }
        /// <summary>
        /// Updates the rain intensity by increasing it size and particles
        /// </summary>
        /// <param name="intensityAmount"></param>
        private void UpdateRainIntensitySetup(float intensityAmount, HDRPWeatherVisualEffect vfx)
        {
            for (int i = 0; i < VisualEffects.Count; i++)
            {
                m_maxEmissionCount[i] = Mathf.Lerp(m_maxEmissionCount[i] / 2f, m_maxEmissionCount[i] * 2f, intensityAmount);
                BaseSettings.SetVisualEffectFloat(vfx.m_visualEffect, Mathf.Lerp(m_rainSizeLimits.x, m_rainSizeLimits.y, intensityAmount), "Size");
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