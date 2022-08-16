using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISetVerticalDrawing
{
    public abstract void SetImage(int index);
    public abstract void SetAnimation(int index);
    public abstract void SkipAnimation(int index);
}
