using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "TwoInput/SrcBlend")]
public class Texture2SrcBlend : Texture2MathOp
{
    public const string ID = "Texture2SrcBlend";
    public override string GetID { get { return ID; } }

    public override Node Create (Vector2 pos) 
    {

        Texture2SrcBlend node = CreateInstance<Texture2SrcBlend> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "2SrcBlend";
        node.CreateInputOutputs();
        node.m_OpType=MathOp.SrcBlend;
//miked        node.m_Doc ="Add second texture to first using first textures brightness as a blend value, multiplied by user setting";
        return node;
    }


}