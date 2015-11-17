using UnityEngine;
using System.Collections;


namespace PG {
	public class Particle : MonoBehaviour {

		// age in seconds
		public float age = 0f;
		public Vector3 positionStart;
		public Vector3 positionEnd;
		public Vector3 positionDelta = new Vector3 (0, 0, -9);
		// lifespan in seconds
		public float lifespan = 5f;

		// Grid mode properties
		public Vector3 gridPosition;

		public float ageRatio {
			get { return age / lifespan; }
		}

		public bool isDead {
			get { return age >= lifespan; }
		}

		// -------------------------------
		// view

		public Color color {
			set {
				Material m = gameObject.transform.Find("Plane").GetComponent<Renderer>().material;
				Color oc = m.GetColor("_Color");
				value.a = oc.a;
				m.SetColor ("_Color", value);
			}
		}

		void Update(){

		}


	}
}