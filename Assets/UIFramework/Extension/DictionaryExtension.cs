﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryExtension
{
    public static TValue TryGet<TKey,TValue>(this Dictionary<TKey,TValue> dict,TKey key)
    {
        TValue val;
        dict.TryGetValue(key, out val);
        return val;
    }
}
