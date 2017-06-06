using UnityEngine;
using System.Collections;
using System;

public abstract class UIPanel : MonoBehaviour
{
	
	/* IVARS */
	bool _isPresented = false;


	/* HANDLERS */
	Action _onDisappearAnimationFinishedHandler = null;
	Action _onAppearAnimationFinishedHandler = null;


	/* EVENTS INTERFACE */
	/* to be implemented in subclasses */
	public abstract void onWillDisappear (Action onWillDisappearFinished);

	public abstract void onDidDisappear (Action onDidDisappearFinished);

	public abstract void onWillAppear (UIPanel sender, Action onWillAppearFinished);

	public abstract void onDidAppear (UIPanel sender, Action onDidAppearFinished);


	/* ANIMATIONS CALLBACKS INTERFACE */
	/* to be implemented in subclasses */
	public abstract void startAppearAnimation ();

	public abstract void startDisappearAnimation ();




	/* ANIMATIONS OPERATIONS */
	public virtual void startDisappearAnimation (Action onDisappearAnimationFinishedHandler)
	{
		/* subscribe */
		_onDisappearAnimationFinishedHandler = null;
		if (onDisappearAnimationFinishedHandler != null)
			_onDisappearAnimationFinishedHandler = onDisappearAnimationFinishedHandler;

		startDisappearAnimation ();
	}


	public virtual void onDisappearAnimationFinished ()
	{
		/* call back */
		if (_onDisappearAnimationFinishedHandler != null)
			_onDisappearAnimationFinishedHandler ();
		_onDisappearAnimationFinishedHandler = null;

		_isPresented = false;
	}


	public virtual void startAppearAnimation (Action onAppearAnimationFinishedHandler)
	{
		/* subscribe */
		_onAppearAnimationFinishedHandler = null;
		if (onAppearAnimationFinishedHandler != null)
			_onAppearAnimationFinishedHandler = onAppearAnimationFinishedHandler;

		startAppearAnimation ();
	}


	public virtual void onAppearAnimationFinished ()
	{
		/* call back */
		if (_onAppearAnimationFinishedHandler != null)
			_onAppearAnimationFinishedHandler ();
		_onAppearAnimationFinishedHandler = null;

		_isPresented = true;
	}


	public virtual bool isPresented ()
	{
		return _isPresented;
	}

}



