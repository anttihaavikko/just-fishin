using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DepthSetter : MonoBehaviour
{
    private SortingGroup _sortingGroup;
    
    private void Start()
    {
        _sortingGroup = GetComponent<SortingGroup>();
    }
    
    private void Update()
    {
        if (_sortingGroup)
        {
            _sortingGroup.sortingOrder = -Mathf.RoundToInt(transform.position.y);
        }
    }
}
