using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffCtrl : MonoBehaviour
{
    public Material mat_baseSurface;
    public Material mat_wave_test;

    public float speed_wave_test = 0.13f;
    private float waveP_test;

    public AnimationCurve animCurve_waveP;

    private void LateUpdate()
    {
        mat_wave_test.SetFloat("_SurfaceTime", mat_baseSurface.GetFloat("_SurfaceTime"));

        waveP_test += Time.deltaTime * speed_wave_test;
        if (waveP_test >= 1f)
        {
            waveP_test -= 1f;
        }
        mat_wave_test.SetVector("_WaveOffset", new Vector2(0f, animCurve_waveP.Evaluate(waveP_test)));

        float waveP2 = waveP_test + 0.3333f;
        if (waveP2 >= 1f)
        {
            waveP2 -= 1f;
        }

        mat_wave_test.SetVector("_WaveOffset2", new Vector2(0f, animCurve_waveP.Evaluate(waveP2)));


        float waveP3 = waveP_test + 0.6667f;
        if (waveP3 >= 1f)
        {
            waveP3 -= 1f;
        }

        mat_wave_test.SetVector("_WaveOffset3", new Vector2(0f, animCurve_waveP.Evaluate(waveP3)));

        mat_wave_test.SetVector("_TangentVec", transform.InverseTransformDirection(Vector3.right));

    }
}
