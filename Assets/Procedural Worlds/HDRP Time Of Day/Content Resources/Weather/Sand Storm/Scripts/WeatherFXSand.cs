#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public class WeatherFXSand: WeatherFXInstance, IHDRPWeatherVFX
    {
        private List<float> m_maxEmissionCount = new List<float>();
        public AnimationCurve m_blendCurve = AnimationCurve.Constant(0f, 1f, 1f);

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

                if (visualEffect.m_setPlayerYPosition)
                {
                    BaseSettings.SetPlayerYPosition(visualEffect.m_visualEffect, "HeightPos");
                }

                BaseSettings.SetGradientColorBlend(visualEffect.m_visualEffect, "ColorBlend", m_blendCurve, BaseSettings.GetTimeOfDaySystem().ConvertTimeOfDay());
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
        public void SetVFXPlayState(bool state)
        {
            SetVFXState(state);
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
                BaseSettings.SetSandPowerValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPower, BaseSettings.LerpShaderData.m_sandShaderData.m_sandPower));
                BaseSettings.SetSandPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPowerOnTerrain, BaseSettings.LerpShaderData.m_sandShaderData.m_sandPowerOnTerrain));
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
            BaseSettings.SetSandPowerValue(BaseSettings.LerpFromDuration(BaseSettings.LerpShaderData.m_sandShaderData.m_sandPower, BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPower));
            BaseSettings.SetSandPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.LerpShaderData.m_sandShaderData.m_sandPowerOnTerrain, BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPowerOnTerrain));
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
            BaseSettings.SetSandPowerValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPower, BaseSettings.LerpShaderData.m_sandShaderData.m_sandPower));
            BaseSettings.SetSandPowerOnTerrainValue(BaseSettings.LerpFromDuration(BaseSettings.m_weatherProfile.WeatherShaderData.m_sandShaderData.m_sandPowerOnTerrain, BaseSettings.LerpShaderData.m_sandShaderData.m_sandPowerOnTerrain));
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