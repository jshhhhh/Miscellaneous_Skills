using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Runtime.InteropServices;

public class TCPClientTest : MonoBehaviour
{
    //정수 유형의 recv 변수 생성
    int recv;

    //소켓에 대한 ipep 변수 생성
    //IPEndPoint 클래스 : IP 주소와 포트 번호가 있는 네트워크 끝점을 나타냄
    //IPAddress.Any 인수 : 서버가 9050포트 번호에 대한 모든 IP 주소로 연결할 수 있음을 알려줌
    IPEndPoint ipep;
    //통신할 소켓인 newsork 생성
    //AddressFamily.InterNetwork : 로컬 IP 주소를 원한다고 알려줌
    //SocketType.Dgram 인수 : 데이터가 패킷 대신 데이터그램으로 흘러야 함을 나타냄
    //ProtocolType.Udp 인수 : 사용할 소켓의 프로토콜 유형을 알려줌
    Socket newsock;

    IPEndPoint sender;
    EndPoint Remote;

    public Player player;
    public GameObject enemy;

    //string welcome = "Welcome to my test server";

    public Packet sendPacket = new Packet();
    public Packet receivePacket = new Packet();

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        enemy = GameObject.Find("Enemy");

        ipep = new IPEndPoint(IPAddress.Any, 9050);
        newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //이 아이피 + 포트로 전송함
        sender = new IPEndPoint(IPAddress.Parse("172.30.1.104"), 9050);

        newsock.Bind(ipep);

        //보낸 사람의 IP 주소를 찾아 remote 변수에 저장하는 데 사용됨
        Remote = (EndPoint)sender;

        /*//데이터를 보낸 클라이언트의 IP주소
        print("Message received from {0}:" + Remote.ToString());
        //화면에 실제 데이터를 출력, 소켓을 통해 전송되는 데이터는 원시 형식이며 읽을 수 있도록 ASCII 문자로 변환
        print(Encoding.ASCII.GetString(data, 0, recv));*/
    }

    // Update is called once per frame
    void Update()
    {
        if (newsock.Available != 0)
        {
            receiveData();
        }
    }

    public void receiveData()
    {
        byte[] data = new byte[1024];

        newsock.Receive(data);

        receivePacket = ByteArrayToStruct<Packet>(data);

        enemy.transform.position = new Vector3(receivePacket.x, receivePacket.y, receivePacket.z);

        print("receivePacket x: " + receivePacket.x + ", y: " + receivePacket.y + ", z: " + receivePacket.z);
    }

    public void SendMsg()
    {
        SetSendPacket();
        byte[] data = StructToByteArray(sendPacket);
        newsock.Send(data, 0, data.Length, SocketFlags.None);
    }

    static float[] ConvertByteArrayToFloat(byte[] bytes)
    {
        if (bytes.Length % 4 != 0) throw new ArgumentException();

        float[] floats = new float[bytes.Length / 4];
        //print("bytes.Length: " + bytes.Length + ", floats 길이: " + floats.Length);
        for (int i = 0; i < floats.Length; i++)
        {
            floats[i] = BitConverter.ToSingle(bytes, i * 4); // 4바이트마다 시작 인덱스 부여
        }
        return floats;
    }

    static byte[] ConvertFloatToByteArray(float[] floats)
    {
        byte[] data = new byte[floats.Length * 4];
        for (int i = 0; i < floats.Length; i++)
        {
            data = BitConverter.GetBytes(floats[i]);
        }
        return data;
    }

    byte[] StructToByteArray(object obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    T ByteArrayToStruct<T>(byte[] buffer) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        if (size > buffer.Length)
        {
            throw new Exception();
        }

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, ptr, size);
        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return obj;
    }

    void SetSendPacket()
    {
        sendPacket.x = player.transform.position.x;
        sendPacket.y = player.transform.position.y;
        sendPacket.z = player.transform.position.z;
    }
}