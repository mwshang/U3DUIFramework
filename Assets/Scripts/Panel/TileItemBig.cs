using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItemBig : MonoBehaviour,ICacheItem
{

    private static GameObject lastExpanedItem = null;

    private bool isSelected = true;

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

    public void OnFstLevelSelected()
    {
       // bool isSelected = (bool)arg.GetType().GetProperty("isSelected").GetValue(arg);
        //Debug.Log("isSelected:"+isSelected);
        RectTransform rt = this.transform as RectTransform;

        if (  false && lastExpanedItem != null && lastExpanedItem != this.gameObject)
        {
            ScrollViewExtension.ExpandCompleteCallback onComplete = () =>
            {
                if (isSelected)
                {
                    lastExpanedItem = this.gameObject;
                    this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
                }
                else
                {
                    //rt.DOSizeDelta(new Vector2(500, 235), 0.3f);
                    this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
                }
            };

            this.gameObject.SendMessageUpwards("OnExpandItem", new { item = lastExpanedItem, to = new Vector2(500, 235), onComplete = onComplete });
            lastExpanedItem = null;
        }
        else
        {
            if (isSelected)
            {
                lastExpanedItem = this.gameObject;
                this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(600, 335) });
            }
            else
            {
                this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(600, 200) });
            }
            isSelected = !isSelected;
        }


    }
}
