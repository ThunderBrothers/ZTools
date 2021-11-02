using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetMaterialTemplate : BundleEventInfoParameterBase
{
    public Material materialTarget;
    public Dictionary<string, Material[]> selfMaterial = new Dictionary<string, Material[]>();
    public MeshRenderer[] meshRenderers;


    void Start()
    {
        Transform tra = transform.parent.Find("TargetMaterial");
        if (tra == null)
        {
            Debug.LogError($"{gameObject.name} SetMaterialTemplate  transform.parent.Find(TargetMaterial) NULL");
        }else
        {
            materialTarget = tra.GetComponent<MeshRenderer>().material;
        }
       
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers == null)
        {
            Debug.LogError($"{gameObject.name} GetComponentsInChildren<MeshRenderer>()  NULL");
        }
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (!selfMaterial.ContainsKey(meshRenderers[i].name) && !selfMaterial.ContainsValue(meshRenderers[i].materials))
            {
                selfMaterial.Add(meshRenderers[i].name, meshRenderers[i].materials);
            }         
        }
    }

    private void RestMateriaml()
    {
        for (int i = 0; i < selfMaterial.Count; i++)
        {
            Material[] mats = new Material[selfMaterial[meshRenderers[i].name].Length];
            mats = selfMaterial[meshRenderers[i].name];
            meshRenderers[i].materials = mats;
        }
    }

    public override void OnBundleAction(PointerEventData eventData)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, int _IntParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, string _StringParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter)
    {
        if (_BoolParameter)
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Material[] temp = new Material[meshRenderers[i].materials.Length];
                for (int j = 0; j < meshRenderers[i].materials.Length; j++)
                {
                    temp[j] = materialTarget;
                }
                meshRenderers[i].materials = temp;
            }
        }
        else
        {
            RestMateriaml();
        }
    }

    public override void OnReceiveMsg(string msg)
    {

    }

    public override bool supportPRS()
    {
        return true;
    }
}
