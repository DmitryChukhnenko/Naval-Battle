using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Client;

public enum MessageType {
    InvalidMessage,

    Lobby_ClientToServer_CreateGameModel,
    Lobby_ClientToServer_MyName,
    Lobby_ClientToServer_Close,
    Lobby_ClientToServer_Exit,

    Lobby_ServerToClient_Close,
    Lobby_ServerToClient_CreateGameModel,
    Lobby_ServerToClient_Name,

    Arrangement_ClientToServer_Player,
    Arrangement_ClientToServer_Close,
    Arrangement_ClientToServer_Exit,

    Arrangement_ServerToClient_Wait,
    Arrangement_ServerToClient_Close,

    Game_ClientToServer_NicknameCell,

    Game_ServerToClient_YouLost,
    Game_ServerToClient_Move,
    Game_ServerToClient_ChangedCells,
    Game_ServerToClient_Winner,
}

public class Message {
    public MessageType Type { get; set; } = MessageType.InvalidMessage;
    public string[] Args { get; set; } = Array.Empty<string> ();

    public Message () { }
    public Message (MessageType type, object[] args) {
        Type = type;
        Args = args.Select (arg => JsonSerializer.Serialize (arg, new JsonSerializerOptions {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create (UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        })).ToArray ();
    }

    public Message ExpectType (MessageType type) {
        if (Type != type) throw new InvalidOperationException ($"Expected {type}");
        return this;
    }

    public T DeserializeArg<T> (int index) =>
        JsonSerializer.Deserialize<T> (Args[index]) ?? throw new NullReferenceException ();

    public T1 Deserialize1Arg<T1> () =>
        DeserializeArg<T1> (0);
    public (T1, T2) Deserialize2Args<T1, T2> () =>
        (DeserializeArg<T1> (0), DeserializeArg<T2> (1));
    public (T1, T2, T3) Deserialize3Args<T1, T2, T3> () =>
        (DeserializeArg<T1> (0), DeserializeArg<T2> (1), DeserializeArg<T3> (2));

    public override string ToString () => $"{Type}\r\n{string.Join ("\r\n", Args)}";
}
