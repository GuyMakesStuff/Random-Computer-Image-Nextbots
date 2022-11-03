using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ImageGallery : MonoBehaviour
{
    public GameObject ImageSlotInstance;
    public RectTransform SlotContainer;
    public Sprite DefaultSlotImage;
    public float ImageSlotGaps;
    const float ImageSlotSize = 180f;
    public class ImageSlot
    {
        Texture2D TargetImage;
        Sprite ImageSprite;
        Sprite DefaultImage;
        GameObject OBJ;

        Image Preview;
        TMP_Text ImageNameText;
        TMP_Text ImageDiscoverCount;

        public ImageSlot(GameObject obj, Vector2 Pos, Texture2D img, Sprite Default)
        {
            TargetImage = img;
            DefaultImage = Default;
            ImageSprite = Sprite.Create(TargetImage, new Rect(0f, 0f, TargetImage.width, TargetImage.height), Vector2.one / 2f);
            ImageSprite.name = TargetImage.name;

            OBJ = obj;
            OBJ.GetComponent<RectTransform>().anchoredPosition = Pos;

            Preview = OBJ.transform.Find("PreviewImage").GetComponent<Image>();
            ImageNameText = OBJ.transform.Find("ImageNameText").GetComponent<TextMeshProUGUI>();
            ImageDiscoverCount = OBJ.transform.Find("DiscoverCountText").GetComponent<TextMeshProUGUI>();
        }

        public void UpdateSlot()
        {
            NextbotGameManager.Progress.DiscoverableImage IMG = NextbotGameManager.Instance.progress.ImagesDiscovered.Find(DiscoverableImage => DiscoverableImage.ImageName == TargetImage.name);
            bool Discovered = IMG != null;
            int DiscoveryCount = 0;
            if(Discovered) { DiscoveryCount = IMG.DiscoverCount; }

            Preview.sprite = (Discovered) ? ImageSprite : DefaultImage;
            ImageNameText.text = (Discovered) ? TargetImage.name : "?????";
            ImageDiscoverCount.enabled = DiscoveryCount > 1;
            if(Discovered) { ImageDiscoverCount.text = "x" + DiscoveryCount.ToString(); }
        }

        public void RemoveSlot()
        {
            Destroy(OBJ);
        }
    }
    List<ImageSlot> Slots;

    // Start is called before the first frame update
    void Start()
    {
        NextbotGameManager.InitFinished += Reconstruct;
    }
    void Reconstruct()
    {
        if(Slots != null)
        {
            if(Slots.Count > 0)
            {
                for (int S = 1; S < SlotContainer.childCount; S++)
                {
                    Destroy(SlotContainer.GetChild(S).gameObject);
                }
                Slots.Clear();
            }
        }

        Slots = new List<ImageSlot>();

        float TotalImageSlotSize = ImageSlotSize + ImageSlotGaps;
        int ImageSlotPerLine = (int)(965f / TotalImageSlotSize);
        int Collum = -1;
        int Row = 0;
        for (int I = 0; I < NextbotSpawner.Images.Count; I++)
        {
            Collum++;
            if(Collum >= ImageSlotPerLine)
            {
                Collum = 0;
                Row++;
            }

            float CollumPos = (TotalImageSlotSize / 2f) + (TotalImageSlotSize * Collum);
            float RowPos = -((TotalImageSlotSize / 2f) + (TotalImageSlotSize * Row));
            GameObject Slot = Instantiate(ImageSlotInstance, Vector3.zero, Quaternion.identity, SlotContainer);
            Slot.name = "Image Slot_" + (I + 1).ToString();
            Slot.SetActive(true);
            Slots.Add(new ImageSlot(Slot, new Vector2(CollumPos, RowPos), NextbotSpawner.Images[I], DefaultSlotImage));
        }

        float ScrollHeight = TotalImageSlotSize * (Row + 1);
        SlotContainer.sizeDelta = new Vector2(0f, ScrollHeight);
        SlotContainer.anchoredPosition = Vector2.down * (ScrollHeight / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ImageSlot IS in Slots)
        {
            IS.UpdateSlot();
        }
    }
}
