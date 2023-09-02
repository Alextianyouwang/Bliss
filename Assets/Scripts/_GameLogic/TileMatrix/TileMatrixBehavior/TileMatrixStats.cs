
using UnityEngine;

public class TileMatrixStats 
{
    public Vector3 center;
    public float radius;
    public float innerRadius;
    public float outerRadius;
    public float coneShapeMultiplier;
    public float yOffset;
    public float noiseWeight;
    public float dampSpeed;

    public TileMatrixStats(
        Vector3 _center, 
        float _radius, 
        float _innerRadius, 
        float _outerRadius, 
        float _coneShapeMultiplier, 
        float _yOffset, 
        float _noiseWeight, 
        float _dampSpeed) 
    {
        center = _center;
        radius = _radius;  
        innerRadius = _innerRadius; 
        outerRadius = _outerRadius;
        coneShapeMultiplier = _coneShapeMultiplier;
        yOffset = _yOffset;
        noiseWeight = _noiseWeight;
        dampSpeed = _dampSpeed;
    }

}
