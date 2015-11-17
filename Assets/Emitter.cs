using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PG;


namespace PG {

	public class Emitter : MonoBehaviour {
		
		//--------------------------------------
		// Statics

		public enum Mode {
			Stream,
			Grid,
			Wash,
		};

		// End Statics
		//--------------------------------------
		// Built-ins

		public Emitter(){

		}

		void Awake(){
			colorMap = new ColorMap (colormapTexture);
		}

		// Use this for initialization
		void Start () {
			Run ();
		}
		
		// Update is called once per frame
		void Update () {
			if (!isRunning) {
				return;
			}

//			spawnRate = Time.time;
			// age particles
			// recycle dead particles
			GameObject[] array = new GameObject[activeParticles.Count];
			activeParticles.CopyTo (array);
			foreach (GameObject go in array) {
				PG.Particle p = go.GetComponent<PG.Particle>();
				p.age += Time.deltaTime;
				if (p.isDead) RecycleParticle(go);
			}

			// spawn new particles
			float spawnTime = 1 / spawnRate;
			spawnTimer += Time.deltaTime;
			if (spawnTimer > spawnTime) {
				spawnTimer = 0;
				SpawnParticles(Mathf.CeilToInt( Time.deltaTime / spawnTime ));
			}

			// update particles
			GameObject camera = GameObject.Find ("Main Camera");
			foreach (GameObject go in activeParticles) {
				UpdateParticle(go, camera);
			}
		}

		// End Built-ins
		//--------------------------------------
		// Emitter Interface

		protected bool isRunning = false;

		public Emitter Run(){
			if (isRunning) {
				return this;
			}
			isRunning = true;
			return this;
		}

		public Emitter Halt(){
			isRunning = false;
			return this;
		}

		// End Emitter Interface
		//--------------------------------------
		// Emitter properties

		// set in UI inspector
		public GameObject particlePrefab;
		public Texture2D colormapTexture;
		public ColorMap colorMap;


		// Emitter mode
		public Mode mode = Mode.Stream;

		// The number of particles to spawn per second
		public float spawnRate = 30.0f;
		protected float spawnTimer = 0;


		// grid mode props
		public Vector3 gridSegmentDimensions;
		
		private Vector3 _gridSegments = new Vector3 (15, 10, 10);
		private Vector3 gridSegments {
			get { return _gridSegments; }
			set { 
				_gridSegments = value;
				gridSegmentDimensions = new Vector3(
					_emitterSize.x / _gridSegments.x, 
					_emitterSize.y / _gridSegments.y, 
					_emitterSize.z / _gridSegments.z
				);
			}
		}

		private Vector3 _emitterSize = new Vector3 (21f, 14f, 0f);
		private Vector3 emitterOffset {
			get { return _emitterSize * -0.5f; }
		}

		private Vector3 emitterSize {
			get { return _emitterSize; }
			set { 
				_emitterSize = value;
				gridSegmentDimensions = new Vector3(
					_emitterSize.x / _gridSegments.x, 
					_emitterSize.y / _gridSegments.y, 
					_emitterSize.z / _gridSegments.z
				);
			}
		}

		//--------------------------------------
		// Object Pool
		
		protected List<GameObject> availableParticles = new List<GameObject> ();
		protected List<GameObject> activeParticles = new List<GameObject> ();

		protected List<GameObject> SpawnParticles(int count){
			// ensure the pool has enough particles to get
			while (availableParticles.Count < count) {
				availableParticles.Add(CreateParticle());
			}
			// get a list of particles to activate
			List<GameObject> toActivate = availableParticles.GetRange (0, count);
			foreach (GameObject go in toActivate) {
				// remove from available pool
				availableParticles.Remove(go);

				// add to active pool
				activeParticles.Add(go);
				// set initial conditions
				InitializeParticle(go);
			}
			return toActivate;
		}

		GameObject CreateParticle(){
			GameObject go = Instantiate (particlePrefab);
			go.transform.parent = transform;
			return go;
		}

		GameObject RecycleParticle(GameObject go){
			activeParticles.Remove (go);
			availableParticles.Add (go);
			go.SetActive(false);
			return go;
		}
		
		
		// End Object Pool
		//--------------------------------------
		// Particle functions

		GameObject InitializeParticle(GameObject go){
			Vector3 positionRatio = new Vector3(Random.value, Random.value, Random.value);
			go.SetActive(true);
			PG.Particle p = go.GetComponent<PG.Particle> ();
			p.age = 0;
			switch (mode) {
				case Mode.Stream:
					// start it at the origin
					go.transform.position = new Vector3(0,0,0);

					p.positionStart = go.transform.position;
					p.positionEnd = p.positionStart + p.positionDelta;
					//go.transform.rotation = Quaternion.identity;
					break;
				case Mode.Grid:
					// choose a grid position
					p.gridPosition = Vector3.Scale (gridSegments,positionRatio);
					p.gridPosition.x = Mathf.RoundToInt(p.gridPosition.x);
					p.gridPosition.y = Mathf.RoundToInt(p.gridPosition.y);
					p.gridPosition.z = Mathf.RoundToInt(p.gridPosition.z);

					go.transform.position = Vector3.Scale(p.gridPosition, gridSegmentDimensions) + emitterOffset;
					p.color = colorMap.getPixel(positionRatio.x, positionRatio.y);
					p.positionStart = go.transform.position;
					p.positionEnd = p.positionStart + p.positionDelta;
					//go.transform.rotation = Quaternion.identity;
				break;
				case Mode.Wash:
					go.transform.position = Vector3.Scale (emitterSize, positionRatio) + emitterOffset;
					p.color = colorMap.getPixel(positionRatio.x, positionRatio.y);
					p.positionStart = go.transform.position;
					p.positionEnd = p.positionStart + p.positionDelta;
					
					break;
			}
			return go;
		}

		GameObject UpdateParticle(GameObject go, GameObject camera){
			PG.Particle p = go.GetComponent<PG.Particle> ();
			go.transform.localPosition = Vector3.Lerp (p.positionStart, p.positionEnd, p.ageRatio);
			go.transform.LookAt (camera.transform.position);
			return go;
		}

		// End Particle functions
		//--------------------------------------

	}
}
