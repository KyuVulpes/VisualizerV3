using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualizerV3.Shared {
	public class ObjectData : MonoBehaviour {
		public string Author {
			get => author;
		}

		public string Version {
			get => version;
		}

		public string Copyright {
			get => copyright;
		}

		public string ProjectName {
			get => projectName;
		}

		public Texture2D Icon {
			get => icon;
		}

		[SerializeField]
		private string author;
		[SerializeField]
		private string version;
		[SerializeField]
		private string copyright;
		[SerializeField]
		private string projectName;

		[SerializeField]
		private Texture2D icon;

	}
}
