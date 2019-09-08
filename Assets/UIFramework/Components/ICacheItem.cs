using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICacheItem 
{
    /// <summary>
    /// 通过这个接口进行缓存的键名
    /// </summary>
    /// <param name="attri">通过ScrollViewExtension  cacheKey属性获取缓存键名</param>
    /// <returns></returns>
    //object GetTypeByAttri(string attri);
    object GetCacheItemData();
}
