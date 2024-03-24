using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDaySeasonProfile : ScriptableObject
    {
        //Season
        public float m_seasonTransitionDuration = 2000f;
        [Range(0f, 3.99f)]
        public float m_season = 0f;
        public Color m_seasonWinterTint = Color.white;
        public Color m_seasonSpringTint = Color.white;
        public Color m_seasonSummerTint = Color.white;
        public Color m_seasonAutumnTint = Color.white;

        [SerializeField, HideInInspector] private float m_savedSeason = 0f;

        public const string m_globalSeasonTint = "_PW_Global_SeasonalTint";
        public const string m_shaderSeasonColorShift = "_PW_SeasonalTintAmount";

        #region Public Functions

        /// <summary>
        /// Resets the global shaders
        /// </summary>
        public void Reset()
        {
            Shader.SetGlobalFloat(m_shaderSeasonColorShift, 0f);
            m_season = m_savedSeason;
        }
        /// <summary>
        /// Sets up and stores the season value
        /// </summary>
        public void Setup()
        {
            m_savedSeason = m_season;
        }
        /// <summary>
        /// Applies the seasons and sets the global shader values
        /// </summary>
        public void Apply(bool active, bool applicationPlaying)
        {
            ApplySeasons(active, applicationPlaying);
        }
        /// <summary>
        /// Get the active season (Returns names: Winter, Spring, Autumn, Summer)
        /// </summary>
        /// <returns></returns>
        public string GetCurrentSeason()
        {
            string season = "";

            if (m_season > 0 && m_season < 1)
            {
                season = "Winter";
            }
            else if (m_season > 1 && m_season < 2)
            {
                season = "Spring";
            }
            else if (m_season > 2 && m_season < 3)
            {
                season = "Summer";
            }
            else
            {
                season = "Autumn";
            }

            return season;
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Updates the season settings
        /// </summary>
        private void ApplySeasons(bool active, bool applicationPlaying)
        {
            if (active)
            {
                if (applicationPlaying)
                {
                    UpdateSeasonValue();
                }

                Color color = GetActualSeasonalTintColor();
                if (color == Color.black)
                {
                    color = Color.white;
                }
                Shader.SetGlobalColor(m_globalSeasonTint, ColorInvert(color));
            }
            else
            {
                Shader.SetGlobalColor(m_globalSeasonTint, ColorInvert(Color.white));
            }
        }
        /// <summary>
        /// Gets the actual seasonal color
        /// </summary>
        /// <returns></returns>
        private Color GetActualSeasonalTintColor()
        {
            Color tint = Color.white;
            if (m_season < 1f)
            {
                if (tint != m_seasonWinterTint)
                {
                    tint = Color.Lerp(m_seasonWinterTint, m_seasonSpringTint, m_season);
                }
            }
            else if (m_season < 2f)
            {
                if (tint != m_seasonSpringTint)
                {
                    tint = Color.Lerp(m_seasonSpringTint, m_seasonSummerTint, m_season - 1f);
                }
            }
            else if (m_season < 3f)
            {
                if (tint != m_seasonSummerTint)
                {
                    tint = Color.Lerp(m_seasonSummerTint, m_seasonAutumnTint, m_season - 2f);
                }
            }
            else
            {
                if (tint != m_seasonAutumnTint)
                {
                    tint = Color.Lerp(m_seasonAutumnTint, m_seasonWinterTint, m_season - 3f);
                }
            }

            return tint;
        }
        /// <summary>
        /// Updates the seasons
        /// </summary>
        private void UpdateSeasonValue()
        {
            m_season = Mathf.Lerp(m_season, 3.9999f, Time.deltaTime / m_seasonTransitionDuration);
            if (m_season > 3.98f)
            {
                m_season = 0f;
            }
        }

        #endregion
        #region Public Static Utils

        /// <summary>
        /// Color inverter for seasonal color setup
        /// </summary>
        /// <param name="i_color"></param>
        /// <returns></returns>
        public static Color ColorInvert(Color color)
        {
            Color result;

            result.r = 1.0f - color.r;
            result.g = 1.0f - color.g;
            result.b = 1.0f - color.b;
            result.a = 1.0f - color.a;

            return (result);
        }

        #endregion
    }
}