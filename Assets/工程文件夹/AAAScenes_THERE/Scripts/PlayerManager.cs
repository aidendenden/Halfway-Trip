using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //��������
    public ChangeItem _changeItem;
    public PhotoMode _photoMode;
    

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
