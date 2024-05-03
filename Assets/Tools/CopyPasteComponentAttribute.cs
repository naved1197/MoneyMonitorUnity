using System;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class CopyPasteComponentAttribute : Attribute
{
    public CopyPasteComponentAttribute(string path)
    {
    }
}
