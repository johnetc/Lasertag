﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuInfoSender : MonoBehaviour {

	// Use this for initialization

    public List<Button> ButtonList;
    public List<Text> TextList;
    public List<Slider> SliderList;
    public List<Toggle> ToggleList;
    public List<HorizontalOrVerticalLayoutGroup> LayoutGroupList;
    //public List<HorizontalLayoutGroup> HorzLayoutGroup;
    public List<LayoutElement> LayoutElements; 
    public List<MenuInfoSender> MenuInfoGroup;

	public void GetPrefabComponents () 
	{
		Debug.Log("Getting component data from... " + this.gameObject.name);

	    ButtonList = new List<Button>();
        foreach (var button in gameObject.GetComponentsInChildren<Button>())
	    {
	        ButtonList.Add(button);
	    }

        TextList = new List<Text>();
	    foreach (var componentsInChild in gameObject.GetComponentsInChildren<Text>())
	    {
	        TextList.Add(componentsInChild);
	    }

        SliderList = new List<Slider>();
	    foreach (var componentsInChild in gameObject.GetComponentsInChildren<Slider>())
	    {
	        SliderList.Add(componentsInChild);
	    }

        ToggleList = new List<Toggle> ();
        foreach ( var componentsInChild in gameObject.GetComponentsInChildren<Toggle> () )
        {
            ToggleList.Add ( componentsInChild );
        }

        LayoutGroupList = new List<HorizontalOrVerticalLayoutGroup> ();
        foreach ( var componentsInChild in gameObject.GetComponentsInChildren<VerticalLayoutGroup> () )
        {
            LayoutGroupList.Add ( componentsInChild );
        }

        //HorzLayoutGroup = new List<HorizontalLayoutGroup> ();
        foreach ( var componentsInChild in gameObject.GetComponentsInChildren<HorizontalLayoutGroup> () )
        {
            LayoutGroupList.Add ( componentsInChild );
        }

        LayoutElements = new List<LayoutElement> ();
        foreach ( var componentsInChild in gameObject.GetComponentsInChildren<LayoutElement> () )
        {
            LayoutElements.Add ( componentsInChild );
        }

        MenuInfoGroup = new List<MenuInfoSender>();
        foreach ( var componentsInChild in gameObject.GetComponentsInChildren<MenuInfoSender> () )
        {
            MenuInfoGroup.Add ( componentsInChild );
        }

	}
	
	
    public void CheckIfWorking()
    {
        Debug.Log("Working");
    }
}
