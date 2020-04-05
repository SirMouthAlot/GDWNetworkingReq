using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct CS_to_Plugin_Functions
{
    //public IntPtr testingfunc;
    //public IntPtr functionTwo;
    public IntPtr MsgReceivedPtr;

    // The functions don't need to be the same though
    // Init isn't in C++
    public bool Init()
    {
        MsgReceivedPtr = Marshal.GetFunctionPointerForDelegate(new Action<IntPtr>(NetworkingManager.MsgReceived));

        return true;
    }
}

public class ClientConnectionData
{
    public string name;
    public string status;
    public int id;
}

public class MsgToPopulate
{
    public string msg;
    public int id;
}

public class PositionUpdate
{
    public PositionUpdate(Vector3 pos, string id)
    {
        position = pos;
        objID = id;
    }

    public Vector3 position;
    public string objID;
}

public class RotationUpdate
{
    public RotationUpdate(Vector3 rot, string id)
    {
        rotation = rot;
        objID = id;
    }

    public Vector3 rotation;
    public string objID;
}

public class VelocityUpdate
{
    public VelocityUpdate(Vector3 vel, string id)
    {
        velocity = vel;
        objID = id;
    }

    public Vector3 velocity;
    public string objID;
}

public class NetworkingManager : MonoBehaviour
{
    public static NetworkingManager m_instance;

    // Same old DLL init stuff
    private const string path = "/Plugins/x86_64/NetworkingTutorialDLL";

    private IntPtr Plugin_Handle;
    private CS_to_Plugin_Functions Plugin_Functions;

    public delegate void InitDLLDelegate(CS_to_Plugin_Functions funcs);
    public InitDLLDelegate InitDLL;

    public delegate bool InitServerDelegate(string IP, int port);
    public InitServerDelegate InitServer;

    public delegate bool InitClientDelegate(string IP, int port, string name);
    public InitClientDelegate InitClient;

    public delegate void SendPacketToServerDelegate(string msg);
    public SendPacketToServerDelegate SendPacketToServer;

    public delegate void CleanupDelegate();
    public CleanupDelegate Cleanup;

    // MUST be called before you call any of the DLL functions
    private void InitDLLFunctions()
    {
        InitDLL = ManualPluginImporter.GetDelegate<InitDLLDelegate>(Plugin_Handle, "InitDLL");
        InitServer = ManualPluginImporter.GetDelegate<InitServerDelegate>(Plugin_Handle, "InitServer");
        InitClient = ManualPluginImporter.GetDelegate<InitClientDelegate>(Plugin_Handle, "InitClient");
        SendPacketToServer = ManualPluginImporter.GetDelegate<SendPacketToServerDelegate>(Plugin_Handle, "SendPacketToServer");
        Cleanup = ManualPluginImporter.GetDelegate<CleanupDelegate>(Plugin_Handle, "Cleanup");
    }

    //Fields for username, ip and errors
    public InputField username;
    public InputField ipAddress;
    public Button yesButton;
    public Button noButton;
    public Text errorMessage;
    public Text waitingMessage;

    public static string savedUsername;
    // public GameObject loginCanvas;
    public float sendInterval = 0.1f;
    public float errorTime = 10.0f;
    public float activityTime = 80.0f;
    public float mutexTime = 0.5f;
    private float errorCounter = 0;
    private float mutexCounter = 0;
    private float activityCounter = 0;

    private bool errorMessageExist = false;

    private static bool mutex = false;

    static List<MsgToPopulate> msgs = new List<MsgToPopulate>();
    public static List<ClientConnectionData> clients = new List<ClientConnectionData>();
    public static ClientConnectionData user;
    public static List<PositionUpdate> positionUpdates = new List<PositionUpdate>();
    public static List<RotationUpdate> rotationUpdates = new List<RotationUpdate>();
    public static List<VelocityUpdate> velocityUpdates = new List<VelocityUpdate>();


    // Init the DLL
    private void Awake()
    {
        m_instance = this;

        //Store reference to networking manager in GameManager
        GameManager.m_networkingManager = this;
        GameManager.m_sendInterval = sendInterval;
        //Make sure this object persists in between scenes
        DontDestroyOnLoad(this);

        Plugin_Handle = ManualPluginImporter.OpenLibrary(Application.dataPath + path);

        Plugin_Functions.Init();

        InitDLLFunctions();

        InitDLL(Plugin_Functions);
    }

    private void UpdateExists()
    {
        if (errorMessage != null)
        {
            errorMessageExist = true;
        }
        else
        {
            errorMessageExist = false;
        }
    }

    private void Update()
    {
        UpdateExists();

        mutexCounter += Time.fixedDeltaTime;
        if (mutexCounter >= mutexTime)
        {
            mutex = true;

            UpdateData();

            mutex = false;
        }

        activityCounter += Time.fixedDeltaTime;
        if (activityCounter >= activityTime)
        {
            SendPacketToServer("s;" + user.id.ToString() + ";IDLE");
            activityCounter = 0;
        }

        if (errorMessageExist)
        {
            if (errorMessage.gameObject.activeSelf)
            {
                errorCounter += Time.fixedDeltaTime;
            }

            if (errorCounter >= errorTime)
            {
                errorMessage.gameObject.SetActive(false);
                username.colors = ColorBlock.defaultColorBlock;
                errorCounter = 0.0f;
            }
        }
    }

    // Update client and message data
    private void UpdateData()
    {
        WatchedObject[] watched = GameObject.FindObjectsOfType<WatchedObject>();

        foreach (WatchedObject watch in watched)
        {
            if (!watch.m_owned)
            {
                if (watch.m_updatePos)
                {
                    if (positionUpdates.Count > 0)
                    {
                        for (int i = 0; i < positionUpdates.Count; i++)
                        {
                            if (positionUpdates[i].objID == watch.m_finalObjectID)
                            {
                                watch.gameObject.GetComponent<Rigidbody>().MovePosition(positionUpdates[i].position);

                                //Removes front of the queue (the position from 2 packets ago)
                                watch.positions.Dequeue();
                                //Adds the newest position update to the back of the queue
                                watch.positions.Enqueue(positionUpdates[i].position);

                                positionUpdates.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                if (watch.m_updateRot)
                {
                    if (rotationUpdates.Count > 0)
                    {
                        for (int i = 0; i < rotationUpdates.Count; i++)
                        {
                            if (rotationUpdates[i].objID == watch.m_finalObjectID)
                            {
                                watch.gameObject.transform.rotation = Quaternion.Euler(rotationUpdates[i].rotation);

                                rotationUpdates.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                if (watch.m_updateVel)
                {
                    if (velocityUpdates.Count > 0)
                    {
                        for (int i = 0; i < velocityUpdates.Count; i++)
                        {
                            if (velocityUpdates[i].objID == watch.m_finalObjectID)
                            {
                                watch.gameObject.GetComponent<Rigidbody>().velocity = velocityUpdates[i].velocity;

                                velocityUpdates.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    // Init the server
    public void StartServer()
    {
        if (username.text != "")
        {
            if (ipAddress.text == "")
            {
                ipAddress.text = "127.0.0.1";
            }

            bool serverStatus = InitServer(ipAddress.text, 54000);

            if (!serverStatus)
            {
                errorMessage.text = "Server failed to start";
                errorMessage.gameObject.SetActive(true);

                if (ipAddress.text == "127.0.0.1")
                {
                    ipAddress.text = "";
                }
            }
        }
    }

    // Init the client
    public void StartClient()
    {
        if (username.text != "")
        {

            if (ipAddress.text == "")
            {
                ipAddress.text = "127.0.0.1";
            }
            bool connectionStatus = InitClient(ipAddress.text, 54000, username.text);

            savedUsername = username.text;

            waitingMessage.gameObject.SetActive(true);
            waitingMessage.color = Color.blue;
            waitingMessage.text = "Waiting for other players...";

            noButton.interactable = false;
            yesButton.interactable = false;

            if (!connectionStatus)
            {
                errorMessage.text = "Invalid Server IP";
                errorMessage.gameObject.SetActive(true);

                if (ipAddress.text == "127.0.0.1")
                {
                    ipAddress.text = "";
                }
            }

        }
        else
        {
            errorMessage.text = "Please enter a username";
            errorMessage.gameObject.SetActive(true);
            ColorBlock temp = ColorBlock.defaultColorBlock;
            temp.normalColor = Color.red;
            username.colors = temp;
        }
    }

    // Where we'll process incoming messages
    public static void MsgReceived(IntPtr p_in)
    {
        string p = Marshal.PtrToStringAnsi(p_in);
        Debug.Log(p);

        while (mutex)
        { } // wait
        // Look up mutex or semaphore

        switch (p[0])
        {
            case 'i': //i;UserID
                {
                    //Splits sptring into array, splitting whereever there's a ;
                    string[] ar = p.Split(';');
                    user = new ClientConnectionData();
                    user.name = savedUsername;
                    user.id = int.Parse(ar[1]);
                    //May want to use TryParse (avoids breaking shit)

                    break;
                }
            case 'u':   //u;USERNAMEOVERRIDE
                {
                    string[] ar = p.Split(';');
                    
                    clients[0].name = ar[1];
                    break;
                }
            case 'c':   //c;NAME;STATUS;ClientID
                {
                    ClientConnectionData temp = new ClientConnectionData();
                    string[] ar = p.Split(';');

                    temp.name = ar[1];
                    temp.status = ar[2];
                    temp.id = Int16.Parse(ar[3]);

                    clients.Add(temp);

                    NetworkingManager.m_instance.SendPacketToServer("u;" + user.name);

                    break;
                }
            case 's':   //s;ClientID;STATUS
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);

                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].id == id)
                        {
                            clients[i].status = ar[2];

                            return;
                        }
                    }

                    break;
                }
            case 'm':   //m;clientID;MESSAGE
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    string msg = ar[2];

                    MsgToPopulate msgp = new MsgToPopulate();
                    msgp.msg = msg;
                    msgp.id = id;

                    msgs.Add(msgp);

                    break;
                }
            case 'p':   //p;objID;PosX;PosY;PosZ
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    string objID = ar[2];
                    Vector3 pos = new Vector3(float.Parse(ar[3]), float.Parse(ar[4]), float.Parse(ar[5]));

                    //Position updates list
                    positionUpdates.Add(new PositionUpdate(pos, objID));
                    break;
                }
            case 'r':   //r;clientID;objID;rotX;rotY;rotZ
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    string objID = ar[2];
                    Vector3 rot = new Vector3(float.Parse(ar[3]), float.Parse(ar[4]), float.Parse(ar[5]));

                    //Rotation updates list
                    rotationUpdates.Add(new RotationUpdate(rot, objID));
                    break;
                }
            case 'v':   //v;clientID;objID;velX;velY;velZ
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    string objID = ar[2];
                    Vector3 velocity = new Vector3(float.Parse(ar[3]), float.Parse(ar[4]), float.Parse(ar[5]));

                    //Velocity updates list
                    velocityUpdates.Add(new VelocityUpdate(velocity, objID));
                    break;
                }
            case 'e':   //e;clientID;score
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    int score = int.Parse(ar[2]);

                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].id == id)
                        {
                            GameManager.m_otherPlayers[i].score = score;
                        }
                    }
                    break;
                }
        }
    }

    private void OnApplicationQuit()
    {
        Cleanup();
        ManualPluginImporter.CloseLibrary(Plugin_Handle);
    }
}
