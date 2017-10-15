using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System.Collections.Generic;

[Node (false, "Standard/Example/UnityTextureDrawGradientInput")]
public class UnityTextureDrawGradientInput : TextureNode
{
	public const string ID = "UnityTextureDrawGradientInput";
	public override string GetID { get { return ID; } }

    public Texture2D m_Input;


    public TextureParam m_Param;
    //public Texture2D m_Cached;

    List<Color> m_GradientCols = new List<Color>();
    List<Vector2> m_GradientPos = new List<Vector2>();
    private Color[] data;
    public override Node Create (Vector2 pos) 
	{

        UnityTextureDrawGradientInput node = CreateInstance<UnityTextureDrawGradientInput> ();
        node.m_Param = new TextureParam(256,1);
        node.data=node.m_Param.AllocData();
        node.rect = new Rect (pos.x, pos.y, 250, 350);
		node.name = "UnityTextureDrawGradientInput";
		
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
	}
    int m_PrevX = -1;
    int m_PrevY = -1;
    protected internal override void InspectorNodeGUI() 
	{
        if (Event.current.type == EventType.MouseDrag && m_Input != null)
        {
            float px = (Event.current.mousePosition.x/250.0f)*m_Input.width;
            float py = (1.0f-((Event.current.mousePosition.y-90) / 250.0f)) * m_Input.height;
            int ipx = (int)px;
            int ipy = (int)py;


            Color col = m_Input.GetPixel(ipx, ipy);
            //m_Input.SetPixel((int)px, (int)py,Color.black);
            if (ipx != px && ipy != py && Event.current.mousePosition.y>90)
            {
                m_GradientCols.Add(col);
                m_GradientPos.Add(new Vector2(px, py));
                m_PrevX = ipx;
                m_PrevY = ipy;
                //Debug.Log("Current detected event: " + Event.current.mousePosition + " col " + col + " px " + px + " py " + py + " m_Input.width " + m_Input.width + " m_Input.height " + m_Input.height);
                //Calculate();
            }

//            m_Input.Apply();

        }
/*
        if (GUILayout.Button("Clear"))
        {
            m_GradientCols.Clear();
            m_GradientPos.Clear();
        }
        if (GUILayout.Button("Calc"))
            NodeEditor.RecalculateFrom(this);
*/
//        GUILayout.BeginArea(new Rect(0, 155, 250, 25));
//        if (m_Cached != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 250, 250), m_Cached,ScaleMode.ScaleToFit);
            //            GUILayout.Label(m_Cached);
        }
//        GUILayout.EndArea();




#if UNITY_EDITOR
        //m_Input = (Texture2D) EditorGUI.ObjectField(new Rect(0, 190, 250, 250), m_Input, typeof(Texture2D),false);
#endif
        if (m_Cached != null)
        {
            GUILayout.Label(m_Cached);
        }
        if (m_Input != null)
        {
            foreach (var p in m_GradientPos)
            {
                float px = (p.x / m_Input.width) * 250.0f;
                float py = 250.0f-(p.y / m_Input.height) * 250.0f + 90.0f;
                GUI.DrawTexture(new Rect(px, py, 2, 2), m_Input);
            }
        }

    }

    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();


        // if (m_Cached != null)
        //   GUILayout.Label(m_Cached);
        if (GUILayout.Button("Clear"))
        {
            m_GradientCols.Clear();
            m_GradientPos.Clear();
        }
        m_Input = (Texture2D)EditorGUI.ObjectField(new Rect(0, 390, 250, 250), m_Input, typeof(Texture2D), false);
        foreach (var p in m_GradientPos)
        {
            float px = (p.x / m_Input.width) * 250.0f;
            float py = 250.0f - (p.y / m_Input.height) * 250.0f + 90.0f;
            GUI.DrawTexture(new Rect(px, py, 2, 2), m_Input);
        }


        /*
                if (m_Input != null)
                {
                    GUILayout.BeginArea(new Rect(0, 355, 250, 250));
                    GUILayout.Label(m_Input);
                    GUILayout.EndArea();

                }
        */


    }
    private void OnGUI()
    {
        if (Event.current.type == EventType.MouseDrag && m_Input != null)
        {
            float px = (Event.current.mousePosition.x/250.0f)*m_Input.width;
            float py = (1.0f - ((Event.current.mousePosition.y - 90)/250.0f))*m_Input.height;
            

        }

        NodeGUI();
    }

    protected internal override void NodeGUI()
    {
        if (Event.current.type == EventType.MouseDrag && m_Input != null)
        {
            float px = (Event.current.mousePosition.x/250.0f);     
            float py = ( ((Event.current.mousePosition.y) / 250.0f));
            if (px >= 0.0f && py >= 0.0f && px <= 1.0f && py <= 1.0f)
            {
                px *= m_Input.width;
                py *= m_Input.height;
                int ipx = (int) px;
                int ipy = (int) py;


                Color col = m_Input.GetPixel(ipx, m_Input.height-ipy);
               
                if (ipx != m_PrevX && ipy != m_PrevY && Event.current.mousePosition.y > 0)
                {
                    m_GradientCols.Add(col);
                    m_GradientPos.Add(new Vector2(px, py));
                    m_PrevX = ipx;
                    m_PrevY = ipy;

                    //                Debug.Log(" mouse click " + px + " " + py + " mouse: " + Event.current.mousePosition + " col " + col);
                    //Debug.Log("Current detected event: " + Event.current.mousePosition + " col " + col + " px " + px + " py " + py + " m_Input.width " + m_Input.width + " m_Input.height " + m_Input.height);
                    //Calculate();
                }
            }
        }

//        if (m_Input != null)
//            GUILayout.Label(m_Input);
        if (m_Input != null)
            GUI.DrawTexture(new Rect(0, 0, 250, 250), m_Input, ScaleMode.ScaleToFit);

        float prevX =0;
        float prevY =0;
        for (int index = 0; index < m_GradientPos.Count; index++)
        {
            var p = m_GradientPos[index];
            float px = (p.x/m_Input.width)*250.0f;
            float py = (p.y/m_Input.height)*250.0f; // + 90.0f;
            //GUI.DrawTexture(new Rect(px, py, 2, 2), m_Input);
            if (index > 0)
            {
                UnityEditor.Handles.color = m_GradientCols[index];
                UnityEditor.Handles.DrawLine(new Vector2(prevX, prevY), new Vector2(px, py));
                    //.DrawAAPolyLine(m_GradientPos.ToArray());
            }
            prevX = px;
            prevY = py;
        }


//        base.NodeGUI();
    }

    public override bool Calculate()
    {
        if (!allInputsReady())
            return false;

        if (m_Input == null)
            return false;
        if (m_Param == null)
            m_Param = new TextureParam(256,1);
        if (m_GradientCols.Count > 2)
        {
            try
            {

                for (float t = 0; t < 256.0; t += 1.0f)///256.0f)
                {
                    float along = (float)(m_GradientCols.Count - 2) / 256.0f;

                    int index = (int)(t * along);
                    float blend = (t * along) - index;
                    Color lerp = Color.Lerp(m_GradientCols[index], m_GradientCols[index + 1], blend);
                    data[(int)t] = lerp;

                }

            }

            catch (System.Exception _ex)
            {
                Debug.LogError("exception caught: " + _ex);
            }
        }
    
        m_Cached = m_Param.CreateTexture(data);

        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
