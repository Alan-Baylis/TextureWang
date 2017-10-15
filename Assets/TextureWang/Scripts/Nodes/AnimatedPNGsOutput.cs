using System.Diagnostics;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MyWindow : EditorWindow
{
    float m_FPS = 15;
    private int m_Frame = 0;
    ScaleMode m_ScaleMode=ScaleMode.StretchToFill;
    public Stopwatch m_SW;
    public bool m_Animate;

    public AnimatedPNGsOutput m_Src;
    public RenderTexture[] m_Frames;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    public static void Init(AnimatedPNGsOutput _src,RenderTexture[] frames)
    {
        // Get existing open window or if none, make a new one:
        MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow));
        window.m_Src = _src;
        window.m_Frames = frames;
        window.m_SW=Stopwatch.StartNew();
        window.Show();
    }

    void OnGUI()
    {
        float size = Mathf.Min(position.width-20, position.height-100);
        GUI.BeginGroup(new Rect(0,0, size, size));
        if (m_Frames != null && m_Frames.Length > 0)
        {
            Texture t = m_Frames[m_Frame%m_Frames.Length]; //m_Src.GenerateFrame(m_Frame);
            if (t != null)
                GUI.DrawTexture(new Rect(0, 0, size, size), t, ScaleMode.StretchToFill); //ScaleMode.StretchToFill);
        }
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0, size, 600, 100));
        m_FPS = EditorGUILayout.Slider("FPS", m_FPS, 1, 144);
        m_Frame = (int)EditorGUILayout.Slider("Frame", m_Frame, 0, m_Frames.Length);
        m_Animate = EditorGUILayout.Toggle("Animate", m_Animate);
        m_ScaleMode = (ScaleMode)EditorGUILayout.EnumPopup(new GUIContent("ScaleMode", ""), m_ScaleMode, GUILayout.MaxWidth(200));
        GUI.EndGroup();
        if (m_Animate && m_SW.ElapsedMilliseconds > 1000/m_FPS)
        {
            m_Frame++;
            if (m_Frame >= m_Frames.Length)
                m_Frame = 0;
            m_SW.Reset();
            m_SW.Start();
        }
        Repaint();
        /*
                GUILayout.Label("Base Settings", EditorStyles.boldLabel);
                myString = EditorGUILayout.TextField("Text Field", myString);

                groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
                myBool = EditorGUILayout.Toggle("Toggle", myBool);
                myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
                EditorGUILayout.EndToggleGroup();
        */
    }
}

[Node (false, "Output/AnimatedPNGsOutput")]
public class AnimatedPNGsOutput : TextureNode
{
    public const string ID = "AnimatedPNGsOutput";
    public override string GetID { get { return ID; } }

    public string m_PathName="";

    public float m_StartAnimatedValue;
    public float m_EndAnimatedValue;
    public int m_FrameCount;







    //public Texture2D m_Cached;

    public override Node Create (Vector2 pos) 
    {

        AnimatedPNGsOutput node = CreateInstance<AnimatedPNGsOutput> ();
        
        node.rect = new Rect (pos.x, pos.y, 150, 150);
        node.name = "AnimatedPNGsOutput";
        node.CreateInput("RGB", "TextureParam", NodeSide.Left, 50);
        node.CreateInput("Alpha", "TextureParam", NodeSide.Left, 70);
        node.CreateInput("AnimatedValue", "Float", NodeSide.Left, 90);
        return node;
    }

    protected internal override void InspectorNodeGUI()
    {
    }

    public Texture GenerateFrame(int _frame)
    {

        _frame = _frame%m_FrameCount;
        InputNode m_AnimatedValue = Inputs[2].connection.body as InputNode;
        if (m_AnimatedValue != null)
        {

            if (m_FrameCount > 0 && m_FrameCount < 500)
            {
                
                float step = (m_EndAnimatedValue - m_StartAnimatedValue)/m_FrameCount;
                //for (float t = m_StartAnimatedValue; t < m_EndAnimatedValue; t += step)
                float t = m_StartAnimatedValue + step*_frame;
                {
                    m_AnimatedValue.value = t;//m_Value.Set(t);
                    NodeEditor.RecalculateFrom(m_AnimatedValue);

                    if (m_Param != null && m_Param.m_Destination != null)
                    {
                        return m_Param.m_Destination;

                    }
                }
            }
        }
        return null;
    }

    public override void DrawNodePropertyEditor() 
    {
        base.DrawNodePropertyEditor();

        m_StartAnimatedValue = RTEditorGUI.Slider("Start Animated Value ", m_StartAnimatedValue, -10, 10);
        m_EndAnimatedValue = RTEditorGUI.Slider("End Animated Value ", m_EndAnimatedValue, -10, 10);
        m_FrameCount=(int)RTEditorGUI.Slider("Frame Count ", m_FrameCount, 1, 64);

        if (GUILayout.Button("Choose OutputPath"))
        {
            
            m_PathName = EditorUtility.SaveFilePanel("SavePNG", "Assets/", m_PathName, "png");

        }
        if (GUILayout.Button("preview"))
        {
            if (m_Param == null)
            {
                NodeEditor.RecalculateFrom(this);
            }
                
            
            RenderTexture[] frames= new RenderTexture[m_FrameCount];
            InputNode m_AnimatedValue = Inputs[2].connection.body as InputNode;
            if (m_AnimatedValue != null)
            {

                if (!string.IsNullOrEmpty(m_PathName) && m_FrameCount > 0 && m_FrameCount < 500)
                {
                    Material m = GetMaterial("TextureOps");
                    m.SetInt("_MainIsGrey", m_Param.IsGrey() ? 1 : 0);
                    int count = 0;
                    float step = (m_EndAnimatedValue - m_StartAnimatedValue) / m_FrameCount;
                    for (float t = m_StartAnimatedValue; t < m_EndAnimatedValue; t += step)
                    {
                        m_AnimatedValue.value=t;//.Set(t);
                        NodeEditor.RecalculateFrom(m_AnimatedValue);

                        if (m_Param != null && m_Param.m_Destination != null)
                        {
                            RenderTexture rt=new RenderTexture(m_Param.m_Width,m_Param.m_Height,0,RenderTextureFormat.ARGB32);
                            frames[count] = rt;
                            Graphics.Blit(m_Param.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyColorAndAlpha);
                            count++;

                        }
                    }
                }
            }


            MyWindow.Init(this, frames);
        }

        if (GUILayout.Button("save png's "))
        {
            

            InputNode m_AnimatedValue = Inputs[2].connection.body as InputNode;
            if (m_AnimatedValue != null)
            {
                
                if (!string.IsNullOrEmpty(m_PathName)&& m_FrameCount>0 && m_FrameCount<500)
                {
                    int count = 0;
                    float step = (m_EndAnimatedValue - m_StartAnimatedValue)/m_FrameCount;
                    for (float t = m_StartAnimatedValue; t < m_EndAnimatedValue; t += step)
                    {
                        m_AnimatedValue.value = t;//m_Value.Set(t);
                        NodeEditor.RecalculateFrom(m_AnimatedValue);

                        if (m_Param != null && m_Param.m_Destination != null)
                        {
                            string pathrename;
                            if(count<10)
                                pathrename = m_PathName.Replace(".png", "0" + count + ".png");
                            else
                                pathrename = m_PathName.Replace(".png", "" + count + ".png");
                            count++;
                            m_Param.SavePNG(pathrename);
                        }
                        else
                        {
                            Debug.LogError(" null m_Param after rebuild all "+m_Param);
                            break;
                        }
                    }
                }
            }

        }
#if UNITY_EDITOR
        m_PathName = (string)GUILayout.TextField(m_PathName);
#endif


        /*
                GUILayout.BeginArea(new Rect(0, 40, 150, 256));
                if (m_Cached != null)
                {
                    GUILayout.Label(m_Cached);
                }
                GUILayout.EndArea();
        */

    }
    protected internal override void NodeGUI()
    {

        base.NodeGUI();
    }
    public override bool Calculate()
    {
        if (!allInputsReady())
            return false;

        TextureParam input = null;
        if (Inputs[0].connection != null)
            input = Inputs[0].connection.GetValue<TextureParam>();
        if (input == null)
            return false;
        TextureParam input2 = null;
        int index2 = 1;
        if (Inputs.Count < 2)
            index2 = 0;

        if (Inputs[index2].connection != null)
            input2 = Inputs[index2].connection.GetValue<TextureParam>();
        if (input2 == null)
            return false;

        if (m_Param == null)
            m_Param = new TextureParam(m_TexWidth, m_TexHeight);
        m_TexMode=TexMode.ColorRGB;

        Material m = GetMaterial("TextureOps");
        m.SetInt("_MainIsGrey", input.IsGrey() ? 1 : 0);
        m.SetInt("_TextureBIsGrey", input2.IsGrey() ? 1 : 0);
        m.SetTexture("_GradientTex", input2.GetHWSourceTexture());
        Graphics.Blit(input.GetHWSourceTexture(), CreateRenderDestination(input, m_Param), m, (int)ShaderOp.CopyColorAndAlpha);

        CreateCachedTextureIcon();
        


        return true;
    }
}