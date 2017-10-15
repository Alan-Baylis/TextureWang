using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PostProcessing;

    public class PreviewTextureWindow : EditorWindow
    {

        static List<PreviewTextureWindow> m_List=new List<PreviewTextureWindow>();
//        public RenderTexture m_Preview;
        public TextureNode m_Source;
        public Texture2D m_tex;
        public bool m_Locked;
        public bool m_Histogram=false;

        public float m_Scale = 1.0f;

        void OnDestroy()
        {
            if(m_Buffer!=null)
                m_Buffer.Release();

            m_List.Remove(this);
            if (m_Source != null)
                m_Source.RemoveRefreshWindow(this);
        }

        static ComputeShader m_ComputeShader2;
        ComputeBuffer m_Buffer;
        Material m_Material;
        RenderTexture m_HistogramTexture;

        void CreatePreviewHistogram(RenderTexture preview)
        {
            if (m_ComputeShader2 == null)
                m_ComputeShader2 = (ComputeShader)Resources.Load("EyeHistogram");
            var cs = m_ComputeShader2;
            if(m_Buffer==null)
                m_Buffer = new ComputeBuffer(512 * 1, sizeof(uint) << 2);
            int kernel = cs.FindKernel("KHistogramClear");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            cs.Dispatch(kernel, 1, 1, 1);

            kernel = cs.FindKernel("KHistogramGather");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            Texture source = m_Source.m_Cached;
            cs.SetTexture(kernel, "_Source", source);
            cs.SetInt("_IsLinear", GraphicsUtils.isLinearColorSpace ? 1 : 0);
            cs.SetVector("_Res", new Vector4(source.width, source.height, 0f, 0f));
            cs.SetVector("_Channels", new Vector4(1f, 1f, 1f, 0f));
        
            cs.Dispatch(kernel, Mathf.CeilToInt(source.width / 16f), Mathf.CeilToInt(source.height / 16f), 1);

            kernel = cs.FindKernel("KHistogramScale");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            cs.SetVector("_Res", new Vector4(512, 512, m_Scale, 0f));
            cs.Dispatch(kernel, 1, 1, 1);
            Material m = TextureNode.GetMaterial("TextureOps");
        
            m.SetVector("_Multiply",new Vector4(preview.width,preview.height,0,0));
            m.SetBuffer("_Histogram", m_Buffer);
            Graphics.Blit(m_Source.m_Cached, preview, m,  (int)ShaderOp.Histogram);
        }
        public static void Init(TextureNode _src)
        {

            PreviewTextureWindow window = null;
            foreach (var x in m_List)
            {
                if (x!=null && !x.m_Locked)
                {
                    window = x;
                    break;
                }
            }
            if (window == null)
            {
                window = CreateInstance<PreviewTextureWindow>();
                m_List.Add(window);
            }
            else
            {
                if (window.m_Source != null)
                    window.m_Source.RemoveRefreshWindow(window);
            }
            //GetWindow<PreviewTextureWindow>();//ScriptableObject.CreateInstance<PreviewTextureWindow>();
            _src.AddRefreshWindow( window);
            window.m_Source = _src;


//            window.m_Preview = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            //window.position = new Rect(_inst.canvasWindowRect.x+ _inst.canvasWindowRect.width*0.5f, _inst.canvasWindowRect.y + _inst.canvasWindowRect.height * 0.5f, 350, 250);
            window.titleContent=new GUIContent("Preview");
            window.Show();
            window.Repaint();

        }

        public void AllocTex(int width, int height)
        {
            m_tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            m_Locked = GUILayout.Toggle(m_Locked, "Locked");
            m_Histogram = GUILayout.Toggle(m_Histogram, "Histogram");
            m_Scale = EditorGUILayout.FloatField(m_Scale);
            GUILayout.EndHorizontal();
        //            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        if (m_Source == null||m_Source.m_Param == null|| m_Source.m_Cached==null)
            return;
        int wantWidth = m_Source.m_Cached.width;
        int wantHeight = m_Source.m_Cached.height;

        if (m_Histogram)
        {
            wantWidth = 512;
            wantHeight = 512;
        }

        if (m_tex == null || m_tex.width != wantWidth || m_tex.height != wantHeight)
            AllocTex(wantWidth,wantHeight);

        RenderTexture preview = RenderTexture.GetTemporary(wantWidth, wantHeight, 0,RenderTextureFormat.ARGB32);

        Material m = TextureNode.GetMaterial("TextureOps");
        m.SetVector("_Multiply", new Vector4(1.0f, 0, 0, 0));
        if (m_Histogram)
        {
            CreatePreviewHistogram(preview);
        }
        else
        {
            Graphics.Blit(m_Source.m_Cached, preview, m,m_Source.m_TexMode == TextureNode.TexMode.Greyscale
                    ? (int) ShaderOp.CopyGrey: (int) ShaderOp.CopyColor);
        }
        m_tex.ReadPixels(new Rect(0, 0, wantWidth, wantHeight), 0, 0);
        m_tex.Apply();
        RenderTexture.active = null;

        //            EditorGUILayout.LabelField("\n Warning: Erases Current Canvas", EditorStyles.wordWrappedLabel);
        //            EditorGUILayout.Separator();
        Rect texRect = new Rect(2, 20, position.width - 4, position.height - 24);
        GUILayout.BeginArea(texRect, GUI.skin.box);
        GUI.DrawTexture(texRect, m_tex,ScaleMode.StretchToFill);//ScaleMode.StretchToFill);
        //GUI.DrawTexture(texRect, m_Preview, ScaleMode.ScaleToFit);//ScaleMode.StretchToFill);
        GUILayout.EndArea();
        RenderTexture.ReleaseTemporary(preview);
        }
    }
