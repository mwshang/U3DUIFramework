using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UIPanelInfo:ISerializationCallbackReceiver
{
    [NonSerialized]
    public UIPanelType panelType;
    public string panelTypeString;
    //{
    //    get
    //    {
    //        return panelType.ToString();
    //    }
    //    set
    //    {
    //        panelType = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), value);
    //    }
    //}
    public string path;

    public void OnAfterDeserialize()
    {
        panelType = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), panelTypeString);
    }

    public void OnBeforeSerialize()
    {
        throw new NotImplementedException();
    }
}
