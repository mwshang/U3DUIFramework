using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleItem : MonoBehaviour,ICacheItem
{
    private object data;
    public object GetTypeByAttri(string attri)
    {
        Debug.Log("GetValueByAttri data:"  +  this.data);
        return this.data.GetType().GetProperty(attri).GetValue(this.data);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetData(object data)
    {
        
        this.data = data;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public object GetCacheItemData()
    {
        return this.data;
    }
}
