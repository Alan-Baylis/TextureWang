using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;


public abstract class TextureMathOp : TextureNode
{
//    public enum TexOP { Add, ClipMin, Multiply, Power,Blur,CalcNormal,Level1,Transform,Gradient,Min,Max }
    public enum MathOp { Add = ShaderOp.AddRGB, Multiply = ShaderOp.MultRGB, Power = ShaderOp.Power, Min = ShaderOp.Min1, Max = ShaderOp.Max1 , Step = ShaderOp.Step, Invert = ShaderOp.Invert,Stepify=ShaderOp.Stepify,Transform=ShaderOp.Transform,EdgeDist = ShaderOp.EdgeDist,Smooth=ShaderOp.Smooth, BlackEdge = ShaderOp.BlackEdge,Splatter=ShaderOp.Splatter ,Circle=ShaderOp.Circle, SplatterGrid = ShaderOp.SplatterGrid,Sobel=ShaderOp.Sobel, MapCylinder =ShaderOp.MapCylinder,RandomEdge=ShaderOp.RandomEdge}

    public MathOp m_OpType = MathOp.Add;


    //public Texture m_Cached;

    public FloatRemap m_Value1  ;//where all 0.5
    public FloatRemap m_Value2  ;
    public FloatRemap m_Value3  ;
    public Gradient m_Gradient = new UnityEngine.Gradient();

    protected internal override void InspectorNodeGUI()
    {
        
    }

    protected virtual void CreateInputOutputs()
    {
        CreateInput("Texture1", "TextureParam", NodeSide.Left, 50);
        CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
    }


    private void OnGUI()
    {
        NodeGUI();
    }

    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();

    }

   


    public void General(float _r,float _g,float _b, TextureParam _input, TextureParam _output, ShaderOp _shaderOp)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        Material mat = GetMaterial("TextureOps");
        
//        if (Inputs.Count>1 && Inputs[1].connection != null)
//            _r=Inputs[1].connection.GetValue<float>();
        mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_input.m_Width, 1.0f / (float)_input.m_Height, _input.m_Width, 0));
        mat.SetVector("_Multiply", new Vector4(_r, _g, _b, 1));
        mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
        SetCommonVars(mat);

        CalcOutputFormat(_input);
        RenderTexture destination = CreateRenderDestination(_input, _output);
        Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)_shaderOp);
//        Debug.LogError(" multiply in Final" + timer.ElapsedMilliseconds + " ms");
    }
   
 
    public override bool Calculate()
    {
        if (!allInputsReady())
        {
           // Debug.LogError(" input no ready");
            return false;
        }
        TextureParam input = null;
        if (Inputs[0].connection != null)
            input = Inputs[0].connection.GetValue<TextureParam>();
        if (m_Param == null)
            m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        if (input == null)
            Debug.LogError(" input null");

        if (input != null && m_Param != null)
        {
    
             General(m_Value1, m_Value2, m_Value3, input, m_Param, (ShaderOp)m_OpType);

        }
        CreateCachedTextureIcon();
        //m_Cached = m_Param.GetHWSourceTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
    }
}