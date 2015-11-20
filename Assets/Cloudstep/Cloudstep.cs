/************************************************************************************

Cloudstep - Zero Vection Locomotion Thingy

Demo Video: https://www.youtube.com/watch?v=vVVdoquKhO8

Copyright 2015
Mark Schramm - http://www.VR-Bits.com
Blair Renaud - http://irisvirtualreality.com

Apache License
Version 2.0, January 2004
http://www.apache.org/licenses/

************************************************************************************/



using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Cloudstep : MonoBehaviour {

	public bool useStepWalk=true;
	public bool showDebugCapsules=false;
	public float stepSize=1f;
	public float timeBetweenSteps=0.5f;
	public float SprintStepSize=2f;
	public float SprintTimeBetweenSteps=0.25f;
	public float maxStepHeight=0.7f;
	public float maxDropheight=2f;

	public AudioSource audioSource;
	public AudioClip stepSound;

	public float skinWidth=0.01f;
	public int excludedLayerID=100;
	public float RotationRatchet = 45.0f;
	public bool RStickTurns180=true;



	private float steptimer=0f;
	private CharacterController charactercontroller;
	private Vector3 debughitpos,debughitpos2,debughitpos3,debughitpos4;
	private Vector3 debugoutsidepos;


	private bool prevHatLeft = false;
	private bool prevHatRight = false;

	private Color nColor,sColor,eColor,wColor;
	private Vector3 possiblePos_N,possiblePos_S,possiblePos_E,possiblePos_W;
	float characterRadius;
	float characterHeight;
	// Use this for initialization
	void Start () {
	
	}




	void Awake(){

		charactercontroller=GetComponent<CharacterController>();
		characterRadius =  charactercontroller.radius+charactercontroller.skinWidth*2;
		characterHeight =  charactercontroller.height+charactercontroller.skinWidth*2;
		skinWidth=charactercontroller.skinWidth;



	}


	// Update is called once per frame
	void FixedUpdate () {

		//---------------------
		//uncomment these lines if youre using the Oculus Gamepad script
		//also make sure to initialize the gamepad script
		//float y=OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftYAxis);
		//float x=OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftXAxis);
		//---------------------
		float x=Input.GetAxis("Horizontal");;
		float y=Input.GetAxis("Vertical");;

		if (useStepWalk){
		CalculateNextPossiblePos(y,x);
		UpdateMovement(y,x);
		

		
			if (!Mathf.Approximately(y,0f) || !Mathf.Approximately(x,0f))
				steptimer+=Time.deltaTime;

			if ( Mathf.Approximately(x,0f) && Mathf.Approximately(y,0f)){
				steptimer=timeBetweenSteps;
			}
		}

		UpdateRotation();
	}







	void OnDrawGizmos() {

		if (showDebugCapsules){
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(debughitpos, 0.05f);
		Gizmos.DrawSphere(debughitpos3, 0.05f);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(debughitpos2, 0.05f);
		Gizmos.DrawSphere(debughitpos4, 0.05f);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(debugoutsidepos, 0.05f);


		//---------------------
		// To enable Debug Capsule Drawing, add this free UnityPackage to your project and uncomment the lines
		// https://www.assetstore.unity3d.com/en/#!/content/11396
		//---------------------
		Gizmos.color = nColor;
		//DebugExtension.DrawCapsule(possiblePos_N-transform.up*(characterHeight/2f),possiblePos_N+transform.up*(characterHeight/2f),nColor,characterRadius);
		Gizmos.DrawSphere(possiblePos_N, 0.05f);

		Gizmos.color = sColor;
		//DebugExtension.DrawCapsule(possiblePos_S-transform.up*(characterHeight/2f),possiblePos_S+transform.up*(characterHeight/2f),sColor,characterRadius);
		Gizmos.DrawSphere(possiblePos_S, 0.05f);

		Gizmos.color = eColor;
		//DebugExtension.DrawCapsule(possiblePos_W-transform.up*(characterHeight/2f),possiblePos_W+transform.up*(characterHeight/2f),eColor,characterRadius);
		Gizmos.DrawSphere(possiblePos_W, 0.05f);

		Gizmos.color = wColor;
		//DebugExtension.DrawCapsule(possiblePos_E-transform.up*(characterHeight/2f),possiblePos_E+transform.up*(characterHeight/2f),wColor,characterRadius);
		Gizmos.DrawSphere(possiblePos_E, 0.05f);
		}
	
	}

	public void UpdateRotation(){

		float x=Input.GetAxis("RightHorizontal");
		bool turn180=Input.GetButtonDown("RightStickClick");
		bool curHatLeft =Input.GetButton("LeftBumper")|| x<-0.5f;
		bool curHatRight = Input.GetButton("RightBumper") || x>0.5f;
		
		//---------------------
		//uncomment these lines if youre using the Oculus Gamepad script
		//also make sure to initialize the gamepad script
		//float x=OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightXAxis);
		//bool turn180=OVRGamepadController.GPC_GetButtonDown(OVRGamepadController.Button.RStick);
		//bool curHatLeft = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.LeftShoulder)|| x<-0.5f;
		//bool curHatRight = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.RightShoulder) || x>0.5f;
		//---------------------


		Vector3 euler = transform.rotation.eulerAngles;
		
	
		
		if (curHatLeft && !prevHatLeft)
			euler.y -= RotationRatchet;
		
		prevHatLeft = curHatLeft;
		


		if (turn180 && RStickTurns180)
		euler.y += 180;

		if(curHatRight && !prevHatRight)
			euler.y += RotationRatchet;
		
		prevHatRight = curHatRight;

		if (Input.GetKeyDown(KeyCode.Q))
			euler.y -= RotationRatchet;
		
		if (Input.GetKeyDown(KeyCode.E))
			euler.y += RotationRatchet;
		
	
		transform.rotation = Quaternion.Euler(euler);


	}


	public void CalculateNextPossiblePos(float y,float x){


		if (y>0 || showDebugCapsules)
		possiblePos_N=CalcDirection(1,0);

		if (y<0 || showDebugCapsules)
		possiblePos_S=CalcDirection(-1,0);

		if (x>0 || showDebugCapsules)
		possiblePos_E=CalcDirection(0,1);

		if (x<0 || showDebugCapsules)
		possiblePos_W=CalcDirection(0,-1);


	}

	private Vector3 CalcDirection(float y, float x){

		int layerMask = 1 << excludedLayerID;
		layerMask = ~layerMask;
		

		RaycastHit hit;
		Vector3 possiblePos= Vector3.zero;

		float currentStepsize= stepSize;


		if (Input.GetAxis("Sprint")>0 ) currentStepsize=SprintStepSize;
		//if (OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftTrigger)>0 ) currentStepsize=SprintStepSize;

		
		//get capsule for current pos
		Vector3 p1 = transform.position + charactercontroller.center + ( Vector3.up * -characterHeight * 0.5F ) + characterRadius*Vector3.up ;
		Vector3 p2 = p1 + Vector3.up * charactercontroller.height - 2*characterRadius*Vector3.up;
		
		
		// Cast character controller shape one step size + skinwidth forwards
		if (Physics.CapsuleCast(p1, p2, characterRadius, transform.forward*y+transform.right*x, out hit, currentStepsize+skinWidth,layerMask)){

			// if we hit an object, check if we can step over it		
			if (Physics.CapsuleCast(p1+Vector3.up*maxStepHeight, p2+Vector3.up*maxStepHeight, characterRadius, transform.forward*y+transform.right*x, out hit, currentStepsize+skinWidth,layerMask)){
				
				//if we hit there too, we could at least check how close we can get
				float distance= (Vector3.Distance(hit.point, new Vector3(transform.position.x,hit.point.y,transform.position.z)))-characterRadius;

				if (y>0) nColor=Color.red;
				if (y<0) sColor=Color.red;
				if (x>0) eColor=Color.red;
				if (x<0) wColor=Color.red;
				debughitpos=hit.point;

				if (distance>0.05f){

					//do another capsulecast to check if our feet can even move closer
					Vector3 p1_3 = transform.position+((transform.forward*y+transform.right*x)*(distance-skinWidth)) + charactercontroller.center + ( Vector3.up * -characterHeight * 0.5F ) + characterRadius*Vector3.up ;
					Vector3 p2_3 = p1_3 + Vector3.up * charactercontroller.height - 2*characterRadius*Vector3.up;

					
					if (Physics.CheckCapsule( p1_3, p2_3, characterRadius,layerMask)){
					
						//nothing can be done here
						possiblePos= Vector3.zero;
					}else{
						//yes, we can move closer
						possiblePos=transform.position+((transform.forward*y+transform.right*x)*(distance-skinWidth));
					}

				}else{
					possiblePos= Vector3.zero;
				}
				
				
				
			}else{

				//we can walk diagonaly upwards			
				if (y>0) nColor=Color.yellow;
				if (y<0) sColor=Color.yellow;
				if (x>0) eColor=Color.yellow;
				if (x<0) wColor=Color.yellow;


				//raycast from possible pos downwards
				Vector3 p1_2 = (transform.position+Vector3.up*maxStepHeight+(transform.forward*y+transform.right*x)*currentStepsize ) + charactercontroller.center + ( Vector3.up * -characterHeight * 0.5F ) + characterRadius*Vector3.up ;
				Vector3 p2_2 = p1_2 + Vector3.up * charactercontroller.height - 2*characterRadius*Vector3.up;
				float distanceToGround = 0;
				
				RaycastHit hit2;
				if (Physics.CapsuleCast( p1_2+transform.up*0.05f, p2_2+transform.up*0.05f, characterRadius, -transform.up, out hit2, maxDropheight,layerMask)){
					distanceToGround = ((transform.position+Vector3.up*maxStepHeight+(transform.forward*y+transform.right*x)*currentStepsize ) .y-hit2.point.y);	
					possiblePos=(transform.position+Vector3.up*maxStepHeight+(transform.forward*y+transform.right*x)*currentStepsize ) + Vector3.down*(distanceToGround-characterHeight/2f);}
				else
				{
					possiblePos=Vector3.zero;
				}
				
				
				
				
			}
			
		}else{
			//we can walk north without problems
			if (y>0) nColor=Color.green;
			if (y<0) sColor=Color.green;
			if (x>0) eColor=Color.green;
			if (x<0) wColor=Color.green;

			Vector3 p1_2 = (transform.position+(transform.forward*y+transform.right*x)*currentStepsize ) + charactercontroller.center + ( Vector3.up * -characterHeight * 0.5F ) + characterRadius*Vector3.up ;
			Vector3 p2_2 = p1_2 + Vector3.up * charactercontroller.height - 2*characterRadius*Vector3.up;
			float distanceToGround = 0;
			
			RaycastHit hit2;
			if (Physics.CapsuleCast( p1_2+transform.up*0.05f, p2_2+transform.up*0.05f, characterRadius, -transform.up, out hit2, maxDropheight,layerMask)){
				distanceToGround = ((transform.position+(transform.forward*y+transform.right*x)*currentStepsize ) .y-hit2.point.y);
			
			possiblePos=(transform.position+(transform.forward*y+transform.right*x)*currentStepsize ) + Vector3.down*(distanceToGround-characterHeight/2f);
			}else
			{
				possiblePos=Vector3.zero;
			}

			
		}

		return possiblePos;

	}


	public void UpdateMovement(float y,float x){


		if (steptimer>=timeBetweenSteps || (Input.GetAxis("Sprint")>0 && steptimer>= SprintTimeBetweenSteps)){
		//if (steptimer>=timeBetweenSteps || (OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftTrigger)>0 && steptimer>= SprintTimeBetweenSteps)){


		if (y!=0){


			if (y>0 && possiblePos_N!=Vector3.zero){
				transform.position=possiblePos_N;
					audioSource.PlayOneShot(stepSound);
				}

			if (y<0 && possiblePos_S!=Vector3.zero){
				transform.position=possiblePos_S;
					audioSource.PlayOneShot(stepSound);
				}
			}

			else if(x!=0){

			if (x>0 && possiblePos_E!=Vector3.zero){
				transform.position=possiblePos_E;
					audioSource.PlayOneShot(stepSound);
				}
			
			if (x<0 && possiblePos_W!=Vector3.zero){
				transform.position=possiblePos_W;
					audioSource.PlayOneShot(stepSound);
				}

			}



			steptimer=0f;
		}


	
	}




}
