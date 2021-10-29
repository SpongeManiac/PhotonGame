using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyControl
{
    public KeyCode key;
    public Action keyDown;
    public Action keyUp;

    public KeyControl(KeyCode key, Action keyDown, Action keyUp)
    {
        this.key = key;
        this.keyDown = keyDown;
        this.keyUp = keyUp;
    }
}
