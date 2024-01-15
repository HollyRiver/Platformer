using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public PhysicsMaterial2D[] mat = new PhysicsMaterial2D[2];
    Rigidbody2D PlatRigid;
    CompositeCollider2D PlatModel;

    void Awake()
    {
        PlatRigid = GetComponent<Rigidbody2D>();
        PlatModel = GetComponent<CompositeCollider2D>();
    }

    // Swift Materials Function
    public void InSlope()
    {
        PlatRigid.sharedMaterial = mat[1];
        PlatModel.sharedMaterial = mat[1];
    }

    public void OutSlope()
    {
        PlatRigid.sharedMaterial = mat[0];
        PlatModel.sharedMaterial = mat[0];
    }
}
