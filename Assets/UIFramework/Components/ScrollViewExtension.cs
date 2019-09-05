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

    public Vector2 gap = new Vector2(2, 2);
    public Vector2 minItemSize = new Vector2(600,100);

    public ScrollType scrollType = ScrollType.Vertical;

    // protected
    protected ScrollRect scrollRect = null;
    protected RectTransform rtViewport = null;
    protected RectTransform rtContent = null;    

    protected List<GameObject> _itemList = new List<GameObject>();
    //protected List<GameObject> _cacheItemList = new List<GameObject>();

    protected float _totalHeight = 0;
    protected Vector2 _lastChangeValue = Vector2.zero;
    
    // private
    private List<object> _dataProvider;
    private Dictionary<int,bool> _dicCreated = new Dictionary<int, bool>();
    private bool isInited = false;

    public enum ScrollType
    {
        Horizontal,
        Vertical
    }

    public enum CreateItemDir
    {
        SCROLL_DOWN,//从下滑动
        SCROLL_UP, // 从上滑动
        SCROLL_RIGHT,
        SCROLL_LEFT
    }

    public delegate GameObject CreateItem(object arg);
    public delegate void InitItem(GameObject item, object data);


    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        rtViewport = scrollRect.gameObject.GetComponent<RectTransform>();     
        rtContent = scrollRect.content;

        
    }


    // Start is called before the first frame update
    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnValueChange);

        if (scrollType == ScrollType.Vertical)
        {
            rtContent.pivot = new Vector2(0.5f, 0.5f);
            rtContent.anchorMin = new Vector2(0.5f, 1f);
            rtContent.anchorMax = new Vector2(0.5f, 1f);

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // 修复滑块太小BUG
            RectTransform rt = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            Vector2 sizeDelta = scrollRect.verticalScrollbar.GetComponent<RectTransform>().sizeDelta;
            sizeDelta.y = this.rtViewport.sizeDelta.y;
            rt.sizeDelta = sizeDelta;
            //解决滑动条位置偏移问题
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,rtViewport.sizeDelta.y);
        } else
        {
            rtContent.pivot = new Vector2(0f, 0.5f);
            rtContent.anchorMin = new Vector2(0.5f, 0.5f);
            rtContent.anchorMax = new Vector2(0.5f, 0.5f);

            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // 修复滑块太小BUG
            Vector2 sizeDelta = scrollRect.horizontalScrollbar.GetComponent<RectTransform>().sizeDelta;
            sizeDelta.x = this.rtViewport.sizeDelta.x;
            scrollRect.horizontalScrollbar.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        }        
        this.GenerateList();
        this.isInited = true;
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
            if (isInited)
            {
                this.GenerateList();
            }
            
        }
    }


    protected void Clear()
    {
        foreach(GameObject obj in _itemList)
        {
            Destroy(obj);
        }
        _itemList.Clear();
        this._dicCreated.Clear();
    }

    protected void GenerateList()
    {
        this.Clear();
        bool isVertical = this.scrollType == ScrollType.Vertical;
        if (this._dataProvider != null && this._dataProvider.Count > 0)
        {
            this.InitContentSize();

            int _maxCount = 0;

            if (isVertical)
            {
                _maxCount = (int)(rtViewport.sizeDelta.y / (minItemSize.y + gap.y)) + 1;

                int count = Mathf.Clamp(this._dataProvider.Count, 0, _maxCount);

                for (int i = 0; i < count; i++)
                {
                    object data = this._dataProvider[i];
                    this.CreateVerticalItem(data, i, CreateItemDir.SCROLL_DOWN);
                }
                scrollRect.verticalScrollbar.value = 1;
            } else
            {
                _maxCount = (int)(rtViewport.sizeDelta.x / (minItemSize.x + gap.x)) + 1;
                int count = Mathf.Clamp(this._dataProvider.Count, 0, _maxCount);
                for (int i = 0; i < count; i++)
                {
                    object data = this._dataProvider[i];
                    this.CreateHorizontalItem(data, i, CreateItemDir.SCROLL_LEFT);
                }
                scrollRect.horizontalScrollbar.value = 0;
            }


        }
    }

    protected void InitContentSize()
    {
        if (this.scrollType == ScrollType.Vertical)
        {
            float contentHeight = this._dataProvider.Count * minItemSize.y + (this._dataProvider.Count - 1) * this.gap.y;
            this.rtContent.sizeDelta = new Vector2(this.rtViewport.sizeDelta.x, contentHeight);
            this.rtContent.anchoredPosition = new Vector2(0,this.rtContent.anchoredPosition.y);
        } else
        {
            float contentWidth = this._dataProvider.Count * minItemSize.x + (this._dataProvider.Count - 1) * this.gap.x;
            this.rtContent.sizeDelta = new Vector2(contentWidth, this.rtViewport.sizeDelta.y);
            this.rtContent.anchoredPosition = new Vector2(this.rtContent.anchoredPosition.x, 0);
            
            
        }
    }

    protected GameObject CreateListItem(object data, int index)
    {
        GameObject obj = this.createItem(data);
        obj.name = index.ToString();
        obj.transform.SetParent(this.rtContent.transform, false);
        this.initItem(obj, data);
        return obj;
    }

    protected GameObject CreateVerticalItem(object data,int index, CreateItemDir dir)
    {
        GameObject obj = this.CreateListItem(data, index);

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

        if (dir == CreateItemDir.SCROLL_DOWN)
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
    
    protected GameObject CreateHorizontalItem(object data, int index, CreateItemDir dir)
    {
        GameObject obj = this.CreateListItem(data, index);

        RectTransform rectTrans = obj.GetComponent<RectTransform>();
        rectTrans.anchorMin = new Vector2(0f, 0.5f);
        rectTrans.anchorMax = new Vector2(0f, 0.5f);
        Vector2 pivot = rectTrans.pivot;

        // adjust content height
        if (rectTrans.sizeDelta.x != this.minItemSize.x && !this._dicCreated.TryGet(index))
        {
            this._dicCreated[index] = true;
            float deltaW = rectTrans.sizeDelta.x - this.minItemSize.x;
            Vector2 size = this.rtContent.sizeDelta;
            size.x += deltaW;
            this.rtContent.sizeDelta = size;
        }
        if (dir == CreateItemDir.SCROLL_RIGHT)
        {
            float px = -rectTrans.sizeDelta.x * pivot.x;

            GameObject lastItem = _itemList[0];
            RectTransform lastRT = lastItem.GetComponent<RectTransform>();
            px += lastRT.anchoredPosition.x - lastRT.sizeDelta.x * lastRT.pivot.x;
            px -= this.gap.x;
            rectTrans.anchoredPosition = new Vector3(px, 0, rectTrans.position.z);

            
            _itemList.Insert(0, obj);
        }
        else
        {
            float px = rectTrans.sizeDelta.x * pivot.x;
            if (_itemList.Count > 0)
            {
                GameObject fstItem = _itemList[0];
                RectTransform fstRT = fstItem.GetComponent<RectTransform>();
                px += fstRT.sizeDelta.x * fstRT.pivot.x;
                px += fstRT.anchoredPosition.x + this.gap.x;
            }
            rectTrans.anchoredPosition = new Vector3(px, 0, rectTrans.position.z);
            _itemList.Add(obj);
        }
        return obj;
    }
        
    protected void OnValueChange(Vector2 pos)
    {
        if (this._dataProvider == null || this._dataProvider.Count == 0 || !this.isInited)
        {
            return;
        }
        
       if (scrollType == ScrollType.Vertical)
        {
            this.HandleVerticalValueChanged(pos.y - _lastChangeValue.y > 0);            
        } else
        {
            this.HandleHorizontalValueChanged(pos.x - _lastChangeValue.x < 0);
        }
        _lastChangeValue = pos;

        
    }

    protected void HandleVerticalValueChanged(bool isScrollUp)
    {
        if (isScrollUp)
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
                    this.CreateVerticalItem(this._dataProvider[index], index, CreateItemDir.SCROLL_UP);
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
                    this.CreateVerticalItem(this._dataProvider[index], index, CreateItemDir.SCROLL_DOWN);
                }
            }
        }
    }

    protected void HandleHorizontalValueChanged(bool isScrollRight)
    {
        if (isScrollRight)
        {
            // 处理第一元素,如果越过左边,则创建
            GameObject item = _itemList[0];
            RectTransform rt = item.GetComponent<RectTransform>();
            Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);
            
            if (p.x - rt.pivot.x * rt.sizeDelta.x > 0)
            {
                int index = int.Parse(item.name) - 1;
                if (index >= 0)
                {
                    this.CreateHorizontalItem(this._dataProvider[index], index, CreateItemDir.SCROLL_RIGHT);
                }
            }

            //处理最后一个元素,向上越过底部则删除
            item = _itemList[_itemList.Count - 1];
            rt = item.GetComponent<RectTransform>();
            p = rt.parent.parent.InverseTransformPoint(rt.position);
            if (p.y > this.rtViewport.sizeDelta.x + rt.pivot.x * rt.sizeDelta.x)
            {
                _itemList.RemoveAt(_itemList.Count - 1);
                Destroy(item);
            }

        } else
        {
            
            // 处理第一元素,如果超出了就删除
            GameObject item = _itemList[0];
            RectTransform rt = item.GetComponent<RectTransform>();
            Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);
            
            if (p.x < rt.pivot.x * (1 - rt.sizeDelta.x))
            {
                _itemList.RemoveAt(0);
                Destroy(item);
            }

            //处理最后一个元素,向上越过底部则创建
            item = _itemList[_itemList.Count - 1];
            rt = item.GetComponent<RectTransform>();
            p = rt.parent.parent.InverseTransformPoint(rt.position);
            float tx = p.x + (1 - rt.pivot.x) * rt.sizeDelta.x + this.gap.x;
            if (tx < this.rtViewport.sizeDelta.x)
            {
                int index = int.Parse(item.name) + 1;
                if (index < this._dataProvider.Count)
                {
                    this.CreateHorizontalItem(this._dataProvider[index], index, CreateItemDir.SCROLL_LEFT);
                }
            }
        }
    }
}
