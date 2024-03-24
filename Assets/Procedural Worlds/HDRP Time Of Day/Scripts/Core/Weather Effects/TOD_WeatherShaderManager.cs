using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteInEditMode]
    public class TOD_WeatherShaderManager : MonoBehaviour
    {

        [Header("Rain")]
        [Range(0f, 1f)]
        public float m_rainPower = 0.0f;
        [Range(0f, 1f)]
        public float m_rainPowerOnTerrain = 0.0f;
        public float m_rainMinHeight = 0.0f;
        public float m_rainMaxHeight = 3000.0f;
        public float m_rainSpeed = 0.0f;
        public float m_rainScale = 1.0f;
        public float m_rainDarkness = 0.0f;
        public float m_rainSmoothness = 2.0f;
        public Texture2D m_rainMap;

        [Header("Sand")]
        [Range(0f, 1f)]
        public float m_sandPower = 0.0f;
        public float m_sandMaxHeight = 3000.0f;
        public float m_sandContrast = 0.0f;
        public float m_sandWorldScale = 1.0f;
        public Texture2D m_sandAlbedoMap;
        public Texture2D m_sandNormalMap;
        public Texture2D m_sandMaskMap;

        [Header("Snow")]
        [Range(0f, 1f)]
        public float m_snowPower = 0.0f;
        [Range(0f, 1f)]
        public float m_snowPowerOnTerrain = 0.0f;
        public float m_snowMinHeight = 0.0f;
        [Range(0f, 1f)]
        public float m_snowAge = 0.0f;
        public float m_snowContrast = 0.0f;
        public float m_snowWorldScale = 1.0f;
        [Range(0f, 1f)]
        public float m_snowSubsurface = 1.0f;
        public float m_snowBuildUp = 1.0f;
        public Texture2D m_snowAlbedoMap;
        public Texture2D m_snowNormalMap;
        public Texture2D m_snowMaskMap;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //Update Global Parameters

            //Rain
            Shader.SetGlobalTexture("_TOD_RainMap", m_rainMap);
            //Shader.SetGlobalFloat("_TOD_RainScale", m_rainScale);
            Shader.SetGlobalVector("_TOD_RainDataA", new Vector4(m_rainPower, m_rainPowerOnTerrain, m_rainMinHeight, m_rainMaxHeight));
            Shader.SetGlobalVector("_TOD_RainDataB", new Vector4(m_rainSpeed, m_rainDarkness, m_rainSmoothness, m_rainScale));
            //Shader.SetGlobalVector("_TOD_RainDataB", new Vector4(m_rainPower, m_rainSpeed, m_rainDarkness, m_rainSmoothness));

            //Sand
            //Shader.SetGlobalVector("_TOD_SandData", new Vector4(m_sandPower, m_sandContrast, m_sandWorldScale,0));
            Shader.SetGlobalVector("_TOD_SandDataA", new Vector4(m_sandPower, m_sandMaxHeight, m_sandContrast, m_sandWorldScale));
            Shader.SetGlobalTexture("_TOD_SandAlbedoMap", m_sandAlbedoMap);
            Shader.SetGlobalTexture("_TOD_SandNormalMap", m_sandNormalMap);
            Shader.SetGlobalTexture("_TOD_SandMaskMap", m_sandMaskMap);

            //Snow
            //Shader.SetGlobalVector("_TOD_SnowData", new Vector4(m_snowAmount, m_snowContrast, m_snowWorldScale, 0));
            Shader.SetGlobalVector("_TOD_SnowDataA", new Vector4(m_snowPower, m_snowPowerOnTerrain, m_snowMinHeight, m_snowAge));
            Shader.SetGlobalVector("_TOD_SnowDataB", new Vector4(m_snowWorldScale, m_snowContrast, m_snowSubsurface, m_snowBuildUp));
            Shader.SetGlobalTexture("_TOD_SnowAlbedoMap", m_snowAlbedoMap);
            Shader.SetGlobalTexture("_TOD_SnowNormalMap", m_snowNormalMap);
            Shader.SetGlobalTexture("_TOD_SnowMaskMap", m_snowMaskMap);
        }
    }
}