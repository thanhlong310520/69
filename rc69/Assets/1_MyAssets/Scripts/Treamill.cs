using Raccoon.Controller;
using Raccoon.Data;
using UnityEngine;

public class Treamill : GoContact
{
    public Transform holderPos;
    public Transform endPos;
    public Transform modelHolder;

    TreamillDataSO data;
    GameObject currentModel;

    public TreamillDataSO GetData => data;

    public void SetData(TreamillDataSO data)
    {
        this.data = data;
        if(modelHolder.childCount > 0) currentModel = modelHolder.GetChild(0).gameObject;
        if (currentModel != null) Destroy(currentModel);
        currentModel = null;

        if (data == null || data.prefabModel == null) return;

        Transform parent = modelHolder != null ? modelHolder : transform;
        currentModel = Instantiate(data.prefabModel, parent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
    }

    protected override void DoneWait()
    {
        user.GetComponent<ICharactor>().ContactTreamill(this);
    }

}
