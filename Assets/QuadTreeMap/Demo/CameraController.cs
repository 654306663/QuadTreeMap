using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public class CameraController : MonoBehaviour
    {
        private new Camera camera;
        private float cameraMoveLerpTime = 8f;
        private float cameraRotateLerpTime = 8f;
        private Vector3 cameraPositionTemp;

        // Start is called before the first frame update
        void Start()
        {
            camera = GetComponent<Camera>();
            cameraPositionTemp = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            Zoom();

            cameraPositionTemp = new Vector3(Mathf.Clamp(cameraPositionTemp.x, moveLimitX[0], moveLimitX[1])
            , Mathf.Clamp(cameraPositionTemp.y, zoomMinPositionY, zoomMaxPositionY)
            , Mathf.Clamp(cameraPositionTemp.z, moveLimitZ[0], moveLimitZ[1]));
            float rotateX = (cameraPositionTemp.y - zoomMinPositionY) / (zoomMaxPositionY - zoomMinPositionY)
            * (zoomMaxRotateX - zoomMinRotateX) + zoomMinRotateX;
            transform.position = Vector3.Lerp(transform.position, cameraPositionTemp, cameraMoveLerpTime * Time.deltaTime);
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(rotateX, 0, 0), cameraRotateLerpTime * Time.deltaTime);
        }



        [SerializeField] private Vector2 moveLimitX = new Vector2(-200, 200);
        [SerializeField] private Vector2 moveLimitZ = new Vector2(-200, 200);
        [SerializeField] private float moveCo = 0.1f;
        private Vector3 oldMousePos;
        private void Move()
        {
            if (Input.GetMouseButtonDown(0))
            {
                oldMousePos = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 deltaPosition = Input.mousePosition - oldMousePos;
                Vector3 delta = new Vector2(deltaPosition.x, deltaPosition.y) * moveCo;

                cameraPositionTemp -= new Vector3(delta.x, 0, delta.y);
                oldMousePos = Input.mousePosition;
            }
        }

        [SerializeField] private float zoomMaxPositionY = 60;
        [SerializeField] private float zoomMinPositionY = 10;
        [SerializeField] private float zoomMaxRotateX = 70;
        [SerializeField] private float zoomMinRotateX = 50;
        Touch oldTouch1; //上次触摸点1(手指1)
        Touch oldTouch2; //上次触摸点2(手指2)
        [SerializeField] private float zoomScaleCo = 0.3f; //缩放系数
        private void Zoom()
        {
            cameraPositionTemp -= new Vector3(0, Input.mouseScrollDelta.y * 10f * zoomScaleCo, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Matrix4x4 temp = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            if (camera != null)
            {
                Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
            }
            Gizmos.matrix = temp;
        }
    }

}