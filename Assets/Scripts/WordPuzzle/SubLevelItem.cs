using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SubLevelItem : MonoBehaviour
{

    private Outline outline;
    private Outline textOutline;
    private Text SubName;
    private Toggle toggle;

    private object _data;



    private void Awake()
    {
        Transform subName = this.transform.Find("SubName");
        SubName = subName.GetComponent<Text>();
        textOutline = subName.GetComponent<Outline>();

        outline = GetComponent<Outline>();

        toggle = GetComponent<Toggle>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetSelected(bool v)
    {        
        outline.enabled = v;
        textOutline.enabled = v;
    }

    public void SetData(object arg)
    {
        this._data = arg;
    }

    public void OnValueChanged()
    {
        this.SetSelected(toggle.isOn);
        this.SendMessageUpwards("OnFstLevelSelected", new {data=this._data,isSelected=toggle.isOn });
    }
}
