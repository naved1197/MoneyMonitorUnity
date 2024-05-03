	using UnityEngine;
	using System.Collections.Generic;

namespace CubeHole
{ 
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance = null;
		private AudioClip[] SoundFiles;
		private Dictionary<string, int> ClipIndex = new Dictionary<string, int>();
		public AudioSource soundFx;
		public AudioSource music;
		public static bool isMute;
		private void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(gameObject);

		}
		public void InitAudio(AudioClip[] SoundFiles)
		{
			for (int i = 0; i < SoundFiles.Length; i++)
			{
				ClipIndex.Add(SoundFiles[i].name, i);
			}
			this.SoundFiles = SoundFiles;
		}
		public void Play(string clip, float volume = 1, bool PitchChange = false)
		{
			if (isMute)
                return;
			if (!ClipIndex.ContainsKey(clip))
				return;
			if (PitchChange)
				soundFx.pitch = Random.Range(0.8f, 1.2f);
			soundFx.PlayOneShot(SoundFiles[ClipIndex[clip]], volume);
		}
		public void PlayRandomSounds(string[] Clips, float volume = 1, bool PitchChange = false)
		{
			if (isMute)
                return;
			if (PitchChange)
				soundFx.pitch = Random.Range(0.8f, 1.2f);
			int index = ClipIndex[Clips[Random.Range(0, Clips.Length)]];
			soundFx.PlayOneShot(SoundFiles[index], volume);
		}
		public void PlayButtonSound()
		{
			Play("button", PitchChange: true);
		}
	}
}
