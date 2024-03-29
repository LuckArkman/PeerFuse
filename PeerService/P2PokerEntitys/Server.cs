using System.Net;
using System.Net.Sockets;
using P2PokerBean;
using P2PokerDAO;
using P2PokerEnums;
using P2pokerInterface;
using P2PokerSingleton;

namespace P2PokerEntitys;

public class Server : ServerDAO, IServer
{
    public Server(){}

    public Server(string host, int port)
    {
        controllerManager = new ControllerManager(this);
        connectDone = new ManualResetEvent(false);
        receiveDone = new ManualResetEvent(false);
        ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        Start(ipEndPoint);
    }

    void Start(IPEndPoint ipEndPoint)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipEndPoint);
        socket.Listen(100);
        Console.WriteLine($"Seed Is Started");
        Thread acceptThread = new Thread(AcceptConnections);
        acceptThread.Start();
    }

    void AcceptConnections()
    {
        socket.BeginAccept(ConnectCallback, null);
    }

    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = socket.EndAccept(ar);
            Client cl = new Client(client, this);
            cl.OnStart();
            cl.SendData(Message.PackData(new Msg(RequestCode.User, ActionCode.UserId, cl.UserID.ToString())));
            clientList.Add(cl.UserID, cl);
            socket.BeginAccept(ConnectCallback, null);
        }
        catch (SocketException e)
        {
            Console.WriteLine("In Server ::> SocketException " + e.ToString());
        }
    }

    public void SendResponse(Guid RoomId,IPlayer client, ActionCode actionCode, string data)
    {
        var db = Singleton._singleton().CreateDBContext();
        var reposit = Singleton._singleton().CreateRoomRepository(db);
        var rom = reposit.Get(RoomId);
        var cl = rom.clientList.Find(c => c.UserID == client.UserID);
    }

    public void HandleRequest(RequestCode requestCode, ActionCode actionCode, string data, IPlayer client)
    {
        controllerManager.HandleRequest(requestCode, actionCode, data, client);
    }

    public void RemoveClient(IPlayer player, Socket socket)
    {
        var db = Singleton._singleton().CreateDBContext();
        var reposit = Singleton._singleton().CreateRoomRepository(db);
        player.socket.Shutdown(SocketShutdown.Both);
        if (reposit.Get(player.roomController.UUID).clientList.Find(c => c.UserID == player.UserID) is not null)
        {
            lock (reposit.Get(player.roomController.UUID).clientList)
            {
                reposit.Get(player.roomController.UUID).clientList.Remove(player);
                Console.WriteLine($"{nameof(RemoveClient)} In Server Cliente Id {player.UserID} has be desconnected");
            }
        }
    }
}