using Common;
using Models;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniMap : MonoBehaviour
{
    [SerializeField]
    private Collider MiniMapBoundingBox;

    [SerializeField]
    private Image MiniMapImage;

    [SerializeField]
    private Image Arrow;

    [SerializeField]
    private Text MapName;

    private Transform PlayerTransform;
    private float realWidth;
    private float realHeight;
    private float pivotX;
    private float pivotY;
    private bool initialized;

    void Start()
    {
        this.initialized = false;
        User.Instance.CurrentCharacterObjectChanged += OnCurrentCharacterObjectChanged;
        TryInitMiniMap();
    }

    private void OnDestroy()
    {
        User.Instance.CurrentCharacterObjectChanged -= OnCurrentCharacterObjectChanged;
    }

    private void OnCurrentCharacterObjectChanged(GameObject go)
    {
        TryInitMiniMap();
    }

    private void TryInitMiniMap()
    {
        if (this.initialized) return;

        if (this.MiniMapBoundingBox == null || this.MiniMapImage == null || this.Arrow == null || this.MapName == null)
        {
            Log.Error("UIMiniMap: MiniMapBoundingBox/MiniMapImage/Arrow/MapName is not assigned!");
            this.enabled = false;
            return;
        }

        if (User.Instance.CurrentCharacterObject == null)
            return;

        this.MapName.text = User.Instance.CurrentMapData.Name;
        if (this.MiniMapImage.overrideSprite == null)
            this.MiniMapImage.overrideSprite = MiniMapManager.Instance.LoadSprite();

        this.MiniMapImage.SetNativeSize();
        this.MiniMapImage.transform.localPosition = Vector3.zero;

        this.PlayerTransform = User.Instance.CurrentCharacterObject.transform;
        this.realWidth = this.MiniMapBoundingBox.bounds.size.x;
        this.realHeight = this.MiniMapBoundingBox.bounds.size.z;
        this.initialized = true;
    }

    void Update()
    {
        if (!this.initialized) return;
        if (this.PlayerTransform == null || this.MiniMapBoundingBox == null) return;

        this.pivotX = (this.PlayerTransform.position.x - this.MiniMapBoundingBox.bounds.min.x) / this.realWidth;
        this.pivotY = (this.PlayerTransform.position.z - this.MiniMapBoundingBox.bounds.min.z) / this.realHeight;
        this.MiniMapImage.rectTransform.pivot = new Vector2(this.pivotX, this.pivotY);
        this.MiniMapImage.rectTransform.localPosition = Vector3.zero;
        this.Arrow.rectTransform.eulerAngles = new Vector3(0, 0, -this.PlayerTransform.eulerAngles.y);
    }
}
