﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor;


public abstract class TextureNode : Node
{
    private const string  m_Help = "";
    public virtual string GetHelp() { return m_Help; }

    private List<EditorWindow> m_Refresh;
    protected int m_NodeWidth = 128;
    protected int m_NodeHeight = 142;

    public enum TexMode { ColorRGB,Greyscale }
    public Texture m_Cached;

    public TexMode m_TexMode= TexMode.Greyscale;
    public int m_TexWidth=1024;
    public int m_TexHeight=1024;

    public TextureParam m_Param;
    public bool m_Saturate=true;
    public bool m_InvertInput;
    public bool m_ClampInputUV;
    public bool m_InvertOutput;
    public FloatRemap m_InputMin = new FloatRemap(0.0f,0,1);
    public FloatRemap m_InputMax = new FloatRemap(1.0f,0,1);
    public FloatRemap m_OutputMin = new FloatRemap(0.0f, 0, 1);
    public FloatRemap m_OutputGamma = new FloatRemap(1.0f, 0, 10.0f);
    public FloatRemap m_OutputMax = new FloatRemap(1.0f, 0, 1);
    public FloatRemap m_ScalePreview = new FloatRemap(1.0f);
    private bool m_ShowLevels;
    private bool m_ShowNames;

    private static Material ms_UIMaterial;

    public void AddRefreshWindow(EditorWindow _w)
    {
        if (m_Refresh == null)
            m_Refresh=new List<EditorWindow>();

        if (!m_Refresh.Contains(_w))
            m_Refresh.Add(_w);
    }
    public void RemoveRefreshWindow(EditorWindow _w)
    {
        if (m_Refresh != null)
        {
            if (m_Refresh.Contains(_w))
                m_Refresh.Remove(_w);
        }
    }
    protected internal override void CopyScriptableObjects(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
        ConnectRemapFloats(this,replaceSerializableObject);
    }

    public static void ConnectRemapFloats(Node _n,Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
// Get the fields of the specified class.
        FieldInfo[] myField = _n.GetType().GetFields();
        foreach (var x in myField)
        {
            // if(x.FieldType is FloatRemap)
            if (x.GetValue(_n) is FloatRemap)
            {
                FloatRemap fr = (FloatRemap) x.GetValue(_n); //its a struct this makes a copy
                //                Debug.Log(x + " " + x.FieldType + " " + x.GetType() + " " + x.ReflectedType+" fr val:"+fr.m_Value);
                if (fr.m_ReplaceWithInput && fr.m_Replacement == null)
                {
                    Debug.LogError(" wants to be replaced but isnt linked ");
                }
                else if (fr.m_Replacement != null)
                {
                    NodeKnob knob = fr.m_Replacement;
                    NodeInput replace = replaceSerializableObject.Invoke(knob) as NodeInput;
                    if (replace != null)
                    {
                        fr.m_Replacement = replace;
                    }
                    else
                    {
                        fr.m_Replacement = null;
                    }
                }
                x.SetValue(_n, fr); //its a god damn struct it needs to be saved back out
            }
        }
    }

    protected internal override void DrawConnections()
    {
        CheckNodeKnobMigration();
        if (Event.current.type != EventType.Repaint)
            return;
        foreach (NodeOutput output in Outputs)
        {
            if (output == null)
                continue;
            Vector2 startPos = output.GetGUIKnob().center;
            Vector2 startDir = output.GetDirection();

            foreach (NodeInput input in output.connections)
            {
                if (input != null)
                {
                    if (m_TexMode == TexMode.Greyscale)
                    {

                        NodeEditorGUI.DrawConnection(startPos,
                            startDir,
                            input.GetGUIKnob().center,
                            input.GetDirection(),
                            Color.red);
                    }
                    else
                    if (m_TexMode != TexMode.Greyscale)
                    {
                        NodeEditorGUI.DrawConnection(startPos + new Vector2(0, -3),
                            startDir,
                            input.GetGUIKnob().center + new Vector2(0, -3),
                            input.GetDirection(),
                            Color.red);

                        NodeEditorGUI.DrawConnection(startPos+new Vector2(0,0),
                            startDir,
                            input.GetGUIKnob().center + new Vector2(0, 0),
                            input.GetDirection(),
                            Color.green);
                        NodeEditorGUI.DrawConnection(startPos + new Vector2(0,3),
                            startDir,
                            input.GetGUIKnob().center + new Vector2(0, 3),
                            input.GetDirection(),
                            Color.blue);
                    }


                    //                        EditorGUI.LabelField(new Rect(input.GetGUIKnob().center-new Vector2(50,20), new Vector2(200, 50)), input.name);

                }
            }
        }
        foreach (var input in Inputs)
        {
            if(input!=null)
                EditorGUI.LabelField(new Rect(input.GetGUIKnob().center - new Vector2(50, 20), new Vector2(200, 50)), input.name);
        }


    }
    protected override void InitBase()
    {
        m_TexWidth = 1024;//miked NodeEditor.curNodeCanvas.m_TexWidth;
        m_TexHeight = 1024; //miked NodeEditor.curNodeCanvas.m_TexHeight;
        m_Param = new TextureParam(m_TexWidth, m_TexHeight);
        base.InitBase();

    }

    protected virtual void CreateInputOutputs()
    {
        CreateInput("Texture1", "TextureParam", NodeSide.Left, 50);
        CreateInput("Texture2", "TextureParam", NodeSide.Left, 70);
        CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
    }

    public virtual void SetCommonVars(Material _mat)
    {
        SetUniqueVars(_mat);
        _mat.SetInt("_InvertInput", m_InvertInput?1:0);
        _mat.SetInt("_ClampInputUV", m_ClampInputUV ? 1 : 0);
        _mat.SetInt("_InvertOutput", m_InvertOutput ? 1 : 0);
        _mat.SetInt("_Saturate", m_Saturate ? 1 : 0);
        _mat.SetVector("_ScaleOutput", new Vector4(m_InputMin, m_InputMax, m_OutputMin, m_OutputMax));
        _mat.SetVector("_ScaleOutput2", new Vector4(m_OutputGamma, 0,0,0));
    }

    public virtual void SetUniqueVars(Material _mat)
    {
        
    }
    protected internal override void NodeGUI()
    {
/*
        GUILayout.Label("This is a custom Node!");

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if(Inputs.Count>0)
            Inputs[0].DisplayLayout();

        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        Outputs[0].DisplayLayout();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
*/
        //draw the texture in the dragable node box

        if (m_Cached != null)
        {
            
            if (Event.current.type == EventType.Repaint)
            {

                if (m_TexMode == TexMode.Greyscale)
                    Graphics.DrawTexture(new Rect(2, 3, m_NodeWidth - 4, m_NodeHeight - 24), m_Cached,GetMaterial("UIMaterial"), m_TexMode == TexMode.Greyscale ? 0 : 1);
                else
                    GUI.DrawTexture(new Rect(2, 3, m_NodeWidth - 4, m_NodeHeight - 14), m_Cached, ScaleMode.StretchToFill);
                    
            }
        }
//miked                NodeEditor.curNodeCanvas.scaleMode);//ScaleMode.StretchToFill);

        //            GUILayout.Label(m_Cached);
    }

    protected internal abstract void InspectorNodeGUI();
    static string ms_PathName;
    public override void DrawNodePropertyEditor()
    {
        //GUI.changed = false;
        if (GUILayout.Button("save png"))
        {

            ms_PathName = EditorUtility.SaveFilePanel("SavePNG", "Assets/", ms_PathName, "png");

            m_Param.SavePNG(ms_PathName);
        }
        m_TexMode = (TexMode)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Colors", "3 components per texture or one"), m_TexMode, GUILayout.MaxWidth(300));
        m_ShowNames = EditorGUILayout.Foldout(m_ShowNames, "Input Rename:");
        if (m_ShowNames)
        {
            foreach (var x in Inputs)
            {
                if (x == null)
                    continue;
                GUILayout.BeginHorizontal();
                x.name = (string)GUILayout.TextField(x.name);
                GUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.LabelField("TexWidth");
        m_TexWidth=(int)EditorGUILayout.Slider(m_TexWidth, 1.0f, 2048.0f);
        EditorGUILayout.LabelField("TexHeight");
        m_TexHeight = (int)EditorGUILayout.Slider(m_TexHeight, 1.0f, 2048.0f);
        GUI.skin.button.wordWrap = true;
        //EditorGUI.TextArea(new Rect(10, 10, 200, 100),"Info: " +GetHelp());
//        Rect r=EditorGUILayout.BeginVertical(GUILayout.Height(100), GUILayout.Width(100));
        GUILayout.TextArea("Info: " + GetHelp(), GUILayout.Height(100), GUILayout.Width(300));
//        EditorGUILayout.EndVertical();

/*
        if (m_Param!=null)
            GUILayout.Label("outParam: Width: " + m_Param.m_Width + "outParam: Height: " + m_Param.m_Height);

        if (m_Param != null && m_Param.m_Destination!=null)
            GUILayout.Label("HWDest: Width: " + m_Param.m_Destination.width + "HWDest: Height: " + m_Param.m_Destination.height+" fmt:"+ m_Param.m_Destination.format, EditorStyles.wordWrappedLabel);
*/
        m_Saturate = GUILayout.Toggle(m_Saturate, "Clip result to 0..1");
        m_ClampInputUV = GUILayout.Toggle(m_ClampInputUV, "Clamp Input UV");
        m_InvertInput = GUILayout.Toggle(m_InvertInput, "Invert Input");
        m_InvertOutput = GUILayout.Toggle(m_InvertOutput, "Invert Output");

        
        m_ShowLevels = EditorGUILayout.Foldout(m_ShowLevels, "Levels:");
        if (m_ShowLevels)
        {
            m_InputMin.SliderLabel(this, "Scale Output In Min");
            m_InputMax.SliderLabel(this, "Scale Output In Max");
            m_OutputMin.SliderLabel(this, "Scale Output Out Min");
            m_OutputGamma.SliderLabel(this, "Scale Output Gamma");
            m_OutputMax.SliderLabel(this, "Scale Output Out Max");
        }


        EditorGUILayout.Space();
        RTEditorGUI.Seperator();

    }
    protected void CalcOutputFormat(TextureParam _input, TextureParam _inputB)
    {
        if (/*_input.IsGrey() && _inputB.IsGrey() &&*/ m_TexMode != TexMode.ColorRGB)
        {
            m_TexMode = TexMode.Greyscale;
        }
        else
        {
            m_TexMode = TexMode.ColorRGB;
        }
    }
    protected void CalcOutputFormat(TextureParam _input)
    {
        if (/*_input.IsGrey()&&*/m_TexMode!=TexMode.ColorRGB )
        {
            m_TexMode = TexMode.Greyscale;
        }
        else
        {
            m_TexMode = TexMode.ColorRGB;
        }
    }

    protected virtual void PostDrawNodePropertyEditor()
    {
/*
        if (m_Param != null)
        {
            Texture tex = CreateTextureIcon(256);

            GUILayout.Label(tex);
        }
*/
    }


    static readonly Dictionary<string,Material> ms_MaterialDictionary = new Dictionary<string, Material>();

    protected RenderTexture CreateRenderDestination(TextureParam _input,TextureParam _output)
    {
        if(_input == _output)
            Debug.LogError(" using input as output ");
        return _output.CreateRenderDestination(m_TexWidth, m_TexHeight,  TextureParam.GetRTFormat(m_TexMode==TexMode.Greyscale));
    }

    static public Material GetMaterial(string _shader)
    {
        Material ret;
        if (!ms_MaterialDictionary.TryGetValue(_shader, out ret))
        {
            ret = new Material(Shader.Find("Hidden/" + _shader)) {hideFlags = HideFlags.DontSave};
            ms_MaterialDictionary.Add(_shader, ret);
        }
        return ret;

    }
    public Texture CreateTextureIcon(int _size)
    {
        if (m_Param == null)
            return null;
        return m_Param.m_Destination;//GetHWSourceTexture();
#if OUTPUT_SCALED_GREY
        //this is just for display as a label on the node
        RenderTexture destination = new RenderTexture(_size, _size, 24, RenderTextureFormat.ARGB32);
        Material m = GetMaterial("TextureOps");
        m.SetVector("_Multiply",new Vector4(m_ScalePreview,0,0,0));
        Graphics.Blit(m_Param.GetHWSourceTexture(), destination, m, m_Param.IsGrey() ? (int)ShaderOp.CopyGrey : (int)ShaderOp.CopyColor);

        return destination;
#endif

    }
    public virtual void OnLoadCanvas()
    {
       
        
    }

    public void OpenPreview()
    {
        PreviewTextureWindow.Init(this);
    }

    public void CreateCachedTextureIcon()
    {
        m_Cached = CreateTextureIcon(1024);
        if (m_Refresh != null)
        {
            foreach (var w in m_Refresh)
            {
                if(w!=null)
                    w.Repaint();
            }
        }
    }
    

}

