
using UnityEngine;
public class LuaUIEventListener : MonoBehaviour
{
    public delegate void VoidDelegate(GameObject go, int parameter);
    public delegate void BoolDelegate(GameObject go, bool state, int parameter);
    public delegate void FloatDelegate(GameObject go, float delta, int parameter);
    public delegate void VectorDelegate(GameObject go, Vector2 delta, int parameter);
    public delegate void ObjectDelegate(GameObject go, GameObject obj, int parameter);
    public delegate void KeyCodeDelegate(GameObject go, KeyCode key, int parameter);
    public int data = 0;


    public VoidDelegate onSubmit;
    public VoidDelegate onClick;
    public VoidDelegate onDoubleClick;
    public BoolDelegate onHover;
    public BoolDelegate onPress;
    public BoolDelegate onSelect;
    public FloatDelegate onScroll;
    public VoidDelegate onDragStart;
    public VectorDelegate onDrag;
    public VoidDelegate onDragOver;
    public VoidDelegate onDragOut;
    public VoidDelegate onDragEnd;
    public ObjectDelegate onDrop;
    public KeyCodeDelegate onKey;
    public BoolDelegate onTooltip;

    bool isColliderEnabled
    {
        get
        {
            Collider c = GetComponent<Collider>();
            if (c != null) return c.enabled;
            Collider2D b = GetComponent<Collider2D>();
            return (b != null && b.enabled);
        }
    }

    void OnSubmit() { if (isColliderEnabled && onSubmit != null) onSubmit(gameObject, data); }
    void OnClick() { if (isColliderEnabled && onClick != null) onClick(gameObject, data); }
    void OnDoubleClick() { if (isColliderEnabled && onDoubleClick != null) onDoubleClick(gameObject, data); }
    void OnHover(bool isOver) { if (isColliderEnabled && onHover != null) onHover(gameObject, isOver, data); }
    void OnPress(bool isPressed) { if (isColliderEnabled && onPress != null) onPress(gameObject, isPressed, data); }
    void OnSelect(bool selected) { if (isColliderEnabled && onSelect != null) onSelect(gameObject, selected, data); }
    void OnScroll(float delta) { if (isColliderEnabled && onScroll != null) onScroll(gameObject, delta, data); }
    void OnDragStart() { if (onDragStart != null) onDragStart(gameObject, data); }
    void OnDrag(Vector2 delta) { if (onDrag != null) onDrag(gameObject, delta, data); }
    void OnDragOver() { if (isColliderEnabled && onDragOver != null) onDragOver(gameObject, data); }
    void OnDragOut() { if (isColliderEnabled && onDragOut != null) onDragOut(gameObject, data); }
    void OnDragEnd() { if (onDragEnd != null) onDragEnd(gameObject, data); }
    void OnDrop(GameObject go) { if (isColliderEnabled && onDrop != null) onDrop(gameObject, go, data); }
    void OnKey(KeyCode key) { if (isColliderEnabled && onKey != null) onKey(gameObject, key, data); }

    /// <summary>
    /// Get or add an event listener to the specified game object.
    /// </summary>

    static public LuaUIEventListener Get(GameObject go)
    {
        LuaUIEventListener listener = go.GetComponent<LuaUIEventListener>();
        if (listener == null) listener = go.AddComponent<LuaUIEventListener>();
        return listener;
    }

    private void OnDestroy()
    {
        Clear();
        LuaUIEventListener listener = GetComponent<LuaUIEventListener>();
        if (listener != null) Destroy(listener);
    }

    public void Clear()
    {
        onSubmit = null;
        onClick = null;
        onDoubleClick = null;
        onHover = null;
        onPress = null;
        onSelect = null;
        onScroll = null;
        onDragStart = null;
        onDrag = null;
        onDragOver = null;
        onDragOut = null;
        onDragEnd = null;
        onDrop = null;
        onKey = null;
        onTooltip = null;
    }
}
