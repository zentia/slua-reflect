using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lua_DelegateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int TestMethod(int a, int b)
    {
        return a + b;
    }
    public string TestMethod(string name)
    {
        return name;
    }
}
