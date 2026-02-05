using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class JatosInterface
{
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD

    [DllImport("__Internal")]
    private static extern void sendResultDataToJatos(string data);

    [DllImport("__Internal")]
    private static extern void startNextJatosEvent();

    // Static method to call the JavaScript function
    public static void SendResultDataToJatos(string data)
    {
        sendResultDataToJatos(data);
    }

    public static void StartNextJatosEvent()
    {
        startNextJatosEvent();
    }

#else
    public static void SendResultDataToJatos(string data)
    {
        Debug.Log(data);
    }

    public static void StartNextJatosEvent()
    {
        Debug.Log("Starting next Jatos component.");
    }
#endif
}