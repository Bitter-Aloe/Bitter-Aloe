using UnityEngine;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
namespace ProceduralWorlds.HDRPTOD
{
    [System.Serializable]
    public class TimeOfDayPostFXProfileData
    {
#if HDPipeline
        public GeneralQuality m_postProcessingQuality = GeneralQuality.Medium;
#endif
        public bool m_colorGradingSettings = false;
        public bool m_bloomSettings = false;
        public bool m_shadowToningSettings = false;
        public bool m_vignetteSettings = false;
        public bool m_ambientOcclusionSettings = false;
        public bool m_depthOfFieldSettings = false;
        public bool m_useColorGrading = true;
        public bool m_useBloom = true;
        public bool m_useShadowToning = true;
        public bool m_useVignette = true;
        public bool m_useAmbientOcclusion = true;
        public bool m_useDepthOfField = true;
        public AnimationCurve m_contrast;
        public Gradient m_colorFilter;
        public AnimationCurve m_saturation;
        public AnimationCurve m_temperature;
        public AnimationCurve m_tint;
        public AnimationCurve m_bloomThreshold;
        public AnimationCurve m_bloomIntensity;
        public AnimationCurve m_bloomScatter;
        public Gradient m_bloomTint;
        public Gradient m_shadows;
        public Gradient m_highlights;
        public AnimationCurve m_shadowBalance;
        public Gradient m_vignetteColor;
        public AnimationCurve m_vignetteIntensity;
        public AnimationCurve m_vignetteSmoothness;
        public AnimationCurve m_ambientIntensity;
        public AnimationCurve m_ambientDirectStrength;
        public AnimationCurve m_ambientRadius;
#if HDPipeline
        public DepthOfFieldMode m_depthOfFieldMode = DepthOfFieldMode.Manual;
#endif
        public bool m_physicallyBased = false;
        public bool m_useGaiaAutoDOF = true;
        public float m_nearBlurMultiplier = 1f;
        public AnimationCurve m_nearBlurStart;
        public AnimationCurve m_nearBlurEnd;
        public float m_farBlurMultiplier = 1f;
        public AnimationCurve m_farBlurStart;
        public AnimationCurve m_farBlurEnd;
        #region Validate Functions
        public bool ValidateAmbientOcclusion()
        {
            if (m_ambientIntensity == null)
            {
                m_ambientIntensity = AnimationCurve.Constant(0f, 1f, 0.85f);
                return false;
            }
            if (m_ambientDirectStrength == null)
            {
                m_ambientDirectStrength = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            if (m_ambientRadius == null)
            {
                m_ambientDirectStrength = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            return true;
        }
        public bool ValidateBloom()
        {
            if (m_bloomThreshold == null)
            {
                m_bloomThreshold = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            if (m_bloomIntensity == null)
            {
                m_bloomIntensity = AnimationCurve.Constant(0f, 1f, 0.5f);
                return false;
            }
            if (m_bloomScatter == null)
            {
                m_bloomScatter = AnimationCurve.Constant(0f, 1f, 0.5f);
                return false;
            }
            if (m_bloomTint == null)
            {
                m_bloomTint = new Gradient();
                return false;
            }
            return true;
        }
        public bool ValidateColorGrading()
        {
            if (m_contrast == null)
            {
                m_contrast = AnimationCurve.Constant(0f, 1f, 10f);
                return false;
            }
            if (m_colorFilter == null)
            {
                m_colorFilter = new Gradient();
                return false;
            }
            if (m_saturation == null)
            {
                m_saturation = AnimationCurve.Constant(0f, 1f, 10f);
                return false;
            }
            if (m_temperature == null)
            {
                m_temperature = AnimationCurve.Constant(0f, 1f, 0f);
                return false;
            }
            if (m_tint == null)
            {
                m_tint = AnimationCurve.Constant(0f, 1f, 0f);
                return false;
            }
            return true;
        }
        public bool ValidateDepthOfField()
        {
            if (m_nearBlurStart == null)
            {
                m_nearBlurStart = AnimationCurve.Constant(0f, 1f, 0f);
                return false;
            }
            if (m_nearBlurEnd == null)
            {
                m_nearBlurEnd = AnimationCurve.Constant(0f, 1f, 4f);
                return false;
            }
            if (m_farBlurStart == null)
            {
                m_farBlurStart = AnimationCurve.Constant(0f, 1f, 10f);
                return false;
            }
            if (m_farBlurEnd == null)
            {
                m_farBlurEnd = AnimationCurve.Constant(0f, 1f, 20f);
                return false;
            }
            return true;
        }
        public bool ValidateShadowToning()
        {
            if (m_shadows == null)
            {
                m_shadows = new Gradient();
                return false;
            }
            if (m_highlights == null)
            {
                m_highlights = new Gradient();
                return false;
            }
            if (m_shadowBalance == null)
            {
                m_shadowBalance = AnimationCurve.Constant(0f, 1f, 0f);
                return false;
            }
            return true;
        }
        public bool ValidateVignette()
        {
            if (m_vignetteColor == null)
            {
                m_vignetteColor = new Gradient();
                return false;
            }
            if (m_vignetteIntensity == null)
            {
                m_vignetteIntensity = AnimationCurve.Constant(0f, 1f, 0.25f);
                return false;
            }
            if (m_vignetteSmoothness == null)
            {
                m_vignetteSmoothness = AnimationCurve.Constant(0f, 1f, 0.6f);
                return false;
            }
            return true;
        }
        #endregion
        #region Apply Functions
#if HDPipeline
        public void ApplyColorGradingSettings(ColorAdjustments colorAdjustments, WhiteBalance whiteBalance, float time)
        {
            colorAdjustments.active = m_useColorGrading;
            colorAdjustments.contrast.value = m_contrast.Evaluate(time);
            colorAdjustments.colorFilter.value = m_colorFilter.Evaluate(time);
            colorAdjustments.saturation.value = m_saturation.Evaluate(time);
            whiteBalance.active = m_useColorGrading;
            whiteBalance.temperature.value = m_temperature.Evaluate(time);
            whiteBalance.tint.value = m_tint.Evaluate(time);
        }
        public void ApplyBloomSettings(Bloom bloom, float time)
        {
            bloom.active = m_useBloom;
            bloom.threshold.value = m_bloomThreshold.Evaluate(time);
            bloom.intensity.value = m_bloomIntensity.Evaluate(time);
            bloom.scatter.value = m_bloomScatter.Evaluate(time);
            bloom.tint.value = m_bloomTint.Evaluate(time);
            bloom.quality.value = (int)m_postProcessingQuality;
        }
        public void ApplyShadowToningSettings(SplitToning splitToning, float time)
        {
            splitToning.active = m_useShadowToning;
            splitToning.shadows.value = m_shadows.Evaluate(time);
            splitToning.highlights.value = m_highlights.Evaluate(time);
            splitToning.balance.value = m_shadowBalance.Evaluate(time);
        }
        public void ApplyVignetteSettings(Vignette vignette, float time)
        {
            vignette.active = m_useVignette;
            vignette.color.value = m_vignetteColor.Evaluate(time);
            vignette.intensity.value = m_vignetteIntensity.Evaluate(time);
            vignette.smoothness.value = m_vignetteSmoothness.Evaluate(time);
        }
#if UNITY_2022_2_OR_NEWER
        public void ApplyAmbientOcclusion(ScreenSpaceAmbientOcclusion ambientOcclusion, float time)
#else
        public void ApplyAmbientOcclusion(AmbientOcclusion ambientOcclusion, float time)
#endif
        {
            ambientOcclusion.active = m_useAmbientOcclusion;
            ambientOcclusion.intensity.value = m_ambientIntensity.Evaluate(time);
            ambientOcclusion.directLightingStrength.value = m_ambientDirectStrength.Evaluate(time);
            ambientOcclusion.radius.value = m_ambientRadius.Evaluate(time);
            ambientOcclusion.quality.value = (int)m_postProcessingQuality;
        }
        public void ApplyDepthOfField(DepthOfField depthOfField, float time)
        {
            depthOfField.active = m_useDepthOfField;
            depthOfField.focusMode.value = m_depthOfFieldMode;
            depthOfField.nearFocusStart.value = m_nearBlurStart.Evaluate(time) * m_nearBlurMultiplier;
            depthOfField.nearFocusEnd.value = m_nearBlurEnd.Evaluate(time) * m_nearBlurMultiplier;
            depthOfField.farFocusStart.value = m_farBlurStart.Evaluate(time) * m_farBlurMultiplier;
            depthOfField.farFocusEnd.value = m_farBlurEnd.Evaluate(time) * m_farBlurMultiplier;
#if GAIA_PRO_PRESENT
#endif
        }
        public void SetDepthOfFieldQuality(DepthOfField depthOfField)
        {
            depthOfField.quality.value = 3;
            switch (m_postProcessingQuality)
            {
                case GeneralQuality.Low:
                {
                    depthOfField.nearSampleCount = 3;
                    depthOfField.nearMaxBlur = 2f;
                    depthOfField.farSampleCount = 4;
                    depthOfField.farMaxBlur = 5f;
                    depthOfField.resolution = DepthOfFieldResolution.Quarter;
                    depthOfField.highQualityFiltering = false;
                    break;
                }
                case GeneralQuality.Medium:
                {
                    depthOfField.nearSampleCount = 4;
                    depthOfField.nearMaxBlur = 3f;
                    depthOfField.farSampleCount = 5;
                    depthOfField.farMaxBlur = 6f;
                    depthOfField.resolution = DepthOfFieldResolution.Half;
                    depthOfField.highQualityFiltering = false;
                    break;
                }
                default:
                {
                    depthOfField.nearSampleCount = 5;
                    depthOfField.nearMaxBlur = 4f;
                    depthOfField.farSampleCount = 7;
                    depthOfField.farMaxBlur = 8f;
                    depthOfField.resolution = DepthOfFieldResolution.Full;
                    depthOfField.highQualityFiltering = true;
                    break;
                }
            }
            depthOfField.physicallyBased = m_physicallyBased;
        }
#endif
        #endregion
    }
    public class HDRPTimeOfDayPostFXProfile : ScriptableObject
    {
        public TimeOfDayPostFXProfileData TimeOfDayPostFXData
        {
            get { return m_timeOfDayPostFXData; }
            set
            {
                if (m_timeOfDayPostFXData != value)
                {
                    m_timeOfDayPostFXData = value;
                }
            }
        }
        [SerializeField] private TimeOfDayPostFXProfileData m_timeOfDayPostFXData = new TimeOfDayPostFXProfileData();
    }
}