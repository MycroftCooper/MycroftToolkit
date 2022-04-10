using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace MycroftToolkit.QuickCamera {
    public class StrategyCameraCtrl : MonoBehaviour {
        private StrategyCameraCtrlActions cameraActions;
        private InputAction moveAction;
        private Transform cameraTransform;

        #region 面板设置相关
        [BoxGroup("水平移动")]
        [SerializeField]
        private float maxSpeed = 5f;
        private float speed;
        [BoxGroup("水平移动")]
        [SerializeField]
        private float acceleration = 10f;
        [BoxGroup("水平移动")]
        [SerializeField]
        private float damping = 15f;

        [BoxGroup("垂直移动")]
        [SerializeField]
        private float stepSize = 2f;
        [BoxGroup("垂直移动")]
        [SerializeField]
        private float zoomDampening = 7.5f;
        [BoxGroup("垂直移动")]
        [SerializeField]
        private float minHeight = 5f;
        [BoxGroup("垂直移动")]
        [SerializeField]
        private float maxHeight = 50f;
        [BoxGroup("垂直移动")]
        [SerializeField]
        private float zoomSpeed = 2f;

        [BoxGroup("边缘移动")]
        [SerializeField]
        [Range(0f, 0.1f)]
        private float edgeTolerance = 0.05f;

        [BoxGroup("旋转")]
        [SerializeField]
        private float maxRotationSpeed = 1f;
        #endregion

        #region 启用与停用相关
        private void Awake() {
            cameraActions = new StrategyCameraCtrlActions();
            cameraTransform = this.GetComponentInChildren<Camera>().transform;
        }

        private void OnEnable() {
            zoomHeight = cameraTransform.localPosition.y;
            cameraTransform.LookAt(this.transform);

            lastPosition = this.transform.position;

            moveAction = cameraActions.Camera.KeyboardMove;
            cameraActions.Camera.Rotate.performed += RotateCamera;
            cameraActions.Camera.Zoom.performed += zoomCamera;
            cameraActions.Camera.Enable();
        }

        private void OnDisable() {
            cameraActions.Camera.Rotate.performed -= RotateCamera;
            cameraActions.Camera.Zoom.performed -= zoomCamera;
            cameraActions.Camera.Disable();
        }
        #endregion

        private void Update() {
            //inputs
            updateKeyboardMoveAction();
            updateMouseAtScreenEdge();
            updateMouseMoveAction();

            //move base and camera objects
            updateVelocity();
            updateMovement();
            updateZoom();
        }
        private Vector3 horizontalVelocity;
        private Vector3 lastPosition;
        private void updateVelocity() {
            horizontalVelocity = (this.transform.position - lastPosition) / Time.deltaTime;
            horizontalVelocity.y = 0f;
            lastPosition = this.transform.position;
        }

        #region 水平移动相关
        private Vector3 targetPosition;
        private Vector3 startDrag;
        private void updateKeyboardMoveAction() {
            Vector3 inputValue = moveAction.ReadValue<Vector2>().x * getCameraRight()
                        + moveAction.ReadValue<Vector2>().y * getCameraForward();

            inputValue = inputValue.normalized;

            if (inputValue.sqrMagnitude > 0.1f)
                targetPosition += inputValue;
        }

        private void updateMouseMoveAction() {
            if (!Mouse.current.middleButton.isPressed)
                return;

            //create plane to raycast to
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (plane.Raycast(ray, out float distance)) {
                if (Mouse.current.middleButton.wasPressedThisFrame)
                    startDrag = ray.GetPoint(distance);
                else
                    targetPosition += startDrag - ray.GetPoint(distance);
            }
        }

        private void updateMouseAtScreenEdge() {
            //mouse position is in pixels
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 moveDirection = Vector3.zero;

            //horizontal scrolling
            if (mousePosition.x < edgeTolerance * Screen.width)
                moveDirection += -getCameraRight();
            else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
                moveDirection += getCameraRight();

            //vertical scrolling
            if (mousePosition.y < edgeTolerance * Screen.height)
                moveDirection += -getCameraForward();
            else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
                moveDirection += getCameraForward();

            targetPosition += moveDirection;
        }

        private void updateMovement() {
            if (targetPosition.sqrMagnitude > 0.1f) {
                //create a ramp up or acceleration
                speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
                transform.position += targetPosition * speed * Time.deltaTime;
            } else {
                //create smooth slow down
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
                transform.position += horizontalVelocity * Time.deltaTime;
            }

            //reset for next frame
            targetPosition = Vector3.zero;
        }
        #endregion

        private void RotateCamera(InputAction.CallbackContext obj) {
            if (Mouse.current.rightButton.isPressed || Keyboard.current.qKey.isPressed || Keyboard.current.eKey.isPressed) {
                float inputValue = obj.ReadValue<Vector2>().x;
                transform.rotation = Quaternion.Euler(0f, inputValue * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
            } else if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.eKey.isPressed) {

            }
        }


        private float zoomHeight;
        private void updateZoom() {
            //set zoom target
            Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);
            //add vector for forward/backward zoom
            zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
            cameraTransform.LookAt(this.transform);
        }

        private void zoomCamera(InputAction.CallbackContext obj) {
            float inputValue = -obj.ReadValue<Vector2>().y / 100f;

            if (Mathf.Abs(inputValue) > 0.1f) {
                zoomHeight = cameraTransform.localPosition.y + inputValue * stepSize;

                if (zoomHeight < minHeight)
                    zoomHeight = minHeight;
                else if (zoomHeight > maxHeight)
                    zoomHeight = maxHeight;
            }
        }

        //gets the horizontal forward vector of the camera
        private Vector3 getCameraForward() {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            return forward;
        }

        //gets the horizontal right vector of the camera
        private Vector3 getCameraRight() {
            Vector3 right = cameraTransform.right;
            right.y = 0f;
            return right;
        }
    }
}
