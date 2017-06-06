using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PanelA : UIPanel
{

	/* IVARS */
	Animator _animator = null;


	/* INIT */
	void Awake ()
	{
		/* get pointer on animator object */
		_animator = GetComponent<Animator> ();
	}


	/* Demo */
	public void onButtonClick ()
	{
		UIPanelsStack.Instance.present ("PanelB", this.name, null);
	}
		

	/* PANEL PRESENTATION EVENTS  - CALLED BY UIPanelsStack OBJ */
	override public void onWillDisappear (Action onWillDisappearFinished)
	{
		/* Use this callback to code your responce to event and prepare to the next panel stage
		e.g save UI state to model */
		Debug.Log ("::: PanelA onWillDisappear");
		onWillDisappearFinished (); /* handler should be called to continue with next presentation stage (usefull for async operations) */
	}


	override public void onDidDisappear (Action onDidDisappearFinished)
	{
		/* e.g reconfigure UI objects on panel, release resources, start of stop game process, etc */
		Debug.Log ("::: PanelA onDidDisappear");
		onDidDisappearFinished ();  /* handler should be called to continue with next presentation stage (usefull for async operations) */
	}


	override public void onWillAppear (UIPanel sender, Action onWillAppearFinished)
	{
		/* e.g prepare UI objects to be presented, recover state from model */
		string senderName = (sender != null) ? sender.name : "";
		Debug.Log ("::: PanelA onWillAppear, sender -> " + senderName);
		onWillAppearFinished ();  /* handler should be called to continue with next presentation stage (usefull for async operations) */
	}


	override public void onDidAppear (UIPanel sender, Action onDidAppearFinished)
	{
		string senderName = (sender != null) ? sender.name : "";
		Debug.Log ("::: PanelA onDidAppear, sender -> " + senderName);
		onDidAppearFinished ();  /* handler should be called to continue with next presentation stage (usefull for async operations) */
	}





	/* ANIMATIONS */
	override public void startAppearAnimation ()
	{
		/* to be connected to the corresponding event in animation settings */
		_animator.SetTrigger ("presentPanel");
	}

	override public void startDisappearAnimation ()
	{
		/* to be connected to the corresponding event in animation settings */
		_animator.SetTrigger ("hidePanel");
	}

}
