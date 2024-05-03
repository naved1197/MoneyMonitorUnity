using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class SpriteShapePointsExposer : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShape;
    public Vector3 leftPoint;
    public Vector3 midPoint;
    public Vector3 rightPoint;
    public float midPointHeight;

    public void Log(string log)
    {
        Debug.Log(log);
    }
    private void OnValidate()
    {
        UpdatePositions();
    }
    private void Update()
    {
        UpdatePositions();
    }

    void UpdatePositions()
    {
        if(spriteShape == null)
        {
            Log("Please assign sprite shape");
            return;
        }
        spriteShape.spline.SetPosition(0, leftPoint);
        spriteShape.spline.SetPosition(1, midPoint);
        spriteShape.spline.SetPosition(2, rightPoint);
        spriteShape.spline.SetHeight(1, midPointHeight);
    }
}
