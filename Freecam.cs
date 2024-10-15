using Invector.vCamera;
using UnityEngine;
using BepInEx;
using System.Collections;
using Invector.vCharacterController;
using Invector.vMelee;

namespace FreecamMod 
{
	internal class Freecam : MonoBehaviour
	{
		vThirdPersonCamera original_camera;
		public float desiredMoveSpeed 
		{
			get => _desiredMoveSpeed;
			private set 
			{
				_desiredMoveSpeed = value;
			}
		}
		float _desiredMoveSpeed = 5;
		IInputSystem InputManager = UnityInput.Current;
		internal bool freecamActive = false;
		void Start() 
		{
			original_camera = GetComponent<vThirdPersonCamera>();
		}

		public void setActive(bool active) 
		{
			StartCoroutine(SetMovementComponents(!active));
			freecamActive = active;
			original_camera.enabled = !active;
			GameObject sen = GameObject.Find("S-105.1");
			Rigidbody body = sen.GetComponent<Rigidbody>();
			body.velocity = Vector3.zero;
		}
		
		private Vector3? anchPosition;
		
		IEnumerator SetMovementComponents(bool active) 
		{
			GameObject sen = GameObject.Find("S-105.1");
			while (true) 
			{
				if (sen)
					break;
				sen = GameObject.Find("S-105.1");
				yield return new WaitForEndOfFrame();
			}
			GameObject v06 = GameObject.Find("V-06");
			while (true) 
			{
				if (v06)
					break;
				v06 = GameObject.Find("V-06");
				yield return new WaitForEndOfFrame();
			}
			sen.GetComponent<vThirdPersonController>().enabled = active;
			sen.GetComponent<vShooterMeleeInput>().enabled = active;
			sen.GetComponent<vMeleeManager>().enabled = active;
			v06.GetComponent<Drone>().enabled = active;
			Rigidbody body = sen.GetComponent<Rigidbody>();
			body.useGravity = active;
			if (!active)
				body.velocity = Vector3.zero;
		}
		
		void EnsureVelocityIsZero() 
		{
			GameObject sen = GameObject.Find("S-105.1");
			if (anchPosition == null)
				anchPosition = sen.transform.localPosition;
			sen.transform.localPosition = anchPosition.Value;
			Rigidbody body = sen.GetComponent<Rigidbody>();
			body.velocity = Vector3.zero;
		}
		
		void Update() 
		{
			if (freecamActive) 
			{
				float scroll = Input.GetAxis("Mouse ScrollWheel");
				desiredMoveSpeed += scroll * 10;
				EnsureVelocityIsZero();
			}
			else
				anchPosition = null;
		}
		
		void LateUpdate() 
		{
			if (freecamActive) 
			{
				float moveSpeed = desiredMoveSpeed * 0.25f;

				if (InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetKey(KeyCode.RightShift))
					moveSpeed *= 4f;
				if (InputManager.GetKey(KeyCode.LeftArrow) || InputManager.GetKey(KeyCode.A))
					transform.position += transform.right * -1 * moveSpeed;
				if (InputManager.GetKey(KeyCode.RightArrow) || InputManager.GetKey(KeyCode.D))
					transform.position += transform.right * moveSpeed;
				if (InputManager.GetKey(KeyCode.UpArrow) || InputManager.GetKey(KeyCode.W))
					transform.position += transform.forward * moveSpeed;
				if (InputManager.GetKey(KeyCode.DownArrow) || InputManager.GetKey(KeyCode.S))
					transform.position += transform.forward * -1 * moveSpeed;
				if (InputManager.GetKey(KeyCode.Space) || InputManager.GetKey(KeyCode.PageUp))
					transform.position += transform.up * moveSpeed;
				if (InputManager.GetKey(KeyCode.LeftControl) || InputManager.GetKey(KeyCode.PageDown))
					transform.position += transform.up * -1 * moveSpeed;
				float mouseX = Input.GetAxis("Mouse X");
				float mouseY = Input.GetAxis("Mouse Y");
				float newRotationX = transform.localEulerAngles.y + mouseX;// * 0.3f;
				float newRotationY = transform.localEulerAngles.x - mouseY;// * 0.3f;
				transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
			}
		}
	}
}