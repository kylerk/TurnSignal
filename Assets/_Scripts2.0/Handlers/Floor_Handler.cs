using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using OVRLay;

public class Floor_Handler : MonoBehaviour
{
    public Transform hmd;
    [Space(10)]
    public Twister twister;
    [Space(10)]
    public bool reversed = false;
    [Space(10)]
    public float maxYDist = 0.65f;
    public int maxTurns = 10;
    [Space(10)]
    public float turnProgress = 0f;
    public float currentTurnValue = 0f;
    [Space(10)]
    // -1: Unset, 0: Forward, 1: Right, 2: Up
    public float lastTrustedProgress = 0f;
    public int currentTrustedpoint = -1;
    public float[] pointTrust = new float[3] { 0, 0, 0 };

    void Update()
    {
        GetAllPointTrusts();

        int mostTrustedPoint = GetMostTrustedPointIndex();
        if (mostTrustedPoint != currentTrustedpoint)
        {
            if (mostTrustedPoint >= 0)
                SetCurrentTrustPoint(mostTrustedPoint);

            return;
        }

        if (currentTrustedpoint < 0)
            return;

        var trustedPoint = GetPointFromPointIndex(currentTrustedpoint);
        var trustedProgress = GetPointOnCircleProgress(trustedPoint);

        var diff = GetAdjustedDiff(trustedProgress, lastTrustedProgress);
        lastTrustedProgress = trustedProgress;

        currentTurnValue += diff;

        turnProgress = currentTurnValue / (360f * maxTurns);
        twister.twist = (reversed) ? -turnProgress : turnProgress;
    }

    public void SetCurrentTrustPoint(int pointIndex)
    {
        currentTrustedpoint = pointIndex;
        lastTrustedProgress = GetPointOnCircleProgress(
            GetPointFromPointIndex(pointIndex)
        );
    }

    void GetAllPointTrusts()
    {
        pointTrust[0] = 1f - Mathf.Abs(hmd.forward.y / maxYDist);
        pointTrust[1] = 1f - Mathf.Abs(hmd.right.y / maxYDist);
        pointTrust[2] = 1f - Mathf.Abs(hmd.up.y / maxYDist);
    }

    int GetMostTrustedPointIndex()
    {
        float f = pointTrust[0], r = pointTrust[1], u = pointTrust[2];

        if (f > r && f > u)
            return 0;
        else if (r > f && r > u)
            return 1;
        else if (u > f && u > r)
            return 2;

        return -1;
    }

    Vector3 GetPointFromPointIndex(int ind)
    {
        switch (ind)
        {
            case 0:
                return hmd.forward;
            case 1:
                return hmd.right;
            case 2:
                return hmd.up;
            default:
                return Vector3.zero;
        }
    }

    float GetAdjustedDiff(float newer, float older)
    {
        var diff = newer - older;

        if (diff > 340)
            diff = newer - (360f + older);
        else if (diff < -340)
            diff = newer - (360f - older);

        return diff;
    }

    public float GetPointOnCircleProgress(Vector3 spot)
    {
        float radius = Vector3.Distance(spot, new Vector3(0, spot.y, 0));

        float sin = (180f / Mathf.PI) * Mathf.Asin(
            Mathf.Max(-1f, (Mathf.Min(1f, spot.z / radius)))
        );

        float cos = (180f / Mathf.PI) * Mathf.Acos(
            Mathf.Max(-1f, (Mathf.Min(1f, spot.x / radius)))
        );

        float outSin = (cos > 90f) ? 180f - sin : (sin < 0f) ? 360f + sin : sin;
        float outCos = (sin < 0f) ? 360f - cos : cos;

        return (outSin + outCos) / 2f;
    }
}
