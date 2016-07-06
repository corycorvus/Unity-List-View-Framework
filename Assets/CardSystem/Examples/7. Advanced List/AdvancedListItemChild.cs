using UnityEngine;
using System.Collections;
using ListView;

public class AdvancedListItemChild : AdvancedListItem {
    public TextMesh description;
    public Transform modelTransform;

    public GameObject model;

    public override void Setup(AdvancedListItemData data) {
        base.Setup(data);
        description.text = data.description;
        model = list.GetModel(data.model);
        model.transform.parent = modelTransform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;
        model.transform.localRotation = Quaternion.identity;
    }
}