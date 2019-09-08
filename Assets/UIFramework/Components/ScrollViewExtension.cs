using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;


/// <summary>
/// 针对ScrollView进行优化,只创建视口能展现的几条数据,将该脚本挂在ScrollView组件上
/// 支持不同尺寸item,支持item扩大缩小功能,参见 OnExpandItem接口
/// 注意item的锚点和pivot需要重合(0.5,0.5)
/// 必要参数:
/// createItem:创建item回调函数
/// initItem:初始化item回调函数
/// minItemSize:item尺寸
/// DataProvider:List类型的数据源
/// 
/// 非必要参数:
/// gap:item间距,默认Vector2(0, 2)
/// scrollType:滚动方向,默认垂直ScrollType.Vertical
/// useCache:是否使用缓存,默认使用,注意item需要实现ICacheItem接口,否则会 出现 缓存失败
/// cacheKey:数据源的属性名,为了支持不同大小的 item需要 分类 存储
/// 
/// 使用缓存功能注意事项:
/// ScrollViewExtension  sve;
/// sve.useCache = true;
/// sve.cacheKey = "xxx";
/// 
/// 
/// e.g.
/// 测试Item Anchors与Pivot都为(0.5,0.5)
/// sve = scroll.GetComponent<ScrollViewExtension>();
/// sve.createItem = this.CreateItem;
/// sve.initItem = this.InitItem;
/// sve.gap = new Vector2(0, 10);
/// sve.minItemSize = new Vector2(600, 100);
/// sve.scrollType = ScrollViewExtension.ScrollType.Vertical;
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
/// expand demo:
/// 多条item扩展
/// if (isSelected)
/// {
///     lastExpanedItem = this.gameObject;
///     this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
/// }
/// else
/// {
///     this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
/// }
/// 或则天条item扩展:
/// if  (lastExpanedItem !=  null  && lastExpanedItem != this.gameObject)
/// {
///     ScrollViewExtension.ExpandCompleteCallback onComplete = ()=> {
///         if (isSelected)
///         {
///             lastExpanedItem = this.gameObject;
///             this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
///         }
///         else
///         {
///             //rt.DOSizeDelta(new Vector2(500, 235), 0.3f);
///             this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
///         }
///     };
///     this.gameObject.SendMessageUpwards("OnExpandItem", new { item = lastExpanedItem, to = new Vector2(500, 235), onComplete= onComplete });
///     lastExpanedItem = null;
/// } else
/// {
///     if (isSelected)
///     {
///         lastExpanedItem = this.gameObject;
///         this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
///     }
///     else
///     {
///         this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
///     }
/// }
/// 
/// 
/// </summary>
public class ScrollViewExtension : MonoBehaviour
{
    // public    
    public CreateItem createItem;
    public InitItem initItem;
    public Vector2 gap = new Vector2(2, 2);
    public Vector2 minItemSize = new Vector2(600, 100);
    // 是否使用缓存,默认使用,注意为了支持不同大小的 item,需要实现ICacheItem接口
    public bool useCache = true;
    public string cacheKey = null;//数据源的属性名,为了支持不同大小的 item需要 分类 存储

    public ScrollType scrollType = ScrollType.Vertical;

    // protected
    protected ScrollRect scrollRect = null;
    protected RectTransform rtViewport = null;
    protected RectTransform rtContent = null;

    protected List<GameObject> _itemList = new List<GameObject>();
    protected Dictionary<object, Stack<GameObject>> _cacheItemStack;


    protected float _totalHeight = 0;
    protected Vector2 _lastChangeValue = Vector2.zero;
    protected int _displayCount = 1;

    // private
    private List<object> _dataProvider;
    private Dictionary<int, bool> _dicCreated = new Dictionary<int, bool>();
    private bool isInited = false;

    ///////////////////////
    // Expand logic
    private bool isExpanding = false;

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
    public delegate void ExpandCompleteCallback();


    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        rtViewport = scrollRect.gameObject.GetComponent<RectTransform>();
        rtContent = scrollRect.content;

        if (useCache)
        {
            _cacheItemStack = new Dictionary<object, Stack<GameObject>>();
        }

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
            //解决滑动条位置偏移问题,这块还有问题,用到滑动条的不多,有用到在解决
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rtViewport.sizeDelta.y);
        }
        else
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

    public List<GameObject> GetItemList()
    {
        return this._itemList;
    }


    protected void Clear()
    {
        foreach (GameObject obj in _itemList)
        {
            this.DestroyItem(obj, -1);
        }
        _itemList.Clear();
        this._dicCreated.Clear();
    }

    protected  void DestroyItem(GameObject item,int index)
    {
        if (index >= 0)
        {
            if (this._itemList.Count <=  this._displayCount)
            {//数量不够一屏显示则不删除
                return;
            }

            _itemList.RemoveAt(index);
        }
        
        if (this.useCache)
        {
            this.PushItem2Cache(item);
        }
        else
        {
            Destroy(item);
        }
    }

    protected object GetCacheKey(object data)
    {
        if (data == null)
        {
            Debug.LogWarning("ScrollViewExtension::GetCacheKey  data is null,cache item maybe failed!!!!!!");
        }
        object rst = null;
        if (data != null && !string.IsNullOrEmpty(cacheKey))
        {
            System.Type type = data.GetType();
            rst = type.GetProperty(cacheKey).GetValue(data);
        }
        if (rst == null)
        {
            rst = "default";
        }
        return rst;
    }

    protected Stack<GameObject> GetCacheStack(GameObject obj)
    {
        ICacheItem cacheItem = obj.GetComponent<ICacheItem>();
        if (cacheItem == null)
        {
            Debug.LogWarning("ScrollViewExtension::GetCacheStack  ICacheItem is null,cache item maybe failed!!!!!!");
        }
        object value = this.GetCacheKey(cacheItem != null ? cacheItem.GetCacheItemData() : null);
        Stack<GameObject> stack = _cacheItemStack.TryGet(value);
        if (stack == null)
        {
            stack = new Stack<GameObject>();
            _cacheItemStack[value] = stack;
        }
        return stack;
    }


    protected void PushItem2Cache(GameObject obj)
    {
        Stack<GameObject> stack = this.GetCacheStack(obj);
        stack.Push(obj);
        obj.SetActive(false);
    }
    protected GameObject PopItemFromCache(object key)
    {
        if (key == null)
        {
            key = "default";
        }
        Stack<GameObject> stack = _cacheItemStack.TryGet(key);
        if (stack == null)
        {
            return null;
        }

        GameObject obj = stack.Pop();
        obj.SetActive(true);
        return obj;
    }

    protected bool hasCachItem(object key)
    {
        if (key == null)
        {
            key = "default";
        }
        Stack<GameObject> stack = _cacheItemStack.TryGet(key);
        if (stack == null)
        {
            return false;
        }
        return stack.Count > 0;
    }


    protected void GenerateList()
    {
        this.Clear();
        bool isVertical = this.scrollType == ScrollType.Vertical;
        if (this._dataProvider != null && this._dataProvider.Count > 0)
        {
            this.InitContentSize();

            if (isVertical)
            {
                _displayCount = (int)(rtViewport.sizeDelta.y / (minItemSize.y + gap.y)) + 1;

                int count = Mathf.Clamp(this._dataProvider.Count, 0, _displayCount);

                for (int i = 0; i < count; i++)
                {
                    object data = this._dataProvider[i];
                    this.CreateVerticalItem(data, i, CreateItemDir.SCROLL_DOWN);
                }

                ////////////////////////
                if (this._itemList.Count > 0)
                {//修复item 大小不一致可能导致 初始化为 创建满屏BUG
                    int cd = 0;
                    while (cd < this._displayCount)
                    {
                        GameObject lastItem = this._itemList[this._itemList.Count - 1];
                        RectTransform rt = lastItem.transform as RectTransform;
                        float oy = rt.pivot.y * rt.sizeDelta.y;

                        Vector2 pos = rt.parent.parent.InverseTransformPoint(rt.position);
                        pos.y -= oy;

                        if (pos.y <= this.gap.y)
                        {
                            break;
                        }
                        cd += 1;
                        int index = int.Parse(lastItem.name) + 1;
                        if (index >= this._dataProvider.Count)
                        {
                            break;
                        }
                        this.CreateVerticalItem(this._dataProvider[index], index, CreateItemDir.SCROLL_DOWN);
                    }
                }
                //===================

                scrollRect.verticalScrollbar.value = 1;
            }
            else
            {
                _displayCount = (int)(rtViewport.sizeDelta.x / (minItemSize.x + gap.x)) + 1;
                int count = Mathf.Clamp(this._dataProvider.Count, 0, _displayCount);
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
            contentHeight = Mathf.Clamp(contentHeight, this.rtViewport.sizeDelta.y, contentHeight);
            this.rtContent.sizeDelta = new Vector2(this.rtViewport.sizeDelta.x, contentHeight);
            this.rtContent.anchoredPosition = new Vector2(0, this.rtContent.anchoredPosition.y);
        }
        else
        {
            float contentWidth = this._dataProvider.Count * minItemSize.x + (this._dataProvider.Count - 1) * this.gap.x;
            contentWidth = Mathf.Clamp(contentWidth, this.rtViewport.sizeDelta.x, contentWidth);
            this.rtContent.sizeDelta = new Vector2(contentWidth, this.rtViewport.sizeDelta.y);
            this.rtContent.anchoredPosition = new Vector2(this.rtContent.anchoredPosition.x, 0);

        }
    }

    protected GameObject CreateListItem(object data, int index)
    {
        GameObject obj = null;
        object stackKey = this.GetCacheKey(data);
        if (useCache && this.hasCachItem(stackKey))
        {
            obj = this.PopItemFromCache(stackKey);
        }
        else
        {

            obj = this.createItem(data);
        }

        obj.name = index.ToString();
        obj.transform.SetParent(this.rtContent.transform, false);
        this.initItem(obj, data);
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
        }
        else
        {
            this.HandleHorizontalValueChanged(pos.x - _lastChangeValue.x < 0);
        }
        _lastChangeValue = pos;


    }

    //////////////////////////////////////////////////////////////////////////////////////
    ///  竖向滚动逻辑
    //////////////////////////////////////////////////////////////////////////////////////

    protected GameObject CreateVerticalItem(object data, int index, CreateItemDir dir)
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
            size.y = Mathf.Clamp(size.y, this.rtViewport.sizeDelta.y, size.y);
            this.rtContent.sizeDelta = size;
        }

        if (dir == CreateItemDir.SCROLL_DOWN)
        {
            float py = -rectTrans.sizeDelta.y * (1 - pivot.y);

            if (_itemList.Count > 0)
            {
                GameObject lastItem = _itemList[_itemList.Count - 1];
                RectTransform lastRT = lastItem.GetComponent<RectTransform>();
                py += lastRT.anchoredPosition.y - lastRT.sizeDelta.y * lastRT.pivot.y - this.gap.y;
            }
            rectTrans.anchoredPosition = new Vector3(0, py, rectTrans.position.z);

            _itemList.Add(obj);
        }
        else
        {
            GameObject fstItem = _itemList[0];
            RectTransform fstRT = fstItem.GetComponent<RectTransform>();
            float py = fstRT.anchoredPosition.y + fstRT.sizeDelta.y * (1 - fstRT.pivot.y) + this.gap.y;
            py += rectTrans.sizeDelta.y * pivot.y;
            rectTrans.anchoredPosition = new Vector3(0, py, rectTrans.position.z);
            _itemList.Insert(0, obj);
        }

        return obj;
    }

    protected void HandleVerticalValueChanged(bool isScrollUp)
    {

        if (isScrollUp)
        {// scroll up
            // 处理第一元素,如果向下越过顶部,则创建
            GameObject item = _itemList[0];
            RectTransform rt = item.GetComponent<RectTransform>();
            Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);

            if (p.y < -((1 - rt.pivot.y) * rt.sizeDelta.y))
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
                DestroyItem(item,_itemList.Count - 1);
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
                DestroyItem(item, 0);
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

    //////////////////////////////////////////////////////////////////////////////////////
    /// 横向滚动逻辑
    //////////////////////////////////////////////////////////////////////////////////////
    protected void HandleHorizontalValueChanged(bool isScrollRight)
    {
        if (_itemList.Count == 0)
        {
            return;
        }
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
            if (p.x > this.rtViewport.sizeDelta.x + rt.pivot.x * rt.sizeDelta.x)
            {
                DestroyItem(item, _itemList.Count - 1);
            }

        }
        else
        {

            // 处理第一元素,如果超出了就删除
            GameObject item = _itemList[0];
            RectTransform rt = item.GetComponent<RectTransform>();
            Vector3 p = rt.parent.parent.InverseTransformPoint(rt.position);

            if (p.x < rt.pivot.x * (1 - rt.sizeDelta.x))
            {
                DestroyItem(item, 0);
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
            size.x = Mathf.Clamp(size.x, this.rtViewport.sizeDelta.x, size.x);
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

    //////////////////////////////////////////////////////////////////////////////////////
    /// Item扩展逻辑
    //////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 伸缩ITEM
    /// e.g.
    /// if (isSelected)
    ///{   //扩大
    ///   this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 335) });
    ///}
    ///else
    ///{   //缩小
    ///    this.gameObject.SendMessageUpwards("OnExpandItem", new { item = this.gameObject, to = new Vector2(500, 235) });
    ///}
    /// </summary>
    /// <param name="arg">{GameObject item,Vector2 to;float duration;默认0.3f,注意是float类型的}</param>
    public void OnExpandItem(object arg)
    {
        if (isExpanding || this._itemList.Count == 0)
        {
            return;
        }
        isExpanding = true;

        //  获取参数
        System.Type type = arg.GetType();
        GameObject item = (GameObject)(type.GetProperty("item").GetValue(arg));
        Vector2 to = (Vector2)(type.GetProperty("to").GetValue(arg));

        float duration = 0.3f;
        if (type.GetProperty("duration") != null)
        {
            duration = (float)(type.GetProperty("duration").GetValue(arg));
        }
        ExpandCompleteCallback onComplete = null;
        if (type.GetProperty("onComplete") != null)
        {
            onComplete = (ExpandCompleteCallback)(type.GetProperty("onComplete").GetValue(arg));
        }
        //==


        duration = Mathf.Clamp(duration, 0.3f, 100);


        RectTransform itemRT = item.GetComponent<RectTransform>();
        Vector2 deltaSize = to - itemRT.sizeDelta;

        Vector2 p = itemRT.parent.parent.InverseTransformPoint(itemRT.position);
        // 注意,item anchors为(0.5,1)计算的
        float itemMidY = p.y - itemRT.sizeDelta.y * 0.5f;
        float viewMidY = -this.rtViewport.sizeDelta.y * 0.5f;
        float s = itemMidY - viewMidY;

        float maxMoveH = this.rtContent.sizeDelta.y - this.rtViewport.sizeDelta.y - this.rtContent.anchoredPosition.y;

        //
        if (maxMoveH < 0) maxMoveH = 0;
        s = Mathf.Min(s, maxMoveH);

        //  上部分 的位置保持不变,设置 sizeDelta时位置 会变化
        Vector2 pos = this.rtContent.anchoredPosition;
        pos.y = pos.y - deltaSize.y * 0.5f;
        float contentHeight = this.rtContent.sizeDelta.y + deltaSize.y;
        contentHeight = Mathf.Clamp(contentHeight, this.rtViewport.sizeDelta.y, contentHeight);
        this.rtContent.sizeDelta = new Vector2(this.rtContent.sizeDelta.x, contentHeight);
        this.rtContent.anchoredPosition = pos;
        //===

        if (s < -this.rtViewport.sizeDelta.y * 0.15)
        {//向上移动到中间位置

            float itemIndex = int.Parse(item.name);
            bool notLastItem = (itemIndex <= this._dataProvider.Count - _displayCount);
            if (!notLastItem)
            {
                s = -deltaSize.y;
            }
            this.rtContent.DOAnchorPosY(this.rtContent.anchoredPosition.y - s, duration).OnComplete(() =>
            {
                this._doExpanding(item, to, deltaSize, duration, onComplete);
            });
        }
        else
        {
            this._doExpanding(item, to, deltaSize, duration, onComplete);
        }

    }


    protected void _doExpanding(GameObject item, Vector2 to, Vector2 deltaSize, float duration, ExpandCompleteCallback onComplete = null)
    {
        RectTransform itemRT = item.GetComponent<RectTransform>();
        List<GameObject> afters = this.getGroupByItem(item);

        // expand item
        itemRT.DOSizeDelta(to, duration).OnComplete(() =>
        {
            isExpanding = false;
            if (onComplete != null)
            {
                onComplete();
            }
        });

        // 将后面的items向下移动
        if (afters.Count > 0)
        {
            foreach (GameObject go in afters)
            {
                RectTransform goRT = go.transform as RectTransform;
                goRT.DOAnchorPosY(goRT.anchoredPosition.y - deltaSize.y, duration);
            }
        }
    }

    protected List<GameObject> getGroupByItem(GameObject item)
    {
        List<GameObject> afters = new List<GameObject>();

        bool found = false;

        foreach (GameObject obj in this._itemList)
        {
            if (obj == item)
            {
                found = true;
            }
            else if (found)
            {
                afters.Add(obj);
            }

        }

        return afters;
    }
}
