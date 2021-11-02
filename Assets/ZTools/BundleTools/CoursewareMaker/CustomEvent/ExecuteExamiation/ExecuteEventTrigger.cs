using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExecuteEventTrigger : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public ExecuteExamiation executeExamiation;
    public int index;
    public bool triggerToggle;
    public Dictionary<string, Material[]> selfMaterial = new Dictionary<string, Material[]>();
    public MeshRenderer[] meshRenderers;


    // Start is called before the first frame update
    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        triggerToggle = true;
        executeExamiation = GetComponentInParent<ExecuteExamiation>();
        for (int i = 0;i < meshRenderers.Length;i++)
        {
            if (!selfMaterial.ContainsKey(meshRenderers[i].name) && !selfMaterial.ContainsValue(meshRenderers[i].materials))
            {
                selfMaterial.Add(meshRenderers[i].name, meshRenderers[i].materials);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (triggerToggle)
        {
            triggerToggle = false;
            EventTrigger eventTrigger = GetComponent<EventTrigger>();
            if (eventTrigger!=null)
            {
                Destroy(eventTrigger);
            }
            executeExamiation.CheckRight(index);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (triggerToggle)
        {
            for (int i = 0;i < meshRenderers.Length;i++)
            {
                Material[] temp = new Material[meshRenderers[i].materials.Length];
                for (int j = 0;j < meshRenderers[i].materials.Length;j++)
                {
                    temp[j] = executeExamiation.highlightmaterial;
                }
                meshRenderers[i].materials = temp;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (triggerToggle)
        {
            RestMateriaml();
        }
    }

    private void RestMateriaml()
    {
        for (int i = 0;i < selfMaterial.Count;i++)
        {
            Material[] mats = new Material[selfMaterial[meshRenderers[i].name].Length];
            mats = selfMaterial[meshRenderers[i].name];
            meshRenderers[i].materials = mats;
        }
    }
}
