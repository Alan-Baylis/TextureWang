using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

//namespace TextureWang
//{

    enum ChannelType
    {
        EightBit,
        HalfFloat,
        Float
    }

    public enum ShaderOp
    {
        MultRGB=0,
        Gradient=1,
        Distort=2,
        Normal=3,
        AddRGB=4,
        Add2=5,
        Mult2=6,
        Pow2=7,
        Min=8,
        Max=9,
        clipMin=10, //if < input colour set to 0
        Blend2=11,
        Sub2=12,
        Level1=13,
        Transform=14,
        DirectionWarp = 15,
        Power=16,
        Min1=17,
        Max1=18,
        CopyGrey=19,
        CopyColor = 20,
        Step=21,
        Invert=22,
        SrcBlend=23,
        Stepify=24,
        EdgeDist=25,
        Smooth=26,
        BlackEdge=27,
        CopyColorAndAlpha = 28, 
        EdgeDistDir = 29,
        Splatter=30,
        SplatterGrid=31,
        Sobel=32,
        GenCurve = 33,
        AbsDistort=34,
        MapCylinder=35,
        CopyRandA = 36, //used for unity metalic and roughness
        Histogram = 37,
        ProbabilityBlend = 38,
        RandomEdge=39,
        CopyRGBA = 40,
        CopyNormalMap=41,


    SetCol = 0,
        PerlinBm=1,
        PerlinTurb = 2,
        PerlinRidge = 3,
        VeroniNoise = 4,
        Pattern     = 5,
        Grid        = 6,
        Weave       = 7,
        Circle = 8,
        Ripples=9



    }


    public class TextureParamType : IConnectionTypeDeclaration
    {
        public string Identifier { get { return "TextureParam"; } }
        public Type Type { get { return typeof(TextureParam); } }
        public Color Color { get { return Color.red; } }
        public string InKnobTex { get { return "Textures/In_Knob.png"; } }
        public string OutKnobTex { get { return "Textures/Out_Knob.png"; } }
    }

    public class TextureParam
    {
        public static TextureFormat ms_TexFormat = TextureFormat.RGBAFloat;
        public static RenderTextureFormat ms_RTexFormat = RenderTextureFormat.ARGBFloat;



        public static RenderTextureFormat GetRTFormat(bool _grey)
        {
            if (_grey)
            {
                
                return RenderTextureFormat.RFloat;
            }
            else
            {
                return RenderTextureFormat.ARGBFloat;
            }
        }
        public static TextureFormat GetTexFormat(bool _grey)
        {
            if (_grey)
            {
                return TextureFormat.RGBAFloat;
            }
            else
            {
                return TextureFormat.RFloat;
            }
        }



        Texture2D m_Tex; //only used by TextureInput
        public RenderTexture m_Destination;
        public int m_Width = 1024;
        public int m_Height = 1024;
//        public float[] data;

//        public bool m_DataValid = false;
        public int m_Channels;


        public void SetTex(Texture2D _tex)
        {
            m_Tex = _tex;
//            m_DataValid = false;
        }
/*
        public float[] GetChannel(int channel)
        {
            float[] target = new float[data.Length / 4];
            int j = 0;
            for (int i = channel; i < data.Length; i += 4)
                target[j++] = data[i] * 256.0f;
            return target;
        }
        public void SetChannel(float[] src, int channel)
        {
            int j = 0;
            for (int i = channel; i < data.Length; i += 4)
                data[i] = src[j++] / 256.0f;

        }
*/
        public Texture GetHWSourceTexture()
        {
            if (m_Tex != null)
                return m_Tex;
            /*

                        if(m_Tex==null && m_Destination==null)
                        {
                            m_Tex=CreateTexture(ms_TexFormat);
                            return m_Tex;
                        }
            */
            return m_Destination;
        }

        public Texture2D CreateTexture(Color[] data,TextureFormat format= TextureFormat.ARGB32)
        {
            if (m_Tex == null)
            {
                Debug.LogError(" have to make Texture");
                m_Tex = new Texture2D(m_Width, m_Height, format, false);
            }

            m_Tex.filterMode = FilterMode.Bilinear;
            m_Tex.wrapMode = TextureWrapMode.Repeat;
            m_Tex.SetPixels(data);
            m_Tex.Apply();
            return m_Tex;
        }

        public bool IsGrey()
        {
            return m_Channels == 1;
        }

        void SetChannelCount(RenderTextureFormat _texFormat)
        {
            switch (_texFormat)
            {
                case RenderTextureFormat.R8:
                case RenderTextureFormat.RHalf:
                case RenderTextureFormat.RFloat:
                    m_Channels = 1;
                    break;
                default:
                    m_Channels = 4;
                    break;
            }
        }

        public RenderTexture CreateRenderDestination(int _width, int _height,  RenderTextureFormat _texFormat)
        {
//            _width = m_Width;
//            _height = m_Height;
            if (m_Destination == null||m_Destination.format!=_texFormat||_width!=m_Destination.width ||_height!=m_Destination.height)
            {
                m_Destination = new RenderTexture(_width, _height,0, _texFormat);
                m_Destination.wrapMode = TextureWrapMode.Repeat;
                SetChannelCount(_texFormat);

            }
            return m_Destination;
        }

 

        public TextureParam()
        {
            
        }
        public TextureParam(int w,int h)
        {
            m_Width = w;
            m_Height = h;
            
        }
    public TextureParam(TextureParam _other)
    {
        m_Width = _other.m_Width;
        m_Height = _other.m_Height;
        if (_other.m_Destination != null)
        {
            var rt = _other.m_Destination;
            m_Destination= CreateRenderDestination(rt.width,rt.height,rt.format);

            

        }

    }
    public void SavePNG(string path)
        {
            var tex = new Texture2D(m_Destination.width, m_Destination.height, TextureParam.ms_TexFormat, false);
            RenderTexture.active = m_Destination;
            tex.ReadPixels(new Rect(0, 0, m_Width, m_Height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
        }

        public Color[] AllocData()
        {
            return new Color[m_Width * m_Height * 4];
        }
    }
//}