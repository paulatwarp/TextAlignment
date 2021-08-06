using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class BaseGadget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    static bool m_Log = false;

    [SerializeField]
    public string m_OnClickSound = "UIOptionSelected", m_OnEnterSound = "UIOptionIndicated";

    [SerializeField]
    public string m_OnEnterRolloverID = "";
    string m_OnEnterRollover = "";

    [SerializeField]
    public bool m_ClickReact = true;

    [System.Serializable]
    public class ClickEvent : UnityEvent<BaseGadget> { }

    [SerializeField]
    public ClickEvent m_OnClickEvent;

    [HideInInspector]
    public Action<BaseGadget> m_OnEnterEvent;
    [HideInInspector]
    public Action<BaseGadget> m_OnExitEvent;
    [HideInInspector]
    public Action<BaseGadget> m_OnDownEvent;
    [HideInInspector]
    public Action<BaseGadget> m_OnUpEvent;
    [HideInInspector]
    public Action<BaseGadget> m_OnDragStartEvent;
    [HideInInspector]
    public Action<BaseGadget> m_OnDragEndEvent;

    protected bool m_Interactable = true;
    protected bool m_Selected = false;
    protected bool m_Indicated = false;
    protected bool m_Drag = false;

    Action<BaseGadget> m_Action;
    BaseGadget m_ActionGadget;

    [HideInInspector]
    public object m_ExtraData;              // arbitrary extra information that can be added to this gadget

    protected void Awake()
    {
        if (m_OnEnterRolloverID != "")
            SetRolloverFromID(m_OnEnterRolloverID);
    }

    protected void Start()
    {
        SetIndicated(false);
    }

    protected void OnDestroy()
    {
        // if we're indicated make sure we un-indicate
        if (m_Indicated)
            OnPointerExit(null);
    }

    void OnDisable()
    {
        // if we're indicated make sure we un-indicate
        if (m_Indicated)
            OnPointerExit(null);
    }

    protected void DoAction()
    {
        if (m_Action != null && !m_Action.Target.Equals(null))
            m_Action(m_ActionGadget);
    }

    bool IsMouseInsideScreen()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x >= Screen.width || Input.mousePosition.y >= Screen.height)
            return false;

        return true;
    }

    virtual public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Enter " + this);
        if (!m_Interactable)
            return;

        if (!IsMouseInsideScreen())
            return;

        SetIndicated(true);
    }

    virtual public void OnPointerExit(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Exit " + this);
        if (!m_Interactable)
            return;

        SetIndicated(false);
    }

    virtual public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Down " + this);
        if (!m_Interactable)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        m_Drag = false;

        if (m_OnDownEvent != null)
            m_OnDownEvent(this);
    }

    virtual public void OnPointerUp(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Up " + this);
        if (!m_Interactable)
            return;

        if (m_Drag)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (m_OnUpEvent != null)
            m_OnUpEvent(this);
    }

    protected bool CanReactToClick(PointerEventData eventData)
    {
        if (!m_Interactable)
            return false;

        // At the moment only LMB. This is so we don't have problems clicking RMB while dragging with LMB
        if (eventData.button != PointerEventData.InputButton.Left)
            return false;

        if (!m_ClickReact)
            return false;

        // make sure we've not already started a drag
        if (m_Drag && m_OnDragStartEvent != null)
            return false;

        return true;
    }

    virtual public void OnPointerClick(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Click " + this);

        if (!CanReactToClick(eventData))
            return;

        DoAction();
    }

    virtual public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_Log)
            Debug.Log("Drag " + this);
        if (!m_Interactable)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Drag = true;
        if (m_OnDragStartEvent != null)
            m_OnDragStartEvent(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_Interactable)
            return;

        if (!m_Drag)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!m_Interactable)
            return;

        if (!m_Drag)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Drag = false;
        if (m_OnDragEndEvent != null)
            m_OnDragEndEvent(this);
    }

    public void ForceEndDrag()
    {
        // use this when a drag operation needs to be interrupted (when ESC is pressed for instance)

        if (!m_Interactable)
            return;

        if (!m_Drag)
            return;

        m_Drag = false;
    }

    public void UpdateRollover()
    {
        // force the rollover to update if we're indicated
    }

    public void SetRollover(string NewText)
    {
        m_OnEnterRollover = NewText;

        UpdateRollover();
    }

    public void SetRolloverFromID(string ID)
    {
        UpdateRollover();
    }

    virtual public void SetAction(Action<BaseGadget> NewAction, BaseGadget NewGadget)
    {
        m_Action = NewAction;
        m_ActionGadget = NewGadget;
    }

    public Vector2 GetPosition()
    {
        return GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetPosition(Vector2 Position)
    {
        GetComponent<RectTransform>().anchoredPosition = Position;
    }

    public void SetPosition(float x, float y)
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public float GetWidth()
    {
        return GetComponent<RectTransform>().sizeDelta.x;
    }

    public float GetHeight()
    {
        return GetComponent<RectTransform>().sizeDelta.y;
    }

    public void SetSize(Vector2 Position)
    {
        GetComponent<RectTransform>().sizeDelta = Position;
    }

    public void SetSize(float Width, float Height)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
    }

    public void SetWidth(float Width)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Width, GetHeight());
    }

    public void SetHeight(float Height)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetWidth(), Height);
    }

    void CheckInteractable()
    {
        // make sure we go out of indicated mode if we become non-interactable
        if (m_Indicated)
            OnPointerExit(null);
    }

    virtual public void SetIndicated(bool Indicated)
    {
        m_Indicated = Indicated;
    }

    virtual public void SetInteractable(bool Interactable)
    {
        if (!Interactable)
            CheckInteractable();

        m_Interactable = Interactable;
    }

    public bool GetInteractable()
    {
        return m_Interactable;
    }

    virtual public void SetSelected(bool Selected)
    {
        m_Selected = Selected;
    }

    public bool GetIsSelected()
    {
        return m_Selected;
    }

    virtual public void SetActive(bool Active)
    {
        if (!Active)
            CheckInteractable();

        if (gameObject.activeSelf != Active)
            gameObject.SetActive(Active);
    }

    public bool GetActive()
    {
        return gameObject.activeSelf;
    }
}
