using System;
using UnityEngine;
#if HDRPTIMEOFDAY && HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace ProceduralWorlds.HDRPTOD
{
    [Serializable]
    public class CachedProceduralCloudSettings
    {
        public bool CachedHasBeenSet = false;
        public float Opacity;
        [Header("Layers")]
        public float LayerAOpacity;
        public float LayerBOpacity;
        [Header("Lighting")]
        public Color TintColor;
        public float Exposure;
        [Header("Shadowing")]
        public float ShadowOpacity;
        public Color ShadowColor;

        /// <summary>
        /// Copies the settings from volumetric cloud component
        /// </summary>
        /// <param name="volumetricClouds"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void SetNewSettings(CloudLayer proceduralClouds, CloudLayerChannelMode layerModeA, CloudLayerChannelMode layerModeB)
        {
            CachedHasBeenSet = true;
            Opacity = proceduralClouds.opacity.value;          
            //Layers
            switch (layerModeA)
            {
                case CloudLayerChannelMode.R:
                {
                    LayerAOpacity = proceduralClouds.layerA.opacityR.value;
                    break;
                }
                case CloudLayerChannelMode.G:
                {
                    LayerAOpacity = proceduralClouds.layerA.opacityG.value;
                    break;
                }
                case CloudLayerChannelMode.B:
                {
                    LayerAOpacity = proceduralClouds.layerA.opacityB.value;
                    break;
                }
                case CloudLayerChannelMode.A:
                {
                    LayerAOpacity = proceduralClouds.layerA.opacityA.value;
                    break;
                }
            }
            switch (layerModeB)
            {
                case CloudLayerChannelMode.R:
                {
                    LayerBOpacity = proceduralClouds.layerB.opacityR.value;
                    break;
                }
                case CloudLayerChannelMode.G:
                {
                    LayerBOpacity = proceduralClouds.layerB.opacityG.value;
                    break;
                }
                case CloudLayerChannelMode.B:
                {
                    LayerBOpacity = proceduralClouds.layerB.opacityB.value;
                    break;
                }
                case CloudLayerChannelMode.A:
                {
                    LayerBOpacity = proceduralClouds.layerB.opacityA.value;
                    break;
                }
            }
            TintColor = Color.white;
            Exposure = proceduralClouds.layerA.exposure.value;
            ShadowOpacity = proceduralClouds.shadowMultiplier.value;
            ShadowColor = proceduralClouds.shadowTint.value;
        }
#endif
    }

    /// <summary>
    /// Volumetric Cloud Preset
    /// </summary>
    [CreateAssetMenu(fileName = "HDRP TOD Procedural Cloud Preset", menuName = "Procedural Worlds/HDRP Time Of Day/Profiles/HDRP TOD Procedural Cloud Preset")]
    public class HDRPTimeOfDayProceduralCloudPreset : ScriptableObject
    {
        [Header("Global")]
        public AnimationCurve Opacity = new AnimationCurve(new Keyframe(0f, 0.9f), new Keyframe(0.18f, 0.9f), new Keyframe(0.2f, 0.55f), new Keyframe(0.25f, 0.55f), new Keyframe(0.3f, 0.9f), new Keyframe(0.65f, 0.9f), new Keyframe(0.75f, 0.55f), new Keyframe(0.8f, 0.55f), new Keyframe(0.82f, 0.9f), new Keyframe(1f, 0.9f));
#if HDRPTIMEOFDAY && HDPipeline
        public CloudMapMode Layers = CloudMapMode.Double;
#endif
#if HDRPTIMEOFDAY && HDPipeline
        public CloudResolution Resolution = CloudResolution.Resolution1024;
#endif
        [Header("Layers")]
#if HDRPTIMEOFDAY && HDPipeline
        public CloudLayerChannelMode LayerAChannel = CloudLayerChannelMode.R;
#endif
        public AnimationCurve LayerAOpacity;
#if HDRPTIMEOFDAY && HDPipeline
        public CloudLayerChannelMode LayerBChannel = CloudLayerChannelMode.B;
#endif
        public AnimationCurve LayerBOpacity;
        [Header("Lighting")]
        public bool Lighting = true;
        public Gradient TintColor = new Gradient();
        public AnimationCurve Exposure = AnimationCurve.Constant(0f, 1f, 1f);
        [Header("Shadowing")]
        public bool EnableShadows = false;
        public AnimationCurve ShadowOpacity = new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(0.5f, 0.25f), new Keyframe(1f, 0.35f));
        public Gradient ShadowColor = new Gradient();

        /// <summary>
        /// Cached cloud settings to blend with
        /// </summary>
        private CachedProceduralCloudSettings m_cachedSettings = new CachedProceduralCloudSettings();

        /// <summary>
        /// Applies the procedural cloud settings
        /// </summary>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(CloudLayer clouds, float time)
        {
            if (clouds != null)
            {
                //Global
                clouds.opacity.value = Opacity.Evaluate(time);
                clouds.layers.value = Layers;
                switch (Resolution)
                {
                    case CloudResolution.Resolution256:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution256;
                        break;
                    }
                    case CloudResolution.Resolution512:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution512;
                        break;
                    }
                    case CloudResolution.Resolution1024:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution1024;
                        break;
                    }
                    case CloudResolution.Resolution2048:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution2048;
                        break;
                    }
                    case CloudResolution.Resolution4096:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution4096;
                        break;
                    }
                    case CloudResolution.Resolution8192:
                    {
                        clouds.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution8192;
                        break;
                    }
                }
                //Layers
                switch (LayerAChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        clouds.layerA.opacityR.value = LayerAOpacity.Evaluate(time);
                        clouds.layerA.opacityG.value = 0f;
                        clouds.layerA.opacityB.value = 0f;
                        clouds.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        clouds.layerA.opacityG.value = LayerAOpacity.Evaluate(time);
                        clouds.layerA.opacityR.value = 0f;
                        clouds.layerA.opacityB.value = 0f;
                        clouds.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        clouds.layerA.opacityB.value = LayerAOpacity.Evaluate(time);
                        clouds.layerA.opacityR.value = 0f;
                        clouds.layerA.opacityG.value = 0f;
                        clouds.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        clouds.layerA.opacityA.value = LayerAOpacity.Evaluate(time);
                        clouds.layerA.opacityR.value = 0f;
                        clouds.layerA.opacityG.value = 0f;
                        clouds.layerA.opacityB.value = 0f;
                        break;
                    }
                }
                switch (LayerBChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        clouds.layerB.opacityR.value = LayerBOpacity.Evaluate(time);
                        clouds.layerB.opacityG.value = 0f;
                        clouds.layerB.opacityB.value = 0f;
                        clouds.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        clouds.layerB.opacityG.value = LayerBOpacity.Evaluate(time);
                        clouds.layerB.opacityR.value = 0f;
                        clouds.layerB.opacityB.value = 0f;
                        clouds.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        clouds.layerB.opacityB.value = LayerBOpacity.Evaluate(time);
                        clouds.layerB.opacityR.value = 0f;
                        clouds.layerB.opacityG.value = 0f;
                        clouds.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        clouds.layerB.opacityA.value = LayerBOpacity.Evaluate(time);
                        clouds.layerB.opacityR.value = 0f;
                        clouds.layerB.opacityG.value = 0f;
                        clouds.layerB.opacityB.value = 0f;
                        break;
                    }
                }
                //Lighting
                clouds.layerA.lighting.value = Lighting;
                clouds.layerB.lighting.value = Lighting;
                clouds.layerA.tint.value = Color.white;
                clouds.layerB.tint.value = Color.white;
                clouds.layerA.exposure.value = Exposure.Evaluate(time);
                clouds.layerB.exposure.value = Exposure.Evaluate(time);
                //Shadowing
                clouds.layerA.castShadows.value = EnableShadows;
                clouds.layerB.castShadows.value = EnableShadows;
                clouds.shadowMultiplier.value = ShadowOpacity.Evaluate(time);
                clouds.shadowTint.value = ShadowColor.Evaluate(time);
            }
        }
        /// <summary>
        /// Blends the current settings to the profile settings
        /// </summary>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
        /// <param name="lerpTime"></param>
        public void Blend(CloudLayer clouds, float time, float lerpTime)
        {
            if (!m_cachedSettings.CachedHasBeenSet)
            {
                Debug.LogWarning("Cache volumetric settings has not been setup. Cloud blending with presets will not be proceed when you call Blend().");
                return;
            }

            //Global
            clouds.opacity.value = Mathf.Lerp(clouds.opacity.value, Opacity.Evaluate(time), lerpTime);
            //Layers
            switch (LayerAChannel)
            {
                case CloudLayerChannelMode.R:
                {
                    clouds.layerA.opacityR.value = Mathf.Lerp(clouds.layerA.opacityR.value, LayerAOpacity.Evaluate(time), lerpTime);
                    clouds.layerA.opacityG.value = 0f;
                    clouds.layerA.opacityB.value = 0f;
                    clouds.layerA.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.G:
                {
                    clouds.layerA.opacityG.value = Mathf.Lerp(clouds.layerA.opacityG.value, LayerAOpacity.Evaluate(time), lerpTime);
                    clouds.layerA.opacityR.value = 0f;
                    clouds.layerA.opacityB.value = 0f;
                    clouds.layerA.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.B:
                {
                    clouds.layerA.opacityB.value = Mathf.Lerp(clouds.layerA.opacityB.value, LayerAOpacity.Evaluate(time), lerpTime);
                    clouds.layerA.opacityR.value = 0f;
                    clouds.layerA.opacityG.value = 0f;
                    clouds.layerA.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.A:
                {
                    clouds.layerA.opacityA.value = Mathf.Lerp(clouds.layerA.opacityA.value, LayerAOpacity.Evaluate(time), lerpTime);
                    clouds.layerA.opacityR.value = 0f;
                    clouds.layerA.opacityG.value = 0f;
                    clouds.layerA.opacityB.value = 0f;
                    break;
                }
            }
            switch (LayerBChannel)
            {
                case CloudLayerChannelMode.R:
                {
                    clouds.layerB.opacityR.value = Mathf.Lerp(clouds.layerB.opacityR.value, LayerBOpacity.Evaluate(time), lerpTime);
                    clouds.layerB.opacityG.value = 0f;
                    clouds.layerB.opacityB.value = 0f;
                    clouds.layerB.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.G:
                {
                    clouds.layerB.opacityG.value = Mathf.Lerp(clouds.layerB.opacityG.value, LayerBOpacity.Evaluate(time), lerpTime);
                    clouds.layerB.opacityR.value = 0f;
                    clouds.layerB.opacityB.value = 0f;
                    clouds.layerB.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.B:
                {
                    clouds.layerB.opacityB.value = Mathf.Lerp(clouds.layerB.opacityB.value, LayerBOpacity.Evaluate(time), lerpTime);
                    clouds.layerB.opacityR.value = 0f;
                    clouds.layerB.opacityG.value = 0f;
                    clouds.layerB.opacityA.value = 0f;
                    break;
                }
                case CloudLayerChannelMode.A:
                {
                    clouds.layerB.opacityA.value = Mathf.Lerp(clouds.layerB.opacityA.value, LayerBOpacity.Evaluate(time), lerpTime);
                    clouds.layerB.opacityR.value = 0f;
                    clouds.layerB.opacityG.value = 0f;
                    clouds.layerB.opacityB.value = 0f;
                    break;
                }
            }
            //Lighting
            clouds.layerA.exposure.value = Mathf.Lerp(clouds.layerA.exposure.value, Exposure.Evaluate(time), lerpTime);
            clouds.layerB.exposure.value = Mathf.Lerp(clouds.layerB.exposure.value, Exposure.Evaluate(time), lerpTime);
            //Shadowing
            clouds.shadowMultiplier.value = Mathf.Lerp(clouds.shadowMultiplier.value, ShadowOpacity.Evaluate(time), lerpTime);
            clouds.shadowTint.value = Color.Lerp(clouds.shadowTint.value, ShadowColor.Evaluate(time), lerpTime);
        }
        /// <summary>
        /// Assigns the new cached settings
        /// This need to be set for a successful cloud blend if you are using the Blend function
        /// </summary>
        /// <param name="volumetricClouds"></param>
        public void SetCachedCloudSettings(CloudLayer proceduralClouds, CloudLayerChannelMode layerModeA, CloudLayerChannelMode layerModeB)
        {
            m_cachedSettings.SetNewSettings(proceduralClouds, layerModeA, layerModeB);
        }
#endif
    }
}