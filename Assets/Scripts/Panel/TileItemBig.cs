using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItemBig : MonoBehaviour,ICacheItem
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private object data;
    public object GetCacheItemData()
    {
        return this.data;
    }

    public void SetData(object data)
    {
        this.data = data;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
