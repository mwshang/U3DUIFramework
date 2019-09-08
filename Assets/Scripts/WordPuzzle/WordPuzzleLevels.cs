using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPuzzleLevels : FirstPanel
{

    public GameObject TileItemPrefab;
    public GameObject TileItemPrefab1;

    private ScrollViewExtension sve;

    // Start is called before the first frame update
    void Start()
    {
        GameObject scroll = this.transform.Find("Scroll View").gameObject;

        sve = scroll.GetComponent<ScrollViewExtension>();
        sve.createItem = this.CreateItem;
        sve.initItem = this.InitItem;
        sve.gap = new Vector2(10, 10);
        sve.useCache = true;
        //sve.minItemSize = new Vector2(600, 100);
        sve.scrollType = ScrollViewExtension.ScrollType.Vertical;
        sve.cacheKey = "key";

        List<object> list = new List<object>();
        for (int i = 0; i < 30; i++)
        {
            list.Add(new { id = i, key = i % 2 == 0 ? 0 : 1 });
        }
        sve.DataProvider = list;
    }

    private GameObject CreateItem(object arg)
    {
        GameObject obj = null;
        object key = arg.GetType().GetProperty("key").GetValue(arg);
        int index = int.Parse(key.ToString());

        if (index == 0)
        {
            obj = Instantiate(TileItemPrefab);
        }
        else
        {
            obj = Instantiate(TileItemPrefab1);
        }
        Debug.Log("Create Item:*******************" + arg);

        return obj;
    }
    private void InitItem(GameObject item, object data)
    {
        //item.transform.Find("Text").GetComponent<Text>().text = data.ToString();

        object val = data.GetType().GetProperty("key").GetValue(data);
        int key = int.Parse(val.ToString());

        if (key==1)
        {
            LevelItem itemScript = item.GetComponent<LevelItem>();
            if (itemScript)
            {
                itemScript.SetData(data);
            }
        } else
        {
            TitleItem itemScript = item.GetComponent<TitleItem>();
            if (itemScript)
            {
                itemScript.SetData(data);
            }
        }

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
