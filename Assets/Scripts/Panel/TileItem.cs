using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItem : MonoBehaviour,ICacheItem
{

    private object data;
    public object GetCacheItemData()
    {
        return this.data;
    }

    public  void SetData(object  data)
    {
        this.data = data;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
