﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class LevelItem : MonoBehaviour,ICacheItem
{
    private Transform shadow;
    private Transform container;

    private Text Title;
    private Text SndTitle;
    private Transform FstLevels;
    private Transform SndtLevels;

    private static GameObject lastExpanedItem  = null;

    private object data;


    private void Awake()
    {
        shadow = this.transform.Find("shadow");
        container = this.transform.Find("Mask");

        Title = container.Find("Title").GetComponent<Text>();
        SndTitle = container.Find("SndTitle").GetComponent<Text>();
        FstLevels = container.Find("FstLevels");
        SndtLevels = container.Find("SndtLevels");
    }

    public void SetData(object data)
    {
        if (SndTitle)
        {
            SndTitle.text = data.ToString();
        }
        this.data = data;


    }
    public void OnFstLevelSelected(object arg)
    {
        bool isSelected = (bool)arg.GetType().GetProperty("isSelected").GetValue(arg);
        //Debug.Log("isSelected:"+isSelected);
        RectTransform rt = this.transform as RectTransform;

        if  (lastExpanedItem !=  null  && lastExpanedItem != this.gameObject)
        {
            ScrollViewExtension.ExpandCompleteCallback onComplete = ()=> {
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

            this.gameObject.SendMessageUpwards("OnExpandItem", new { item = lastExpanedItem, to = new Vector2(500, 235), onComplete= onComplete });
            lastExpanedItem = null;
        } else
        {
            if (isSelected)
            {
                lastExpanedItem = this.gameObject;
                this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
            }
            else
            {
                this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
            }
        }

        
    }

    //public object GetTypeByAttri(string attri)
    //{
    //    return this.data.GetType().GetProperty(attri).GetValue(this.data);
    //}

    public object GetCacheItemData()
    {
        return this.data;
    }
}
