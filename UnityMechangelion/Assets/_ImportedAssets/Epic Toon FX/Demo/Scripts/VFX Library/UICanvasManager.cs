using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICanvasManager : MonoBehaviour {
	public static UICanvasManager GlobalAccess;
	void Awake () {
		GlobalAccess = this;
	}

	public bool MouseOverButton = false;
	public Text PENameText;
	public Text ToolTipText;

	// Use this for initialization
	void Start () {
		if (this.PENameText != null) this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}
	
	// Update is called once per frame
	void Update () {
	
		// Mouse Click - Check if mouse over button to prevent spawning particle effects while hovering or using UI buttons.
		if (!this.MouseOverButton) {
			// Left Button Click
			if (Input.GetMouseButtonUp (0)) {
				// Spawn Currently Selected Particle System
				this.SpawnCurrentParticleEffect();
			}
		}

		if (Input.GetKeyUp (KeyCode.A)) {
			this.SelectPreviousPE ();
		}
		if (Input.GetKeyUp (KeyCode.D)) {
			this.SelectNextPE ();
		}
	}

	public void UpdateToolTip(ButtonTypes toolTipType) {
		if (this.ToolTipText != null) {
			if (toolTipType == ButtonTypes.Previous) {
				this.ToolTipText.text = "Select Previous Particle Effect";
			}
			else if (toolTipType == ButtonTypes.Next) {
				this.ToolTipText.text = "Select Next Particle Effect";
			}
		}
	}
	public void ClearToolTip() {
		if (this.ToolTipText != null) {
			this.ToolTipText.text = "";
		}
	}

	private void SelectPreviousPE() {
		// Previous
		ParticleEffectsLibrary.GlobalAccess.PreviousParticleEffect();
		if (this.PENameText != null) this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}
	private void SelectNextPE() {
		// Next
		ParticleEffectsLibrary.GlobalAccess.NextParticleEffect();
		if (this.PENameText != null) this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}

	private RaycastHit rayHit;
	private void SpawnCurrentParticleEffect() {
		// Spawn Particle Effect
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast (mouseRay, out this.rayHit)) {
			ParticleEffectsLibrary.GlobalAccess.SpawnParticleEffect (this.rayHit.point);
		}
	}

	/// <summary>
	/// User interfaces the button click.
	/// </summary>
	/// <param name="buttonTypeClicked">Button type clicked.</param>
	public void UIButtonClick(ButtonTypes buttonTypeClicked) {
		switch (buttonTypeClicked) {
		case ButtonTypes.Previous:
			// Select Previous Prefab
			this.SelectPreviousPE();
			break;
		case ButtonTypes.Next:
			// Select Next Prefab
			this.SelectNextPE();
			break;
		default:
			// Nothing
			break;
		}
	}
}
