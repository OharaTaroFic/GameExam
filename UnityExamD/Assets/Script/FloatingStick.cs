using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

//画面のスティック（触った場所に出る）
//このオブジェクトの範囲（今は画面全体）のどこを触っても、そこにスティックが出る
//指を動かした向きが、ゲームパッドの左スティックとして入る（Player はそれを読んで動く。PC ではマウスでつかんで動かせる）
//触っていないときは隠しておく（「触っていないときの濃さ」を 0 より大きくすると、最初の場所にうすく出す）
public class FloatingStick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField] string m_ControlPath = "<Gamepad>/leftStick";
    protected override string controlPathInternal { get => m_ControlPath; set => m_ControlPath = value; }

    [Label("土台の円")] public RectTransform Base;
    [Label("つまみ")] public RectTransform Knob;
    [Label("つまみが動ける半径")] public float MovementRange = 110f;
    [Label("触っていないときの濃さ"), Range(0, 1)] public float IdleAlpha = 0f;

    RectTransform _area;
    CanvasGroup _group;
    Vector2 _homePos;   //触っていないときの場所
    Vector2 _origin;    //触り始めた場所
    int _pointerId = -1; //動かしている指（2 本目の指は無視する）

    void Awake()
    {
        _area = (RectTransform)transform;
        _group = Base.GetComponent<CanvasGroup>();
        _homePos = Base.anchoredPosition;
        SetIdle();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (_pointerId != -1) { return; }
        _pointerId = e.pointerId;
        _origin = ToLocal(e);
        Base.anchoredPosition = _origin;
        Knob.anchoredPosition = Vector2.zero;
        if (_group != null) { _group.alpha = 1f; }
        SendValueToControl(Vector2.zero);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId != _pointerId) { return; }
        var delta = Vector2.ClampMagnitude(ToLocal(e) - _origin, MovementRange);
        Knob.anchoredPosition = delta;
        SendValueToControl(delta / MovementRange);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != _pointerId) { return; }
        SetIdle();
    }

    protected override void OnDisable()
    {
        SetIdle();
        base.OnDisable();
    }

    void SetIdle()
    {
        _pointerId = -1;
        if (Base == null) { return; }
        Base.anchoredPosition = _homePos;
        Knob.anchoredPosition = Vector2.zero;
        if (_group != null) { _group.alpha = IdleAlpha; }
        if (Application.isPlaying) { SendValueToControl(Vector2.zero); }
    }

    //画面上の位置を、この範囲の中の位置（土台の円を置く座標）に直す
    Vector2 ToLocal(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, e.position, e.pressEventCamera, out var local);
        return local;
    }
}
