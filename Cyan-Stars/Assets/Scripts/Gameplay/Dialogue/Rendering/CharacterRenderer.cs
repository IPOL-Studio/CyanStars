using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    [ExecuteAlways]
    public partial class CharacterRenderer : MonoBehaviour
    {
        public enum PositionRelativeEdge
        {
            Left,
            Right
        }

        public enum PositionMode
        {
            Absolute,
            Percentage
        }

        private RectTransform trans;
        private RectTransform imageTrans;
        private Image image;

        [SerializeField]
        private Vector2 position;
        public Vector2 Position
        {
            get => position;
        }

        [SerializeField]
        private PositionRelativeEdge relativeEdge;

        [SerializeField]
        private float rotation;
        public float Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                imageTrans.rotation = Quaternion.Euler(0, 0, value);
            }
        }

        [SerializeField]
        private Vector2 scale = Vector2.one;
        public Vector2 Scale
        {
            get => scale;
            set
            {
                scale = value;
            }
        }

        private Vector2 canvasSize;

        private void Start()
        {
            trans = transform as RectTransform;
            imageTrans = trans.GetChild(0) as RectTransform;
            image = imageTrans.GetComponent<Image>();
            canvasSize = GetComponentInParent<Canvas>().renderingDisplaySize;

            InitTransformProperties();
            InitImageProperties();

#if UNITY_EDITOR
            ApplyDrivenRectTransformTracker();

            if (Application.isPlaying)
            {
                Vector2 pos = CalculatePosition(relativeEdge, position, PositionMode.Absolute);
                trans.anchoredPosition = pos;
            }
#endif
        }

        private void InitTransformProperties()
        {
            trans.sizeDelta = Vector2.zero;
            trans.rotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

        private void InitImageProperties()
        {
            imageTrans.sizeDelta = image.sprite != null ? image.sprite.rect.size : Vector2.zero;
            imageTrans.pivot = new Vector2(0.5f, 0.5f);
            imageTrans.anchorMin = new Vector2(0.5f, 0.5f);
            imageTrans.anchorMax = new Vector2(0.5f, 0.5f);
            SetImagePosition();
        }

        private void SetImagePosition()
        {
            imageTrans.localScale = scale;
            var halfSize = imageTrans.sizeDelta * scale / 2;
            imageTrans.anchoredPosition = new Vector2(halfSize.x, -halfSize.y);
        }

        public void SetSprite(Sprite sprite)
        {
            SetSprite(sprite, image.sprite != null ? image.sprite.rect.size : Vector2.zero);
        }

        public void SetSprite(Sprite sprite, Vector2 size)
        {
            image.sprite = sprite;
            imageTrans.sizeDelta = size;
        }

        public void SetSprite(Sprite sprite, Vector2 size, Vector2 scale)
        {
            SetSprite(sprite, size);
            Scale = scale;
        }

        private Vector2 CalculatePosition(PositionRelativeEdge edge, Vector2 positionOffset)
        {
            Vector2 result = Vector2.zero;

            result.x = edge == PositionRelativeEdge.Left
                ? positionOffset.x
                : canvasSize.x - positionOffset.x - (imageTrans.sizeDelta * scale).x;

            result.y = -positionOffset.y;

            return result;
        }

        public Vector2 CalculatePosition(PositionRelativeEdge edge, Vector2 positionOffset, PositionMode mode)
        {
            var offset = mode == PositionMode.Absolute ? positionOffset : positionOffset * canvasSize;
            return CalculatePosition(edge, offset);
        }
    }

#if UNITY_EDITOR
    public partial class CharacterRenderer
    {
        private DrivenRectTransformTracker tracker;

        private void ApplyDrivenRectTransformTracker()
        {
            tracker.Clear();
            tracker.Add(this, trans, ~(DrivenTransformProperties.Pivot | DrivenTransformProperties.Anchors));
            tracker.Add(imageTrans.gameObject, imageTrans, DrivenTransformProperties.All);
        }

        private void Update()
        {
            if (Application.isPlaying)
                return;

            SetImagePosition();

            Vector2 pos = CalculatePosition(relativeEdge, position, PositionMode.Absolute);
            trans.anchoredPosition = pos;
            imageTrans.rotation = Quaternion.Euler(0, 0, rotation);
        }

        private void OnEnable()
        {
            if (trans == null)
                return;

            ApplyDrivenRectTransformTracker();
        }

        private void OnDisable()
        {
            tracker.Clear();
        }
    }
#endif
}
