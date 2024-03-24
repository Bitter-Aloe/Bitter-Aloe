using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProceduralWorlds.HDRPTOD
{
    public enum CollapseSideMode { None, Left, Right };

    [ExecuteAlways]
    public class HDRPTimeOfDayDebugUI : MonoBehaviour
    {
#if HDPipeline && UNITY_2021_2_OR_NEWER
        public static HDRPTimeOfDayDebugUI Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static HDRPTimeOfDayDebugUI m_instance;

        [Header("Global Settings")]
        public HDRPTimeOfDay TimeOfDay;
        public EventSystem EventSystem;

        [Header("Collapse Settings")]
        public CollapseSideMode m_collapseMode = CollapseSideMode.Left;
        public RectTransform m_targetUI;
        public RectTransform m_leftCollapseButton;
        public Text m_leftButtonText;
        public RectTransform m_rightCollapseButton;
        public Text m_rightButtonText;

        [Header("Time Of Day Settings")]
        public Slider m_time;
        public Text m_timeLabel;
        public Toggle m_autoUpdate;
        public Slider m_autoUpdateSpeed;
        public Text m_autoUpdateLabel;
        public Slider m_direction;
        public Text m_directionLabel;

        [Header("Lighting Quality")]
        public Dropdown m_lightingRenderMode;
        public Dropdown m_rtRenderMode;

        [Header("Weather")]
        public Button m_startRain;
        public Button m_startSnow;

        private bool m_lastValuesSet = false;
        [SerializeField, HideInInspector] private bool m_lastSSGIValue;
        [SerializeField, HideInInspector] private bool m_lastSSRValue;
        [SerializeField, HideInInspector] private bool m_lastContactShadowsValue;

        private Vector2 m_currentPosition;
        private Vector2 m_startingPosition;
        private bool m_currentCollapseState = false;
        private bool m_updateCollapse;
        private float m_updateTime;

        private void OnEnable()
        {
            m_instance = this;
        }
        private void Awake()
        {
            CheckEventSystem();
        }
        private void Start()
        {
            if (TimeOfDay == null || !Application.isPlaying)
            {
                TimeOfDay = HDRPTimeOfDayAPI.GetTimeOfDay();
            }
            if (TimeOfDay != null)
            {
                bool weatherActive = TimeOfDay.UseWeatherFX;
                if (m_startRain != null)
                {
                    m_startRain.interactable = weatherActive;
                }
                if (m_startSnow != null)
                {
                    m_startSnow.interactable = weatherActive;
                }
            }

            SetLightingRenderMode();
            SetRTMode();
            GetLastValues();
            BuildLightingOptions();
            BuildDefaultValues();
            BuildCollapseState();
        }
        private void OnDisable()
        {
            SetLastValues();
        }
        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_time != null && m_timeLabel != null)
            {
                m_time.SetValueWithoutNotify(TimeOfDay.TimeOfDay);
                m_timeLabel.text = $"{m_time.value:0.00}";
            }

            ProcessCollapse();
        }

        public void ChangeCollapseState()
        {
            m_updateCollapse = true;
            m_currentCollapseState = !m_currentCollapseState;
            m_currentPosition = m_targetUI.anchoredPosition;
            m_updateTime = 0f;

            if (m_leftButtonText != null)
            {
                m_leftButtonText.text = m_currentCollapseState ? ">" : "<";
            }
            if (m_rightButtonText != null)
            {
                m_rightButtonText.text = m_currentCollapseState ? "<" : ">";
            }
        }
        public void SetTimeOfDay()
        {
            if (m_time != null && m_timeLabel != null)
            {
                HDRPTimeOfDayAPI.SetCurrentTime(m_time.value);
                m_timeLabel.text = $"{m_time.value:0.00}";
            }
        }
        public void SetAutoUpdate()
        {
            if (m_autoUpdate != null && m_autoUpdateSpeed != null && m_autoUpdateLabel != null)
            {
                HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(m_autoUpdate.isOn, m_autoUpdateSpeed.value);
                m_autoUpdateLabel.text = $"{m_autoUpdateSpeed.value:0.00}";
            }
        }
        public void SetDirection()
        {
            if (m_direction != null && m_directionLabel != null)
            {
                HDRPTimeOfDayAPI.SetDirection(m_direction.value);
                m_directionLabel.text = $"{m_direction.value:0.00}";
            }
        }
        public void StartStopRain()
        {
            if (HDRPTimeOfDay.Instance.WeatherActive())
            {
                HDRPTimeOfDayAPI.StopWeather();
            }
            else
            {
                HDRPTimeOfDayAPI.StartWeather(HDRPTimeOfDayAPI.GetWeatherIDByName("Rain"));
            }
        }
        public void StartStopSnow()
        {
            if (HDRPTimeOfDay.Instance.WeatherActive())
            {
                HDRPTimeOfDayAPI.StopWeather();
            }
            else
            {
                HDRPTimeOfDayAPI.StartWeather(HDRPTimeOfDayAPI.GetWeatherIDByName("Snow"));
            }
        }
        public void SetLightingRenderMode()
        {
            if (m_lightingRenderMode != null)
            {
                HDRPTimeOfDayAPI.SetLightingRenderQuality((GeneralQuality)m_lightingRenderMode.value);
            }
        }
        public void SetRTMode()
        {
            if (m_rtRenderMode != null)
            {
                HDRPTimeOfDayAPI.SetOverallRayTracingMode((RayTracingOverallQuality)m_rtRenderMode.value);
            }
        }

        private void BuildCollapseState()
        {            
            m_currentCollapseState = false;
            m_updateCollapse = false;
            switch (m_collapseMode)
            {
                case CollapseSideMode.None:
                {
                    if (m_leftCollapseButton != null)
                    {
                        m_leftCollapseButton.gameObject.SetActive(false);
                    }
                    if (m_rightCollapseButton != null)
                    {
                        m_rightCollapseButton.gameObject.SetActive(false);
                    }
                    break;
                }
                case CollapseSideMode.Left:
                {
                    Vector3 pos = new Vector3(265f, -265f, 0f);
                    if (m_targetUI != null)
                    {
                        m_targetUI.anchoredPosition = pos;
                    }
                    if (m_leftCollapseButton != null)
                    {
                        m_leftCollapseButton.gameObject.SetActive(true);
                    }
                    if (m_rightCollapseButton != null)
                    {
                        m_rightCollapseButton.gameObject.SetActive(false);
                    }
                    break;
                }
                case CollapseSideMode.Right:
                {
                    Vector3 pos = new Vector3(1660f, -265f, 0f);
                    if (m_targetUI != null)
                    {
                        m_targetUI.anchoredPosition = pos;
                    }
                    if (m_leftCollapseButton != null)
                    {
                        m_leftCollapseButton.gameObject.SetActive(false);
                    }
                    if (m_rightCollapseButton != null)
                    {
                        m_rightCollapseButton.gameObject.SetActive(true);
                    }
                    break;
                }
            }

            if (m_targetUI != null)
            {
                m_startingPosition = m_targetUI.anchoredPosition;
            }
        }
        private void ProcessCollapse()
        {
            if (m_updateCollapse)
            {
                m_updateTime += Time.deltaTime;
                switch (m_collapseMode)
                {
                    case CollapseSideMode.Left:
                    {
                        float xValue = m_currentCollapseState ? Mathf.Lerp(m_currentPosition.x, -246f, m_updateTime) : Mathf.Lerp(m_currentPosition.x, m_startingPosition.x, m_updateTime);
                        if (m_targetUI != null)
                        {
                            m_targetUI.anchoredPosition = new Vector2(xValue, m_startingPosition.y);
                        }
                        break;
                    }
                    case CollapseSideMode.Right:
                    {
                        float xValue = m_currentCollapseState ? Mathf.Lerp(m_currentPosition.x, 2170f, m_updateTime) : Mathf.Lerp(m_currentPosition.x, m_startingPosition.x, m_updateTime);
                        if (m_targetUI != null)
                        {
                            m_targetUI.anchoredPosition = new Vector2(xValue, m_startingPosition.y);
                        }
                        break;
                    }
                }

                if (m_updateTime >= 1f)
                {
                    m_updateCollapse = false;           
                }
            }
        }
        private void BuildLightingOptions()
        {
            if (m_lightingRenderMode != null)
            {
                m_lightingRenderMode.options.Clear();
                List<string> options = new List<string>
                {
                    "Low",
                    "Medium",
                    "High"
                };
                m_lightingRenderMode.AddOptions(options);
            }
        }
        private void BuildDefaultValues()
        {
            if (TimeOfDay == null)
            {
                return;
            }

            if (m_time != null && m_timeLabel != null)
            {
                m_time.SetValueWithoutNotify(TimeOfDay.TimeOfDay);
                m_timeLabel.text = $"{m_time.value:0.00}";
            }

            HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float updateValue);
            if (m_autoUpdate != null && m_autoUpdateSpeed != null && m_autoUpdateLabel != null)
            {
                m_autoUpdate.SetIsOnWithoutNotify(autoUpdate);
                m_autoUpdateSpeed.SetValueWithoutNotify(updateValue);
                m_autoUpdateLabel.text = $"{updateValue:0.00}";
            }

            if (m_direction != null && m_directionLabel != null)
            {
                m_direction.SetValueWithoutNotify(TimeOfDay.DirectionY);
                m_directionLabel.text = $"{m_direction.value:0.00}";
            }

            if (m_lightingRenderMode != null)
            {
                m_lightingRenderMode.SetValueWithoutNotify(2);
            }
        }
        private void CheckEventSystem()
        {
            if (EventSystem != null)
            {
                EventSystem[] events = FindObjectsOfType<EventSystem>();
                if (events.Length == 1)
                {
                    return;
                }
                EventSystem.enabled = EventSystem != EventSystem.current;
            }
        }
        private void GetLastValues()
        {
            m_lastValuesSet = true;
            m_lastSSGIValue = HDRPTimeOfDayAPI.GetTODBoolFunction(TODAPISetFunctions.SSGI);
            m_lastSSRValue = HDRPTimeOfDayAPI.GetTODBoolFunction(TODAPISetFunctions.SSR);
            m_lastContactShadowsValue = HDRPTimeOfDayAPI.GetTODBoolFunction(TODAPISetFunctions.ContactShadows);
        }
        private void SetLastValues()
        {
            if (!m_lastValuesSet)
            {
                return;
            }

            HDRPTimeOfDayAPI.SetTODBoolFunction(m_lastSSGIValue, TODAPISetFunctions.SSGI);
            HDRPTimeOfDayAPI.SetTODBoolFunction(m_lastSSRValue, TODAPISetFunctions.SSR);
            HDRPTimeOfDayAPI.SetTODBoolFunction(m_lastContactShadowsValue, TODAPISetFunctions.ContactShadows);
        }
#endif
    }
}