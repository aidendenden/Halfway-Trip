using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunManager : MonoBehaviour
{
    private Material sky1;
    private Transform _Sun_T;
    private Light _Sun_L;
    public float _Day_Speed = 1;
    public float TimeNow;
    
    //请把这个脚本挂载在太阳上面
    void Start()
    {
        _Sun_T = gameObject.GetComponent<Transform>();
        _Sun_L = gameObject.GetComponent<Light>();
       
    }


    void Update()
    {
        
       
       
            _Sun_T.Rotate(_Day_Speed * Time.deltaTime, _Day_Speed/2 * Time.deltaTime, 0);
        TimeNow = Mathf.Abs(_Sun_T.rotation.x);
        _SunLightTChange(TimeNow);




    }

    void _SunLightTChange(float time)
    {
        float r =Mathf.Abs( 0.5f - time);

        _Sun_L.colorTemperature = 18000 - 38000*r;
        _Sun_L.intensity = 1 * r;
        
        
    }
    
   



}
