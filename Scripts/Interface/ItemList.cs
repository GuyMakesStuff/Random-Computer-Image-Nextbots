using Experiments.Global.Audio;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ItemList : MonoBehaviour
{
    public string ListLabel;
    public TMP_Text ListLabelText;
    public List<string> Items;
    public RectTransform AddButton;
    public GameObject ItemInstance;
    public float ItemGaps;
    public bool AutoAdd;
    [System.Serializable]
    public class Item
    {
        public TMP_InputField inputField;
        public Button button;
        GameObject OBJ;
        RectTransform rectTransform;
        ItemList list;
        float Gaps;

        public void Construct(GameObject Instance, RectTransform Container, float gaps, int Index, ItemList itemList)
        {
            list = itemList;
            Gaps = gaps;

            OBJ = Instantiate(Instance, Vector3.zero, Quaternion.identity, Container);
            OBJ.SetActive(true);
            rectTransform = OBJ.GetComponent<RectTransform>();
            UpdateItemPos(Index);

            inputField = OBJ.GetComponentInChildren<TMP_InputField>();
            button = OBJ.GetComponentInChildren<Button>();
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { RemoveItem(false); }));
        }

        public void UpdateItemPos(int Index)
        {
            rectTransform.anchoredPosition = Vector2.down * (Gaps * (Index + 1));
        }

        public void RemoveItem(bool ExclusiveDestroy)
        {
            if(!ExclusiveDestroy) { list.DeleteItem(this); }
            Destroy(OBJ);
        }
    }
    List<Item> ItemFields;
    RectTransform rectTransform;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ItemFields = new List<Item>();

        if(AutoAdd)
        {
            for (int I = 0; I < Items.Count; I++)
            {
                AddItem(Items[I], true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ListLabelText.text = ListLabel + ":";
        AddButton.anchoredPosition = Vector2.down * (ItemGaps * (Items.Count + 1));

        for (int I = 0; I < Items.Count; I++)
        {
            if(I >= Items.Count) { break; }

            ItemFields[I].UpdateItemPos(I);
            Items[I] = ItemFields[I].inputField.text;
        }

        float RectHeight = ItemGaps * (Items.Count + 2);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, RectHeight);
        rectTransform.anchoredPosition = Vector2.down * (RectHeight / 2f);
    }

    public void AddItem(string Value, bool Init)
    {
        if(!Init) { Items.Add(Value); }

        Item NewItem = new Item();
        NewItem.Construct(ItemInstance, rectTransform, ItemGaps, ItemFields.Count, this);
        NewItem.inputField.text = Value;
        ItemFields.Add(NewItem);

        Update();
    }
    public void AddNew()
    {
        AddItem("", false);
        AudioManager.Instance.PlaySelectSound();
    }
    public void DeleteItem(Item item)
    {
        Items.Remove(item.inputField.text);
        ItemFields.Remove(item);
        AudioManager.Instance.PlaySelectSound();
    }

    public void SetItems(List<string> NewItems)
    {
        if(ItemFields == null) { ItemFields = new List<Item>(); }

        if(ItemFields.Count > 0)
        {
            foreach (Item I in ItemFields)
            {
                I.RemoveItem(true);
            }
            ItemFields.Clear();
        }
        Items.Clear();

        for (int I = 0; I < NewItems.Count; I++)
        {
            AddItem(NewItems[I], false);
        }
    }
}
