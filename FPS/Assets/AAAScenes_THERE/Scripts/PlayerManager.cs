using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //交流中心
    public ChangeItem _changeItem;
    public PhotoMode _photoMode;
    public FirstPersonController _firstPersonController;

    private void Update()
    {
        checkCanChangeItem();
    }

   void checkCanChangeItem()
    {
        if (_photoMode.galleryPanel.activeSelf || _photoMode.photoModePanel.activeSelf || _photoMode.photoSavePanel.activeSelf)
        {
            _changeItem.IsCanChangeItem = false;
        }
        else
        {
            _changeItem.IsCanChangeItem = true;
        }
    }




}
