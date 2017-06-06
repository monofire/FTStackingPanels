
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIPanelsStack : MonoBehaviour
{

	public static UIPanelsStack Instance = null;


	/* CONST */
	/* default tag to mark and recognise "UIPanel" UI element during panels parsing */
	const string _UIPanelTag = "UIPanel";

	/* name of the first panel presented to player */
	const string _initiallyPresentedPanelName = "MainMenuPanel";

	/* used to recognise always-active-panel during  activateUIPanels() deactivateUIPanels() calls */
	const string _alwaysActivePanelName = "GameScenePanel";


	/* ATTRIBUTES */
	[HideInInspector]
	/* collection of panels managed by this PanelsStack */
	public Dictionary <string, UIPanel> panels = null;

	[HideInInspector]
	public UIPanel previousPanel = null;

	[HideInInspector]
	public UIPanel currentPanel = null;

	[HideInInspector]
	public bool transitionIsFinished = true;


	/* IVARS */
	[HideInInspector]
	UIPanel _sender = null;

	[HideInInspector]
	UIPanel _target = null;

	[HideInInspector]
	/* pointer on Canvas event system */
	GameObject _eventSystem = null;

	/* HANDLERS */
	[HideInInspector]
	Action _onTransitionToTargetFinishedNullable = null;

	[HideInInspector]
	Action _onHideTargetTransitionFinishedNullable = null;



	/* INIT */
	void Awake ()
	{
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy (gameObject);
		}
	}


	void Start ()
	{
		initialize ();
	}


	public void initialize ()
	{
		_eventSystem = GameObject.Find ("EventSystem");
		populatePanels ();
		setInitialState ();
	}




	#region PRESENT PANEL

	/* presents panel without transition to other panel */
	public void presentTarget (string targetName, 
	                           Action onTransitionToTargetFinishedNullable)
	{
		/* prepare */
		resetTargetAndSenderPointers ();

		/* find target */
		UIPanel target = panelWithName (targetName);

		/* set target */
		if (target != null)
			_target = target;
	
		setPreviousAndCurrentPanels (null, target);

		setTransitionCompletionHandler (onTransitionToTargetFinishedNullable);

		/* STEP 0 - lock UI */
		lockUI ();

		/* start from STEP 4 - prepare to leave sender */
		onSenderDidDisappearFinished ();
	}

	/* NOTE: STEPS are 4-6 executed identical to transition sequence */

	#endregion


	#region HIDE PANEL

	public void hideTarget (string targetName, 
	                        Action onHideTargetTransitionFinishedNullable)
	{
		/* find target */
		UIPanel target = panelWithName (targetName);

		/* set target  */
		if (target != null)
			_target = target;


		setPreviousAndCurrentPanels (null, null);
	
		setHideCompletionHandler (onHideTargetTransitionFinishedNullable);
	
		/* STEP 0 - lock UI */
		lockUI ();

		/* STEP 1 - prepare to leave target */
		target.onWillDisappear (onTargetWillDisappearFinished);
	}


	/* STEP 2 - leave target */
	void onTargetWillDisappearFinished () /* target finished to process WillDisappear call */
	{ 
		_target.startDisappearAnimation (onTargetDisappearAnimationFinished);
	}


	/* STEP 3 */
	void onTargetDisappearAnimationFinished ()
	{
		_target.onDidDisappear (onTargetDidDisappearFinished);
	}


	/* STEP 4 */
	void onTargetDidDisappearFinished ()
	{
		unlockUI ();

		/* STEP 5 */
		if (_onHideTargetTransitionFinishedNullable != null) {
			_onHideTargetTransitionFinishedNullable ();
			_onHideTargetTransitionFinishedNullable = null;
		}
	}

	#endregion



	#region PRESENT PANEL WITH TRANSITION FROM OTHER PANEL

	/* hides sender panel and present target panel */

	public void present (string targetName,
	                     string senderName, 
	                     Action onTransitionToTargetFinishedNullable)
	{	

		/* find panels */
		UIPanel sender = panelWithName (senderName);
		UIPanel target = panelWithName (targetName);
	
		/* set panels  */
		if (sender != null)
			_sender = sender;

		if (target != null)
			_target = target;
	
		setPreviousAndCurrentPanels (sender, target);

		setTransitionCompletionHandler (onTransitionToTargetFinishedNullable);

		/* STEP 0 - block UI */
		lockUI ();

		/* STEP 1 - prepare to leave sender */
		sender.onWillDisappear (onSenderWillDisappearFinished);
	}


	/* STEP 2 - leave sender */
	void onSenderWillDisappearFinished () /* sender finished to process WillDisappear call */
	{ 
		_sender.startDisappearAnimation (onSenderDisappearAnimationFinished);
	}


	/* STEP 3 */
	void onSenderDisappearAnimationFinished ()
	{
		_sender.onDidDisappear (onSenderDidDisappearFinished);
	}


	/* STEP 4 - prepare to land on target */
	void onSenderDidDisappearFinished ()
	{
		_target.onWillAppear (_sender, onTargetWillAppearFinished);
	}


	/* STEP 5 - land on target  */
	void onTargetWillAppearFinished ()
	{
		_target.startAppearAnimation (onTargetAppearAnimationFinished);
	}


	/* STEP 5 */
	void onTargetAppearAnimationFinished ()
	{
		_target.onDidAppear (_sender, onTargetDidAppearFinished);
	}


	/* STEP 6 - target finally appeared - notify sender */
	void onTargetDidAppearFinished ()
	{
		/* STEP 7 - unlock UI */
		unlockUI ();

		if (_onTransitionToTargetFinishedNullable != null) {
			_onTransitionToTargetFinishedNullable ();
			_onTransitionToTargetFinishedNullable = null;
		}
	}

	#endregion




	#region TOOLS

	/* PANELS */
	void populatePanels ()
	{
		panels = getChildrenUIPanels (); /* parse child panels and add to collection */
	}


	void setInitialState ()
	{
		/* set previous panel to default */
		UIPanel initiallyPresentedPanel = null;
		panels.TryGetValue (_initiallyPresentedPanelName, out initiallyPresentedPanel);
		previousPanel = initiallyPresentedPanel;
		currentPanel = initiallyPresentedPanel;
	}


	Dictionary <string, UIPanel> getChildrenUIPanels ()
	{
		Dictionary <string, UIPanel> dict = new Dictionary <string, UIPanel> ();
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).gameObject.tag.Equals (_UIPanelTag)) { /* process only panels with "UIPanel" tag*/
				GameObject obj = transform.GetChild (i).gameObject;

				UIPanel panel = obj.GetComponentInChildren <UIPanel> ();
				dict.Add (panel.name, panel);
			}
		}
		return dict;
	}



	/* UI LOCK */
	/* disallow any touch UI events  */
	public void lockUI ()
	{
		transitionIsFinished = false;
		_eventSystem.SetActive (false);
	}

	public void unlockUI ()
	{
		transitionIsFinished = true;
		_eventSystem.SetActive (true);
	}


	/* PANELS ACTIVATION */
	/* to prevent excessive UI drawcalls on the not visible panels */
	public void deactivateUIPanels ()
	{
		setAllPanelsActive (false);
		GC.Collect ();
	}

	public void activateUIPanels ()
	{
		setAllPanelsActive (true);
	}

	void setAllPanelsActive (bool isActive)
	{
		for (int i = 0; i < panels.Keys.Count; i++) {

			string key = panels.Keys.ElementAt (i);

			/* exception (always active) panel  */
			if (!key.Equals (_alwaysActivePanelName)) {

				UIPanel obj = null;
				panels.TryGetValue (key, out obj);

				if (obj != null) {
					obj.gameObject.SetActive (isActive);
				}
			}
		}
	}



	/* PANELS PRESENTATION FLOW */
	void resetTargetAndSenderPointers ()
	{
		_target = null;
		_sender = null;
	}


	UIPanel panelWithName (string panelName)
	{
		/* find panel */
		UIPanel panel = null;
		panels.TryGetValue (panelName, out panel);

		if (panel == null)
			Debug.Log ("::: UIPanelsStack ERROR! - can't find panel: " + panelName);

		return panel;
	}


	void setPreviousAndCurrentPanels (UIPanel previousPanel, UIPanel currentPanel)
	{
		this.previousPanel = previousPanel;
		this.currentPanel = currentPanel;
	}

	void setTransitionCompletionHandler (Action onTransitionToTargetFinishedNullable)
	{
		_onTransitionToTargetFinishedNullable = null;
		if (onTransitionToTargetFinishedNullable != null)
			_onTransitionToTargetFinishedNullable = onTransitionToTargetFinishedNullable;	
	}


	void setHideCompletionHandler (Action onHideTargetTransitionFinishedNullable)
	{
		_onHideTargetTransitionFinishedNullable = null;
		if (onHideTargetTransitionFinishedNullable != null)
			_onHideTargetTransitionFinishedNullable = onHideTargetTransitionFinishedNullable;
	}

	#endregion
}
