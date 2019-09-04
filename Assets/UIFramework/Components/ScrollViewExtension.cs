using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



/// <summary>
/// 针对ScrollView进行优化,只创建视口能展现的几条数据,将该脚本挂在ScrollView组件上
/// 支持不同尺寸item
/// 注意item的锚点和pivot需要重合(0.5,0.5)
/// 必要参数:
/// createItem:创建item回调函数
/// initItem:初始化item回调函数
/// minItemSize:item尺寸
/// 
/// 非必要参数:
/// gap:item间距,默认Vector2(0, 2)
/// scrollType:滚动方向,默认垂直ScrollType.Vertical
/// DataProvider:List类型的数据源
/// 
/// e.g.
/// 测试Item Anchors与Pivot都为(0.5,0.5)
/// sve = scroll.GetComponent<ScrollViewExtension>();
/// sve.createItem = this.CreateItem;
/// sve.initItem = this.InitItem;
/// sve.gap = new Vector2(0, 10);
/// sve.minItemSize = new Vector2(600, 100);
/// sve.DataProvider = new List<object>();
/// 
/// private GameObject CreateItem(object arg)
/// {
///     GameObject obj = Instantiate(TileItemPrefab);
///     obj.transform.Find("Text").GetComponent<Text>().text = arg.ToString();
///     return obj;
/// }
/// private void InitItem(GameObject item, object data)
///{
/// }
/// 
/// </summary>
public class ScrollViewExtension : MonoBehaviour
{
    // public    
    public CreateItem createItem;
    public InitItem initItem;

    public Vector2 gap = new Vector2(0, 2);
    public Vector2 minItemSize = new Vector2(600,100);

    public ScrollType scrollType = ScrollType.Vertical;

    // protected
    protected ScrollRect scrollRect = null;
    protected RectTransform rtViewport = null;
    protected RectTransform rtContent = null;    

    protected List<GameObject> _itemList = new List<GameObject>();
    //protected List<GameObject> _cacheItemList = new List<GameObject>();

    protected float _totalHeight = 0;
    protected int _maxCount = 0;
    protected Vector2 _lastChangeValue = Vector2.zero;
    
    // private
    private List<object> _dataProvider;
    private Dictionary<int,bool> _dicCreated = new Dictionary<int, bool>();

    public enum ScrollType
    {
        Horizontal,
        Vertical
    }

    public enum CreateItemDir
    {
        UP_2_BOTTTOM,//从上往下创建
        BOTTOM_2_UP // 从下往上创建
    }

    public delegate GameObject CreateItem(object arg);
    public delegate void InitItem(GameObject item, object data);


    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        rtViewport = scrollRect.gameObject.GetComponent<RectTransform>();     
        rtContent = scrollRect.content;

        if (scrollType == ScrollType.Vertical)
        {            
            _maxCount = (int)(rtViewport.sizeDelta.y / (minItemSize.y + gap.y)) + 1;
            rtContent.pivot = new Vector2(0.5f, 1f);            
        } else
        {
            // TODO
            
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnValueChange);

        if (scrollType == ScrollType.Vertical)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        } else
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }


    }

    public List<object> DataProvider
    {
        get
        {
            return _dataProvider;
        }
        set
        {
            _dataProvider = value;            
            this.GenerateList();
        }
    }


    protected void Clear()
    {
        foreach(GameObject obj in _itemList)
        {
            Destroy(obj);
        }
        _itemList.Clear();
    }

    protected void GenerateList()
    {
        this.Clear();
        bool isVertical = this.scrollType == ScrollType.Vertical;
        if (this._dataProvider != null && this._dataProvider.Count > 0)
        {
            if (isVertical)
            {
                this.InitContentSize();

                int count = Mathf.Clamp(this._dataProvider.Count, 0, _maxCount);
                for (int i = 0; i < count; i++)
                {
                    object data = this._dataProvider[i];
                    this.CreateVerticalItem(data, i, CreateItemDir.UP_2_BOTTTOM);
                }
                scrollRect.verticalScrollbar.value = 1;
            }


        }
    }

    protected void InitContentSize()
    {
        if (this.scrollType == ScrollType.Vertical)
        {
            float contentHeight = this._dataProvider.Count * minItemSize.y + (this._dataProvider.Count - 1) * this.gap.y;
            this.rtContent.sizeDelta = new Vector2(this.rtViewport.sizeDelta.x, contentHeight);
        } else
        {
            // TODO
        }
    }

    protected GameObject CreateVerticalItem(object data,int index, CreateItemDir dir)
    {
        GameObject obj = this.createItem(data);
        obj.name = index.ToString();
        obj.transform.SetParent(this.rtContent.transform, false);        
        this.initItem(obj, data);

        RectTransform rectTrans = obj.GetComponent<RectTransform>();
        rectTrans.anchorMin = new Vector2(0.5f, 1f);
        rectTrans.anchorMax = new Vector2(0.5f, 1f);
        Vector2 pivot = rectTrans.pivot;

        // adjust content height
        if (rectTrans.sizeDelta.y != this.minItemSize.y && !this._dicCreated.TryGet(index))
        {
            this._dicCreated[index] = true;
            float deltaH = rectTrans.sizeDelta.y - this.minItemSize.y;
            Vector2 size = this.rtContent.sizeDelta;
            size.y += deltaH;
            this.rtContent.sizeDelta = size;
        }

        if (dir == CreateItemDir.UP_2_BOTTTOM)
        {
            float py = -rectTrans.sizeDelta.y * (1 - pivot.y);

            if (_itemList.Count > 0)
            {
                GameObject lastItem = _itemList[_itemList.Count - 1];
                RectTransform lastRT = lastItem.GetComponent<RectTransform>();
                py += lastRT.anchoredPosition.y - lastRT.sizeDelta.y * (1 - lastRT.pivot.y);
                py -= this.gap.y;
            }
            rectTrans.anchoredPosition = new Vector3(0, py, rectTrans.position.z);

            _itemList.Add(obj);
        } else
        {
            GameObject fstItem = _itemList[0];
            RectTransform fstRT = fstItem.GetComponent<RectTransform>();
            float py = fstRT.sizeDelta.y * fstRT.pivot.y;
            py += fstRT.anchoredPosition.y + this.gap.y + rectTrans.sizeDelta.y * pivot.y;
            rectTrans.anchoredPosition = new Vector3(0, py, rectTrans.position.z);
            _itemList.Insert(0, obj);
        }

        return obj;
    }
    

    protected void OnValueChange(Vector2 pos)
    {
        if (this._dataProvider == null || this._dataProvider.Count == 0)
        {
            return;
        }

       if (scrollType == ScrollType.Vertical)
        {
            if (pos.y - _lastChangeValue.y > 0)
            {// scroll up

                // 处理第一元素,如果向下越过顶部,则创建
                GameObject item = _itemList[0];
                RectTransform rt = item.GetComponent<RectTransform>();
                Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);

                if (p.y < -(rt.pivot.y * rt.sizeDelta.y + this.gap.y))
                {
                    int index = int.Parse(item.name) - 1;
                    if (index >= 0)
                    {
                        this.CreateVerticalItem(this._dataProvider[index], index, CreateItemDir.BOTTOM_2_UP);
                    }                    
                }

                //处理最后一个元素,向上越过底部则删除
                item = _itemList[_itemList.Count - 1];
                rt = item.GetComponent<RectTransform>();
                p = rt.parent.parent.InverseTransformPoint(rt.position);
                if (p.y < -this.rtViewport.sizeDelta.y - rt.pivot.y * rt.sizeDelta.y)
                {
                    _itemList.RemoveAt(_itemList.Count - 1);
                    Destroy(item);
                }

            }
            else
            {// scroll down

                // 处理第一元素,如果超出了就删除
                GameObject item = _itemList[0];
                RectTransform rt = item.GetComponent<RectTransform>();                
                Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);
                if (p.y > rt.pivot.y * rt.sizeDelta.y)
                {
                    _itemList.RemoveAt(0);
                    Destroy(item);
                }

                //处理最后一个元素,向上越过底部则创建
                item = _itemList[_itemList.Count - 1];
                rt = item.GetComponent<RectTransform>();
                p = rt.parent.parent.InverseTransformPoint(rt.position);
                float ty = p.y - rt.pivot.y * rt.sizeDelta.y - this.gap.y;
                if (ty > -this.rtViewport.sizeDelta.y)
                {
                    int index = int.Parse(item.name) + 1;
                    if (index < this._dataProvider.Count)
                    {
                        this.CreateVerticalItem(this._dataProvider[index], index, CreateItemDir.UP_2_BOTTTOM);
                    }                    
                }
            }
            
        } else
        {

        }
        _lastChangeValue = pos;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
