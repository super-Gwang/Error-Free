using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Port
{
    [SerializeField]
    private string id;
    [SerializeField]
    private string speed;

    public string ID => id;

    public string Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public Port(string _id, string _speed)
    {
        id = _id;
        speed = _speed;
    }
}
