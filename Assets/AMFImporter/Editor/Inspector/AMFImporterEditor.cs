using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
namespace AdjutantSharp{
[CustomEditor(typeof(AMFImporter))]
public class AMFImporterEditor : ScriptedImporterTabbedEditor
{
    /* public override void OnInspectorGUI(){
        DrawDefaultInspector();
    } */

    public override void OnEnable()
    {
        if (tabs == null)
        {
            tabs = new BaseAssetImporterTabUI[] { new AMFImporterModelEditor(this), new AMFImporterRigEditor(this),new AMFInspector(this)};//,  new AMFImporterMaterialEditor(this) };
            m_TabNames = new string[] {"Model", "Rig","Raw Data"};//,  "Materials"};
        }
        base.OnEnable();
    }

    public override void OnDisable()
    {
        foreach (var tab in tabs)
        {
            tab.OnDisable();
        }
        base.OnDisable();
    }
    public override bool HasPreviewGUI()
    {
        return base.HasPreviewGUI() && targets.Length < 2;
    }

  
    public override GUIContent GetPreviewTitle()
    {
        

        return base.GetPreviewTitle();
    }

    // Only show the imported GameObject when the Model tab is active; not when the Animation tab is active
    public override bool showImportedObject { get { return activeTab is AMFImporterModelEditor; } }
}
}