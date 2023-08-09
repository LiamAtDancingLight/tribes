using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour, IInteractable
{

    [SerializeField] private GameObject _selectionMesh;
    
    private Animator _animator;
    

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnClicked()
    {
        PlayClickedAnimation();
        print("I am clicked! + " + gameObject.name);
        SetSelected(true);
    }

    public void OnDeselected()
    {
        SetSelected(false);
    }

    private void SetSelected(bool newSelected)
    {
        _selectionMesh.SetActive(newSelected);
    }


    private void PlayClickedAnimation()
    {
        _animator.Play("OnClicked", 0, 0);
    }
}
