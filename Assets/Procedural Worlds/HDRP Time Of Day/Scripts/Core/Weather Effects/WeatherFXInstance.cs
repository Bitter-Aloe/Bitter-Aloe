#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
#if GTS_PRESENT
using ProceduralWorlds.GTS;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ProceduralWorlds.HDRPTOD
{
    public enum VFXVectorType { Vector2, Vector3, Vector4 }
    [System.Serializable]
    public class HDRPWeatherBaseSettings
    {
        public Transform m_player;
        public HDRPTimeOfDayWeatherProfile m_weatherProfile;
        public AudioSource m_audioSource;
        public WeatherVFXState m_state = WeatherVFXState.InActive;
        public bool m_syncWind = false;
        [Range(0f, 1f)]
        public float m_currentMaxVolume = 0.5f;
        [Range(0f, 1f)]
        public float m_duration;
        public bool m_allowVFXStateChange = true;
        public bool m_instantiateShaderSettings = false;
        public bool m_isAdditionalEffect = false;

        public WeatherShaderData ShaderDataInstance = new WeatherShaderData();
        public WeatherShaderData LerpShaderData = new WeatherShaderData();

        /// <summary>
        /// Creates new base settings and assigns all the settings you've passed in
        /// </summary>
        /// <param name="player"></param>
        /// <param name="weatherProfile"></param>
        /// <param name="audioSource"></param>
        /// <param name="startingState"></param>
        /// <param name="currentMaxVolume"></param>
        /// <param name="startDuration"></param>
        /// <returns></returns>
        public static HDRPWeatherBaseSettings CreateNewBaseSettings(Transform player, HDRPTimeOfDayWeatherProfile weatherProfile, AudioSource audioSource, WeatherVFXState startingState, float currentMaxVolume, float startDuration)
        {
            HDRPWeatherBaseSettings settings = new HDRPWeatherBaseSettings
            {
                m_player = player,
                m_weatherProfile = weatherProfile,
                m_audioSource = audioSource,
                m_state = startingState,
                m_currentMaxVolume = currentMaxVolume,
                m_duration = startDuration
            };

            return settings;
        }

        /// <summary>
        /// Instanciates the shader data instance that is used
        /// </summary>
        public void InstanciateShaderInstance()
        {
            if (!m_instantiateShaderSettings)
            {
                return;
            }

            ShaderDataInstance.m_rainShaderData = RainShaderData.Copy(m_weatherProfile.WeatherShaderData.m_rainShaderData);
            ShaderDataInstance.m_snowShaderData = SnowShaderData.Copy(m_weatherProfile.WeatherShaderData.m_snowShaderData);
            ShaderDataInstance.m_sandShaderData = SandShaderData.Copy(m_weatherProfile.WeatherShaderData.m_sandShaderData);

#if GTS_PRESENT
            if (GTSTerrain.activeTerrain)
            {
                LoadGTSShaderData(ref LerpShaderData, HDRPTimeOfDayAPI.WeatherActive(), m_weatherProfile.WeatherShaderData);
            }
            else
            {
                LoadDefaultShaderData(ref LerpShaderData, HDRPTimeOfDayAPI.WeatherActive(), m_weatherProfile.WeatherShaderData);
            }
#else
            LoadDefaultShaderData(ref LerpShaderData, HDRPTimeOfDayAPI.WeatherActive(), m_weatherProfile.WeatherShaderData);
#endif
        }
        /// <summary>
        /// Increases the duration current value '+='
        /// </summary>
        /// <param name="value"></param>
        public void IncreaseDurationValue(float value)
        {
            m_duration += value;
        }
        /// <summary>
        /// Sets the weather profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetWeatherProfile(HDRPTimeOfDayWeatherProfile profile)
        {
            m_weatherProfile = profile;
            if (profile != null)
            {
                InstanciateShaderInstance();
            }
        }
        /// <summary>
        /// Changes the vfx state
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(WeatherVFXState state)
        {
            m_state = state;
        }
        /// <summary>
        /// Sets the duration value
        /// </summary>
        /// <param name="duration"></param>
        public void SetDuration(float duration)
        {
            m_duration = duration;
        }
        /// <summary>
        /// Checks if the duration is >= than 1
        /// </summary>
        /// <returns></returns>
        public bool CheckIsActive()
        {
            if (m_state == WeatherVFXState.FadeIn)
            {
                if (m_duration >= 1f)
                {
                    //m_duration = 1f;
                    ChangeState(WeatherVFXState.Active);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return m_state == WeatherVFXState.Active;
        }
        /// <summary>
        /// Sets the new current max volume that can be sued for transition from back to 0
        /// </summary>
        /// <param name="value"></param>
        public void SetNewCurrentMaxVolume(float value)
        {
            m_currentMaxVolume = value;
        }
        /// <summary>
        /// Gets the transition duration
        /// </summary>
        /// <returns></returns>
        public float GetTransitionDuration()
        {
            if (m_weatherProfile != null)
            {
                return m_weatherProfile.WeatherData.m_transitionDuration;
            }

            return 0f;
        }
        /// <summary>
        /// Gets the max weather vfx volume
        /// </summary>
        /// <returns></returns>
        public float GetWeatherEffectVolume()
        {
            if (m_weatherProfile != null)
            {
                return m_weatherProfile.WeatherFXData.m_weatherEffectVolume;
            }
            return 0f;
        }
        /// <summary>
        /// Gets or creates audio source on a currentObject
        /// </summary>
        /// <param name="currentObject"></param>
        public void GetOrCreateAudioSource(GameObject currentObject)
        {
            if (m_audioSource != null)
            {
                return;
            }

            m_audioSource = currentObject.GetComponent<AudioSource>();
            if (m_audioSource == null)
            {
                m_audioSource = currentObject.AddComponent<AudioSource>();
            }
        }
        /// <summary>
        /// Gets the weather vfx audio clip file
        /// </summary>
        /// <returns></returns>
        public AudioClip GetWeatherAudioClip()
        {
            if (m_weatherProfile != null)
            {
                return m_weatherProfile.WeatherFXData.m_weatherAudio;
            }
            return null;
        }
        /// <summary>
        /// Sets up the audio source
        /// Clip can be null if you don't want to cahnge the clip file
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="startingVolume"></param>
        /// <param name="loop"></param>
        /// <param name="startAudioSource"></param>
        public void SetAudioSourceSettings(AudioClip clip, float startingVolume, bool loop, bool startAudioSource)
        {
            if (m_audioSource != null)
            {
                if (clip != null)
                {
                    m_audioSource.clip = clip;
                }

                m_audioSource.loop = loop;
                m_audioSource.volume = startingVolume;
                if (startAudioSource)
                {
                    if (!m_audioSource.isPlaying)
                    {
                        m_audioSource.Play();
                    }
                }
                else
                {
                    m_audioSource.Stop();
                }
            }
        }
        /// <summary>
        /// Used to pause or unpause the current audio effect
        /// </summary>
        /// <param name="state"></param>
        public void SetPauseState(bool state)
        {
            if (state)
            {
                if (m_audioSource.isPlaying)
                {
                    m_audioSource.Stop();
                }
            }
            else
            {
                if (!m_audioSource.isPlaying)
                {
                    m_audioSource.Play();
                }
            }
        }
        /// <summary>
        /// Lerps a float value from - to based on the current duration value
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float LerpFromDuration(float from, float to)
        {
            return Mathf.Lerp(from, to, m_duration);
        }
        /// <summary>
        /// Sets the audio volume
        /// </summary>
        /// <param name="value"></param>
        public void SetAudioVolume(float value)
        {
            if (m_audioSource != null)
            {
                m_audioSource.volume = value;
            }
        }
        /// <summary>
        /// Gets the currently set max volume
        /// </summary>
        /// <returns></returns>
        public float GetCurrentVolume()
        {
            return m_currentMaxVolume;
        }
        /// <summary>
        /// Gets a random value from the mix/max value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float GetRandomValue(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        /// <summary>
        /// Gets a random value from the mix/max value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetRandomValue(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        /// <summary>
        /// Gets the player transform that is in the HDRP Time Of Day component
        /// </summary>
        /// <returns></returns>
        public Transform GetActivePlayerTransform()
        {
            HDRPTimeOfDay hdrpTimeOfDay = HDRPTimeOfDay.Instance;
            if (hdrpTimeOfDay != null)
            {
                m_player = hdrpTimeOfDay.Player;
                return hdrpTimeOfDay.Player;
            }

            return null;
        }
        /// <summary>
        /// Gets ground height at position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="yOffsetCheck"></param>
        /// <returns></returns>
        public float GetGroundPosition(Vector3 worldPosition, float yOffsetCheck = 1000f)
        {
            Vector3 position = worldPosition;
            position.y += yOffsetCheck;
            if (Physics.Raycast(new Ray(position, Vector3.down), out RaycastHit hit))
            {
                Terrain terrain = hit.transform.GetComponent<Terrain>();
                if (terrain != null)
                {
                    return terrain.SampleHeight(hit.point);
                }
                else
                {
                    return hit.point.y;
                }
            }

            return 0f;
        }
        /// <summary>
        /// Gets the HDRP Time Of Day system component
        /// </summary>
        /// <returns></returns>
        public HDRPTimeOfDay GetTimeOfDaySystem()
        {
            return HDRPTimeOfDay.Instance;
        }
        /// <summary>
        /// Moves a gameobject to the player transform + the yOffset
        /// </summary>
        /// <param name="objectToMove"></param>
        /// <param name="yOffset"></param>
        public void MoveGameObjectToPlayerPosition(GameObject objectToMove, float yOffset)
        {
            if (objectToMove != null && m_player != null)
            {
                Vector3 newPosition = m_player.position;
                newPosition.y += yOffset;
                objectToMove.transform.position = newPosition;
            }
        }
        /// <summary>
        /// Sets the global shader rain power value
        /// </summary>
        /// <param name="value"></param>
        public void SetRainPowerValue(float value)
        {
            if (m_weatherProfile != null)
            {
                RainShaderData rainShaderData = ShaderDataInstance.m_rainShaderData;
                rainShaderData.m_rainPower = value;
                WeatherShaderManager.ApplyRainShaderValues(rainShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader rain power on terrain value
        /// </summary>
        /// <param name="value"></param>
        public void SetRainPowerOnTerrainValue(float value)
        {
            if (m_weatherProfile != null)
            {
                RainShaderData rainShaderData = ShaderDataInstance.m_rainShaderData;
                rainShaderData.m_rainPowerOnTerrain = value;
                WeatherShaderManager.ApplyRainShaderValues(rainShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader sand power value
        /// </summary>
        /// <param name="value"></param>
        public void SetSandPowerValue(float value)
        {
            if (m_weatherProfile != null)
            {
                SandShaderData sandShaderData = ShaderDataInstance.m_sandShaderData;
                sandShaderData.m_sandPower = value;
                WeatherShaderManager.ApplySandShaderValues(sandShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader sand power on terrain value
        /// </summary>
        /// <param name="value"></param>
        public void SetSandPowerOnTerrainValue(float value)
        {
            if (m_weatherProfile != null)
            {
                SandShaderData sandShaderData = ShaderDataInstance.m_sandShaderData;
                sandShaderData.m_sandPowerOnTerrain = value;
                WeatherShaderManager.ApplySandShaderValues(sandShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader snow power value
        /// </summary>
        /// <param name="value"></param>
        public void SetSnowPowerValue(float value)
        {
            if (m_weatherProfile != null)
            {
                SnowShaderData snowShaderData = ShaderDataInstance.m_snowShaderData;
                snowShaderData.m_snowPower = value;
                WeatherShaderManager.ApplySnowShaderValues(snowShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader snow power on terrain value
        /// </summary>
        /// <param name="value"></param>
        public void SetSnowPowerOnTerrainValue(float value)
        {
            if (m_weatherProfile != null)
            {
                SnowShaderData snowShaderData = ShaderDataInstance.m_snowShaderData;
                snowShaderData.m_snowPowerOnTerrain = value;
                WeatherShaderManager.ApplySnowShaderValues(snowShaderData);
            }
        }
        /// <summary>
        /// Sets the global shader snow power on terrain value
        /// </summary>
        /// <param name="value"></param>
        public void SetSnowMinHeight(float value)
        {
            if (m_weatherProfile != null)
            {
                SnowShaderData snowShaderData = ShaderDataInstance.m_snowShaderData;
                snowShaderData.m_snowMinHeight = value;
                WeatherShaderManager.ApplySnowShaderValues(snowShaderData);
            }
        }
        /// <summary>
        /// Gets a random position around the player with an offset with the min/max
        /// Snap to ground will adjust the y to the ground
        /// </summary>
        /// <param name="minMax"></param>
        /// <param name="snapToGround"></param>
        /// <param name="yOffsetCheck"></param>
        /// <returns></returns>
        public Vector3 GetRandomPosition(Vector2 minMax, bool snapToGround, float yOffsetCheck)
        {
            if (m_player == null)
            {
                return Vector3.zero;
            }

            Vector3 value = new Vector3(m_player.transform.position.x, 0f, m_player.transform.position.z);
            if (GetRandomValue(minMax, out float x))
            {
                value.x += x;
            }
            else
            {
                value.x -= x;
            }
            if (GetRandomValue(minMax, out float z))
            {
                value.z += z;
            }
            else
            {
                value.z -= z;
            }

            if (snapToGround)
            {
                value.y = GetGroundPosition(value, yOffsetCheck);
            }

            return value;
        }
        /// <summary>
        /// Gets a random value from the mix/max value
        /// Retuns true if the value returned is positive, false if it's negative
        /// Out the result as a float
        /// </summary>
        /// <param name="minMax"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetRandomValue(Vector2 minMax, out float value)
        {
            value = UnityEngine.Random.Range(minMax.x, minMax.y);
            return value >= 0f;
        }
        /// <summary>
        /// Gets a random value from the mix/max value
        /// Retuns true if the value returned is positive, false if it's negative
        /// Out the result as a int
        /// </summary>
        /// <param name="minMax"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetRandomValue(Vector2Int minMax, out int value)
        {
            value = UnityEngine.Random.Range(minMax.x, minMax.y);
            return value >= 0;
        }
        /// <summary>
        /// Lerps a float value from start to end value based on the time
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public float LerpFloat(float startValue, float endValue, float time)
        {
            return Mathf.Lerp(startValue, endValue, time);
        }
        /// <summary>
        /// Lerps a color value from start to end value based on the time
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Color LerpColor(Color startValue, Color endValue, float time)
        {
            return Color.Lerp(startValue, endValue, time);
        }

        #region Load Shader Data

        /// <summary>
        /// Loads default shader data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="weatherActive"></param>
        /// <param name="weatherProfileData"></param>
        private void LoadDefaultShaderData(ref WeatherShaderData data, bool weatherActive, WeatherShaderData weatherProfileData)
        {
            data.m_isUsingGTS = false;

            //Rain
            data.m_rainShaderData.m_rainPower = weatherActive ? 0f : weatherProfileData.m_rainShaderData.m_rainPower;
            data.m_rainShaderData.m_rainPowerOnTerrain = weatherActive ? 0f : weatherProfileData.m_rainShaderData.m_rainPowerOnTerrain;
            //Snow
            data.m_snowShaderData.m_snowPower = weatherActive ? 0f : weatherProfileData.m_snowShaderData.m_snowPower;
            data.m_snowShaderData.m_snowPowerOnTerrain = weatherActive ? 0f : weatherProfileData.m_snowShaderData.m_snowPowerOnTerrain;
            //Sand
            data.m_sandShaderData.m_sandPower = weatherActive ? 0f : weatherProfileData.m_sandShaderData.m_sandPower;
            data.m_sandShaderData.m_sandPowerOnTerrain = weatherActive ? 0f : weatherProfileData.m_sandShaderData.m_sandPowerOnTerrain;
        }
        /// <summary>
        /// Loads profile shader data from GTS
        /// </summary>
        /// <param name="data"></param>
        /// <param name="weatherActive"></param>
        /// <param name="weatherProfileData"></param>
        private void LoadGTSShaderData(ref WeatherShaderData data, bool weatherActive, WeatherShaderData weatherProfileData)
        {
            data.m_isUsingGTS = true;

            //Rain
            Vector4 rainDataA = Shader.GetGlobalVector(WeatherShaderID.m_TOD_RainDataA);
            data.m_rainShaderData.m_rainPower = weatherActive ? rainDataA.x : weatherProfileData.m_rainShaderData.m_rainPower;
            data.m_rainShaderData.m_rainPowerOnTerrain = weatherActive ? rainDataA.y : weatherProfileData.m_rainShaderData.m_rainPowerOnTerrain;
            data.m_rainShaderData.m_rainMinHeight = weatherActive ? rainDataA.z : weatherProfileData.m_rainShaderData.m_rainMinHeight;
            data.m_rainShaderData.m_rainMaxHeight = weatherActive ? rainDataA.w : weatherProfileData.m_rainShaderData.m_rainMaxHeight;
            //Snow
            Vector4 snowDataA = Shader.GetGlobalVector(WeatherShaderID.m_TOD_SnowDataA);
            data.m_snowShaderData.m_snowPower = weatherActive ? snowDataA.x : weatherProfileData.m_snowShaderData.m_snowPower;
            data.m_snowShaderData.m_snowPowerOnTerrain = weatherActive ? snowDataA.y : weatherProfileData.m_snowShaderData.m_snowPowerOnTerrain;
            data.m_snowShaderData.m_snowMinHeight = weatherActive ? snowDataA.z : weatherProfileData.m_snowShaderData.m_snowMinHeight;
            Vector4 snowDataB = Shader.GetGlobalVector(WeatherShaderID.m_TOD_SnowDataB);
            data.m_snowShaderData.m_snowContrast = weatherActive ? snowDataB.y : weatherProfileData.m_snowShaderData.m_snowContrast;
            //Sand
            data.m_sandShaderData.m_sandPower = weatherActive ? 0f : weatherProfileData.m_sandShaderData.m_sandPower;
            data.m_sandShaderData.m_sandPowerOnTerrain = weatherActive ? 0f : weatherProfileData.m_sandShaderData.m_sandPowerOnTerrain;
        }

        #endregion
        #region Visual Effects

        //Utils
        /// <summary>
        /// Destroys a visual effect component and also destroys any property binders if it's true
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="destroyPropertyBinder"></param>
        public void DestroyVisualEffect(VisualEffect vfx, bool destroyPropertyBinder = true)
        {
            if (destroyPropertyBinder)
            {
                VFXPropertyBinder binder = vfx.GetComponent<VFXPropertyBinder>();
                if (binder == null)
                {
                    binder = vfx.GetComponentInChildren<VFXPropertyBinder>();
                }
                if (binder != null)
                {
                    VFXBinderBase[] baseBinders = binder.GetComponents<VFXBinderBase>();
                    if (baseBinders.Length > 0)
                    {
                        foreach (VFXBinderBase baseBinder in baseBinders)
                        {
                            GameObject.DestroyImmediate(baseBinder);
                        }
                    }
                    binder.m_Bindings.Clear();
                    GameObject.DestroyImmediate(binder);
                }
            }

            GameObject.DestroyImmediate(vfx);
        }
        /// <summary>
        /// Sets the visual effect play state (Play or Stop)
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="state"></param>
        public void SetPlayState(VisualEffect vfx, bool state)
        {
            if (vfx != null)
            {
                if (state)
                {
                    vfx.Play();
                }
                else
                {
                    vfx.Stop();
                }
            }
        }
        /// <summary>
        /// Sets the vfx property to the player position
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="offsetY"></param>
        /// <param name="property"></param>
        public void SetPlayerPosition(VisualEffect vfx, float offsetY, string property)
        {
            if (vfx != null && m_player != null)
            {
                if (vfx.HasVector3(property))
                {
                    Vector3 position = m_player.transform.position;
                    if (offsetY > 0f)
                    {
                        position.y += offsetY;
                    }
                    else if (offsetY < 0f)
                    {
                        position.y -= offsetY;
                    }

                    vfx.SetVector3(property, position);
                }
            }
        }
        /// <summary>
        /// Blends a color key by color a and b over time
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="time"></param>
        public void SetGradientColorBlend(VisualEffect vfx, string property, AnimationCurve blendCurve, float time)
        {
            if (vfx != null && blendCurve != null)
            {
                if (vfx.HasFloat(property))
                {
                    vfx.SetFloat(property, Mathf.Clamp01(blendCurve.Evaluate(time)));
                }
            }
        }
        /// <summary>
        /// Sets the vfx property to the player position
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="offsetY"></param>
        /// <param name="property"></param>
        public void SetPlayerPosition(VisualEffect vfx, float offsetY, int property)
        {
            if (vfx != null && m_player != null)
            {
                if (vfx.HasVector3(property))
                {
                    Vector3 position = m_player.transform.position;
                    if (offsetY > 0f)
                    {
                        position.y += offsetY;
                    }
                    else if (offsetY < 0f)
                    {
                        position.y -= offsetY;
                    }

                    vfx.SetVector3(property, position);
                }
            }
        }

        /// <summary>
        /// Sets the vfx property to the player y position
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="offsetY"></param>
        /// <param name="property"></param>
        public void SetPlayerYPosition(VisualEffect vfx, string property)
        {
            if (vfx != null && m_player != null)
            {
                if (vfx.HasFloat(property))
                {
                    Vector3 position = m_player.transform.position;
                    vfx.SetFloat(property, position.y);
                }
            }
        }
        /// <summary>
        /// Sets the vfx property to the player y position
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="offsetY"></param>
        /// <param name="property"></param>
        public void SetPlayerYPosition(VisualEffect vfx, int property)
        {
            if (vfx != null && m_player != null)
            {
                if (vfx.HasFloat(property))
                {
                    Vector3 position = m_player.transform.position;
                    vfx.SetFloat(property, position.y);
                }
            }
        }
        /// <summary>
        /// Parent the visual effect to the player
        /// </summary>
        /// <param name="vfx"></param>
        public void ParentEffectToPlayer(VisualEffect vfx)
        {
            if (vfx != null && m_player != null)
            {
                vfx.transform.SetParent(m_player.transform);
                vfx.transform.localPosition = Vector3.zero;
                vfx.transform.localEulerAngles = Vector3.zero;
            }
        }
        /// <summary>
        /// Sets the visual effect asset
        /// </summary>
        /// <param name="vfx"></param>
        /// <param name="asset"></param>
        public void SetVisualEffectAsset(VisualEffect vfx, VisualEffectAsset asset)
        {
            if (vfx != null && asset != null)
            {
                vfx.visualEffectAsset = asset;
            }
        }
        /// <summary>
        /// Gets main effect from the list
        /// </summary>
        /// <param name="effects"></param>
        /// <returns></returns>
        public HDRPWeatherVisualEffect GetMainEffect(List<HDRPWeatherVisualEffect> effects)
        {
            foreach (HDRPWeatherVisualEffect visualEffect in effects)
            {
                if (visualEffect.m_mainEffect)
                {
                    return visualEffect;
                }
            }
            return null;
        }

        //Set
        public void SetVisualEffectBool(VisualEffect vfx, bool value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasBool(property))
                {
                    vfx.SetBool(property, value);
                }
            }
        }
        public void SetVisualEffectBool(VisualEffect vfx, bool value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasBool(property))
                {
                    vfx.SetBool(property, value);
                }
            }
        }
        public void SetVisualEffectFloat(VisualEffect vfx, float value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasFloat(property))
                {
                    vfx.SetFloat(property, value);
                }
            }
        }
        public void SetVisualEffectFloat(VisualEffect vfx, float value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasFloat(property))
                {
                    vfx.SetFloat(property, value);
                }
            }
        }
        public void SetVisualEffectInt(VisualEffect vfx, int value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasInt(property))
                {
                    vfx.SetInt(property, value);
                }
            }
        }
        public void SetVisualEffectInt(VisualEffect vfx, int value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasInt(property))
                {
                    vfx.SetInt(property, value);
                }
            }
        }
        public void SetVisualEffectVector(VisualEffect vfx, Vector4 value, string property, VFXVectorType type)
        {
            if (vfx != null)
            {
                switch (type)
                {
                    case VFXVectorType.Vector2:
                    {
                        if (vfx.HasVector2(property))
                        {
                            vfx.SetVector2(property, value);
                        }
                        break;
                    }
                    case VFXVectorType.Vector3:
                    {
                        if (vfx.HasVector3(property))
                        {
                            vfx.SetVector3(property, value);
                        }
                        break;
                    }
                    case VFXVectorType.Vector4:
                    {
                        if (vfx.HasVector4(property))
                        {
                            vfx.SetVector4(property, value);
                        }
                        break;
                    }
                }

            }
        }
        public void SetVisualEffectVector(VisualEffect vfx, Vector4 value, int property, VFXVectorType type)
        {
            if (vfx != null)
            {
                switch (type)
                {
                    case VFXVectorType.Vector2:
                    {
                        if (vfx.HasVector2(property))
                        {
                            vfx.SetVector2(property, value);
                        }
                        break;
                    }
                    case VFXVectorType.Vector3:
                    {
                        if (vfx.HasVector3(property))
                        {
                            vfx.SetVector3(property, value);
                        }
                        break;
                    }
                    case VFXVectorType.Vector4:
                    {
                        if (vfx.HasVector4(property))
                        {
                            vfx.SetVector4(property, value);
                        }
                        break;
                    }
                }

            }
        }
        public void SetVisualEffectGradient(VisualEffect vfx, Gradient value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasGradient(property))
                {
                    vfx.SetGradient(property, value);
                }
            }
        }
        public void SetVisualEffectGradient(VisualEffect vfx, Gradient value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasGradient(property))
                {
                    vfx.SetGradient(property, value);
                }
            }
        }
        public void SetVisualEffectAnimationCurve(VisualEffect vfx, AnimationCurve value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasAnimationCurve(property))
                {
                    vfx.SetAnimationCurve(property, value);
                }
            }
        }
        public void SetVisualEffectAnimationCurve(VisualEffect vfx, AnimationCurve value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasAnimationCurve(property))
                {
                    vfx.SetAnimationCurve(property, value);
                }
            }
        }
        public void SetVisualEffectMesh(VisualEffect vfx, Mesh value, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasMesh(property))
                {
                    vfx.SetMesh(property, value);
                }
            }
        }
        public void SetVisualEffectMesh(VisualEffect vfx, Mesh value, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasMesh(property))
                {
                    vfx.SetMesh(property, value);
                }
            }
        }
        //Get
        public bool GetVisualEffectBoolValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasBool(property))
                {
                    return vfx.GetBool(property);
                }
            }

            return false;
        }
        public bool GetVisualEffectBoolValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasBool(property))
                {
                    return vfx.GetBool(property);
                }
            }

            return false;
        }
        public float GetVisualEffectFloatValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasFloat(property))
                {
                    return vfx.GetFloat(property);
                }
            }

            return -1f;
        }
        public float GetVisualEffectFloatValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasFloat(property))
                {
                    return vfx.GetFloat(property);
                }
            }

            return -1f;
        }
        public int GetVisualEffectIntValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasInt(property))
                {
                    return vfx.GetInt(property);
                }
            }

            return -1;
        }
        public int GetVisualEffectIntValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasInt(property))
                {
                    return vfx.GetInt(property);
                }
            }

            return -1;
        }
        public Vector4 GetVisualEffectVectorValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasVector2(property))
                {
                    Vector2 value = vfx.GetVector2(property);
                    return new Vector4(value.x, value.y, 0f, 0f);
                }
                else if (vfx.HasVector3(property))
                {
                    Vector3 value = vfx.GetVector3(property);
                    return new Vector4(value.x, value.y, value.z, 0f);
                }
                else if (vfx.HasVector4(property))
                {
                    return vfx.GetVector4(property);
                }
            }

            return Vector4.zero;
        }
        public Vector4 GetVisualEffectVectorValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasVector2(property))
                {
                    Vector2 value = vfx.GetVector2(property);
                    return new Vector4(value.x, value.y, 0f, 0f);
                }
                else if (vfx.HasVector3(property))
                {
                    Vector3 value = vfx.GetVector3(property);
                    return new Vector4(value.x, value.y, value.z, 0f);
                }
                else if (vfx.HasVector4(property))
                {
                    return vfx.GetVector4(property);
                }
            }

            return Vector4.zero;
        }
        public Gradient GetVisualEffectGradientValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasGradient(property))
                {
                    return vfx.GetGradient(property);
                }
            }

            return null;
        }
        public Gradient GetVisualEffectGradientValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasGradient(property))
                {
                    return vfx.GetGradient(property);
                }
            }

            return null;
        }
        public AnimationCurve GetVisualEffectAnimationCurveValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasAnimationCurve(property))
                {
                    return vfx.GetAnimationCurve(property);
                }
            }

            return null;
        }
        public AnimationCurve GetVisualEffectGAnimationCurveValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasAnimationCurve(property))
                {
                    return vfx.GetAnimationCurve(property);
                }
            }

            return null;
        }
        public Mesh GetVisualEffectMeshValue(VisualEffect vfx, string property)
        {
            if (vfx != null)
            {
                if (vfx.HasMesh(property))
                {
                    return vfx.GetMesh(property);
                }
            }

            return null;
        }
        public Mesh GetVisualEffectMeshValue(VisualEffect vfx, int property)
        {
            if (vfx != null)
            {
                if (vfx.HasMesh(property))
                {
                    return vfx.GetMesh(property);
                }
            }

            return null;
        }

        #endregion
    }

    [System.Serializable]
    public class HDRPWeatherVisualEffect
    {
        public string m_name;
        public bool m_followPlayer = false;
        public bool m_setPlayerYPosition = false;
        public bool m_canUnderwaterControl = true;
        public float m_offset = 0f;
        public bool m_mainEffect = false;
        public VisualEffectAsset m_overrideAsset;
        public VisualEffect m_visualEffect;
        public float m_stopWaitTime = 15f;

        private bool m_readyToBeDestroyed = false;

        /// <summary>
        /// Sets the override VFX asset
        /// </summary>
        public void SetOverrideAsset()
        {
            if (m_overrideAsset != null)
            {
                m_visualEffect.visualEffectAsset = m_overrideAsset;
            }
        }
        /// <summary>
        /// Stops the vfx
        /// </summary>
        /// <returns></returns>
        public IEnumerator StopVFX()
        {
            m_readyToBeDestroyed = false;
            yield return new WaitForEndOfFrame();
            if (m_visualEffect != null)
            {
                m_visualEffect.Stop();
                yield return new WaitForSeconds(m_stopWaitTime);
            }

            m_readyToBeDestroyed = true;
        }
        /// <summary>
        /// Returns ready to be destroyed bool
        /// </summary>
        /// <returns></returns>
        public bool ReadyToBeDestroyed()
        {
            return m_readyToBeDestroyed;
        }
    }

    /// <summary>
    /// Weather VFX Interface
    /// </summary>
    public enum WeatherVFXState { InActive, Active, FadeIn, FadeOut }
    /// <summary>
    /// Weather vfx interface
    /// </summary>
    public interface IHDRPWeatherVFX
    {
        void StartWeatherFX(HDRPTimeOfDayWeatherProfile profile);
        void StopWeatherFX(float duration);
        void SetDuration(float value);
        void DestroyVFX();
        void SetVFXPlayState(bool state);
        void DestroyInstantly();
        float GetCurrentDuration();
        List<VisualEffect> CanBeControlledByUnderwater();
    }

    public class WeatherFXInstance : MonoBehaviour
    {
        public HDRPWeatherBaseSettings BaseSettings = new HDRPWeatherBaseSettings();
        [FormerlySerializedAs("m_visualEffects")]
        public List<HDRPWeatherVisualEffect> VisualEffects = new List<HDRPWeatherVisualEffect>();
        public WindZone WindZone;

        /// <summary>
        /// Sets the vfx play state
        /// </summary>
        /// <param name="state"></param>
        public void SetVFXState(bool state)
        {
            if (!BaseSettings.m_allowVFXStateChange || VisualEffects.Count < 1)
            {
                return;
            }

            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                if (state)
                {
                    visualEffect.m_visualEffect.Play();
                }
                else
                {
                    visualEffect.m_visualEffect.Stop();
                }
            }
        }
        /// <summary>
        /// Gets the wind zone component in the scene
        /// If createWindZone is true it will create a wind zone
        /// </summary>
        /// <param name="createWindZone"></param>
        /// <returns></returns>
        public bool GetWindZone(bool createWindZone, out float currentWindSpeed)
        {
            currentWindSpeed = 0f;
            WindZone = FindObjectOfType<WindZone>();
            if (WindZone == null && createWindZone)
            {
                GameObject windZoneObject = new GameObject("Wind Zone");
                WindZone = windZoneObject.AddComponent<WindZone>();
                SetDefaultWindZoneSettings(WindZone);
                windZoneObject.transform.eulerAngles = new Vector3(25f, 0f, 0f);
                currentWindSpeed = WindZone.windMain;
            }

            if (WindZone != null)
            {
                currentWindSpeed = WindZone.windMain;
            }

            return WindZone != null;
        }
        /// <summary>
        /// Sets the vfx to sync with wind, the state will be if it will try add or remove the effects
        /// </summary>
        /// <param name="addState"></param>
        public void SyncWithWind(bool addState)
        {
#if HDPipeline
            if (addState)
            {
                if (BaseSettings.m_syncWind)
                {
                    WeatherWindSyncController controller = WeatherWindSyncController.Instance;
                    if (controller != null)
                    {
                        controller.AddVisualEffects(VisualEffects.ToArray());
                    }
                }
            }
            else
            {
                WeatherWindSyncController controller = WeatherWindSyncController.Instance;
                if (controller != null)
                {
                    controller.RemoveVisualEffects(VisualEffects.ToArray());
                }
            }
#endif
        }
        /// <summary>
        /// Bool function used to check if all the m_visualEffects in this system has been destroyed
        /// </summary>
        /// <returns></returns>
        public bool AllVisualEffectsDestroyed()
        {
            foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
            {
                if (!visualEffect.ReadyToBeDestroyed())
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Sorts through all the effects and returns a list of effects that underwater effects can control
        /// </summary>
        /// <returns></returns>
        public List<VisualEffect> GetVFXThatUnderwaterCanInteractWith()
        {
            List<VisualEffect> vfxEffects = new List<VisualEffect>();
            if (VisualEffects.Count > 0)
            {
                foreach (HDRPWeatherVisualEffect visualEffect in VisualEffects)
                {
                    if (visualEffect.m_canUnderwaterControl)
                    {
                        vfxEffects.Add(visualEffect.m_visualEffect);
                    }
                }
            }

            return vfxEffects;
        }
        /// <summary>
        /// Sets the default wind zone parameters
        /// </summary>
        /// <param name="windZone"></param>
        private void SetDefaultWindZoneSettings(WindZone windZone)
        {
            if (windZone != null)
            {
                windZone.windMain = 0.15f;
                windZone.windTurbulence = 0.2f;
                windZone.windPulseMagnitude = 0.5f;
                windZone.windPulseFrequency = 0.1f;
            }
        }
    }
}
#endif