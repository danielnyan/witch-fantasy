using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MovementLogic : MonoBehaviour
{
    public abstract void Initialize(MovementController movementController);
    public abstract void MoveUpdate(MovementController movementController);
    public abstract void MoveFixedUpdate(MovementController movementController);
    public abstract void Cleanup(MovementController movementController);
}
