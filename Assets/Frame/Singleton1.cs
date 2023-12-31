using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singletonl<T> where T : class, new()
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

    protected Singletonl() { }

    public static T Instance => instance.Value;
}
