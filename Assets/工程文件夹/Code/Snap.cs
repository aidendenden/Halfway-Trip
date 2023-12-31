using System;
using UnityEngine;

namespace 工程文件夹.Code
{
    public class Snap : MonoBehaviour
    {
        // public LayerMask targetLayer; // 目标层级
        // public Camera mainCamera; // 主摄像机
        //
        // void Start()
        // {
        //     mainCamera = Camera.main;
        // }
        //
        // void Update()
        // {
        //     Collider[] colliders = Physics.OverlapSphere(mainCamera.transform.position, mainCamera.farClipPlane, targetLayer);
        //     
        //     for (int i = 0; i < colliders.Length; i++)
        //     {
        //         Collider collider = colliders[i];
        //         GameObject visibleObject = collider.gameObject;
        //         Vector3 viewportPos = mainCamera.WorldToViewportPoint(visibleObject.transform.position);
        //     
        //         bool isVisible = (viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1 &&
        //                           viewportPos.z > 0);
        //         if (isVisible)
        //         {
        //             Debug.Log("物体进入屏幕范围：" + visibleObject.name);
        //             // 在此处执行您想要的操作
        //         }
        //     
        //     }
        // }
        //
        //
        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawWireSphere(mainCamera.transform.position, mainCamera.farClipPlane);
        // }
        //


        bool isRendering;
        float curtTime = 0f;
        //float lastTime = 0f;
        private Vector3 targetPos;
        public Camera _camera;

        private void Start()
        {
            _camera=Camera.main;
        }

        void OnWillRenderObject()
        {
            curtTime = Time.time;
        }

        public bool IsInView(Vector3 worldPos)
        {
            Transform camTransform = _camera.transform;
            Vector2 viewPos = _camera.WorldToViewportPoint(worldPos);
            Vector3 dir = (worldPos - camTransform.position).normalized;
            float dot = Vector3.Dot(camTransform.forward, dir); //判断物体是否在相机前面


            if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
                return true;
            else
                return false;
        }


        void Update()
        {
            Vector2 vec2 = _camera.WorldToScreenPoint(this.gameObject.transform.position);

            if (IsInView(transform.position))
            {
                Debug.Log("目前本物体在摄像机范围内");
            }
            else
            {
                Debug.Log("目前本物体不在摄像机范围内");
            }

        }

        
          
        //物体出现在屏幕  
        void OnBecameVisible()
        {
            Debug.Log(this.name.ToString() + "这个物体出现在屏幕里面了");
        }

        //物体离开屏幕  
        void OnBecameInvisible()
        {
            Debug.Log(this.name.ToString() + "这个物体离开屏幕里面了");
        }


        public bool IsVisible(Bounds bounds, Camera camera)
        {
            // 得到摄像机的六个面 

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            // 判断边框bound是否在六个面内

            //需要给游戏对象添加Collider
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
    
        private void Detectable()
        {
            #region 优雅方法
        
            //IsInScreen(targetPos);

            #endregion

            #region 粗暴方法

            // Collider[] colliders = Physics.OverlapSphere(snapshotCamera.transform.position, snapshotCamera.farClipPlane, DetectableTargetLayer);
            //
            // for (int i = 0; i < colliders.Length; i++)
            // {
            //     Collider collider = colliders[i];
            //     GameObject visibleObject = collider.gameObject;
            //     Vector3 viewportPos = snapshotCamera.WorldToViewportPoint(visibleObject.transform.position);
            //
            //     bool isVisible = (viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1 &&
            //                       viewportPos.z > 0);
            //     if (isVisible)
            //     {
            //         Debug.Log("物体进入屏幕范围：" + visibleObject.name);
            //         // 在此处执行您想要的操作
            //     }
            //
            // }

            // Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            // RaycastHit hit;
            // if ( Physics.Raycast(ray, out hit, DetectableTargetLayer))
            // {
            //     GameObject hitObject = hit.collider.gameObject;
            //     Debug.Log("检测到物体：" + hitObject.name);
            // }

            #endregion

        }


        bool IsInScreen(Vector3 targetPos)
        {
            Vector3 viewPos = Camera.main.WorldToViewportPoint(targetPos);
            Vector3 dir = (Camera.main.transform.position - viewPos).normalized;
            float dot = Vector3.Dot(Camera.main.transform.forward, dir);
            return dot > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;
        }



        
    }
}