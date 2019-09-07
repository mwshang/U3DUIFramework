using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : FirstPanel
{

    public GameObject TileItemPrefab;
    public GameObject TileItemPrefab1;

    private ScrollViewExtension sve;

    private void Start()
    {

        GameObject scroll = this.transform.Find("Scroll View").gameObject;

        sve = scroll.GetComponent<ScrollViewExtension>();
        sve.createItem = this.CreateItem;
        sve.initItem = this.InitItem;
        sve.gap = new Vector2(10, 10);
        sve.minItemSize = new Vector2(600, 100);
        sve.scrollType = ScrollViewExtension.ScrollType.Vertical;

        List<object> list = new List<object>();
        for (int i=0;  i<10; i++)
        {
            list.Add(i);
        }
        sve.DataProvider = list;
    }

    private GameObject CreateItem(object arg)
    {
        GameObject obj = null;

        int index = int.Parse(arg.ToString());

        if (false && index % 2 == 0)
        {
            obj = Instantiate(TileItemPrefab);
        } else
        {
            obj = Instantiate(TileItemPrefab1);
        }

        return obj;
    }
    private void InitItem(GameObject item,object data)
    {
        item.transform.Find("Text").GetComponent<Text>().text = data.ToString();
    }

    public void Test()
    {
        UIManager.instance.OpenPanel(UIPanelType.System);
    }
}
