using System.Text;
using Newtonsoft.Json;
using P2PokerDAO;
using P2PokerEnums;
using P2pokerInterface;
using P2PokerStruct;

namespace P2PokerBean;

public class Room : RoomControllerDAO
{
    public Room()
    {
    }

    public void OnStart()
    {
        if (UUID.ToString() == "00000000-0000-0000-0000-000000000000" || UUID == null)
        {
            UUID = Guid.NewGuid();
        }
    }

    public void SetClient(IPlayer cl) => client = cl;

    public void SendRoomInfo(IPlayer cl)
    {
    }

    public void SetStartGame(IPlayer client)
    {
    }

    public void OnMessageFullRoom(IPlayer client)
    {
    }

    async void SendTurn(int playerTurn)
        => await Task.CompletedTask;

    public void NextTurn() => SendTurn(turn);

    public void OnPot(string client, string[] info)
    {
    }

    public void JoinClient(IPlayer player)
    {
        if (clientList.Count <= 0) client = player;
        clientList.Add(player);
        player.SendData(Message.PackData(new Msg(RequestCode.User, ActionCode.JoinRoom, player.UserID.ToString())));
    }

    public void BroadCastMessage(IPlayer player, RequestCode requestCode, ActionCode actionCode, string message)
        => clientList.ToList().ForEach(x=>{
                if (x.UserID != player.UserID) clientList.Find(u => u.UserID == x.UserID)!.SendData(Message.PackData(new Msg(requestCode, actionCode, message)));
        });

    public void SendMessage(IPlayer player, RequestCode requestCode, ActionCode actionCode, string message)
        => player.SendData(Message.PackData(new Msg(requestCode, actionCode, message)));
}