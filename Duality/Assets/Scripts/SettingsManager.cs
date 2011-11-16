using UnityEngine;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour {
	public struct Settings {
		public QualityLevel quality;
		public int width;
		public int height;
		public bool fullScreen;
	}
	public Settings settings;
	Settings lastSettings;
	List<Resolution> supportedResolutions;
	int resolutionIndex;
	
	bool inited = false;
	
	public void Init () {
		if (!inited) {
			inited = true;
			
			/* "Screen.resolutions 
			static var resolutions : Resolution[]
			Description
			All fullscreen resolutions supported by the monitor (Read Only).
	
			The returned resolutions are sorted by width, lower resolutions come first." */
			supportedResolutions = new List<Resolution>();
			supportedResolutions.Add(Screen.resolutions[0]);
			for (int i = 1; i < Screen.resolutions.Length; i++)
				if (supportedResolutions[supportedResolutions.Count-1].width != Screen.resolutions[i].width
						|| supportedResolutions[supportedResolutions.Count-1].height != Screen.resolutions[i].height)
					supportedResolutions.Add(Screen.resolutions[i]);
			
			resolutionIndex = 0; /* unimportant /* always gets overwritten even in editor because that way the operating system resolution is used which thus is
				in this list too - the only exception could be if a resolution wouldn't get included if it's aspect ratio is hidden in the project settings*/
			
			settings = new Settings();
			for (int i = 0; i < supportedResolutions.Count; i++)
				if (Screen.width == supportedResolutions[i].width && Screen.height == supportedResolutions[i].height) {
					resolutionIndex = i;
					break;
				}
	
			settings.quality = QualitySettings.currentLevel;
			settings.width = Screen.width;
			settings.height = Screen.height;
			settings.fullScreen = Screen.fullScreen;
			
			lastSettings = settings;
		}
	}
	
	public void ChangeResolution (int change) {
		/* rollover ===*/
		resolutionIndex += change;
		
		if (resolutionIndex < 0)
			resolutionIndex = supportedResolutions.Count - 1;
		else if (resolutionIndex > supportedResolutions.Count - 1)
			resolutionIndex = 0;
		/* === rollover*/
	
		/* non-rollover ===
		resolutionIndex = Mathf.Clamp(resolutionIndex + change, 0, supportedResolutions.Count - 1);
		=== non-rollover*/
		
		settings.width = supportedResolutions[resolutionIndex].width;
		settings.height = supportedResolutions[resolutionIndex].height;
	}
	
	public void ChangeQuality (int change) {
		// non-hardcoded way of getting last enum index?
		
		/* rollover ===*/
		int q = (int)settings.quality + change;
		
		if (q < 0)
			q = 5;
		else if (q > 5)
			q = 0;
			
		settings.quality = (QualityLevel)q;
		/* === rollover*/
	
		/* non-rollover ===
		// the quality level cast to a number returns it's enum index try to change it within the the enum's range
		settings.quality = (QualityLevel)Mathf.Clamp((int)settings.quality + change, 0, 5);
		=== non-rollover*/
	}
	
	public void ResetSettings () {
		settings.quality = QualitySettings.currentLevel;
		settings.width = Screen.width;
		settings.height = Screen.height;
		settings.fullScreen = Screen.fullScreen;
	}
	
	public void ApplySettings () {
		if (QualitySettings.currentLevel != settings.quality) {
			lastSettings.quality = QualitySettings.currentLevel;
		
			QualitySettings.currentLevel = settings.quality;
		}
		
		if (Screen.width != settings.width || Screen.height != settings.height || Screen.fullScreen != settings.fullScreen) {
			lastSettings.width = Screen.width;
			lastSettings.height = Screen.height;
			lastSettings.fullScreen = Screen.fullScreen;
		
			Screen.SetResolution(settings.width, settings.height, settings.fullScreen);
		}
	}
}
