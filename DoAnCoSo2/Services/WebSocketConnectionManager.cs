namespace DoAnCoSo2.Services;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketConnectionManager
{
    private readonly Dictionary<string, WebSocket> _webSockets = new Dictionary<string, WebSocket>();

    public async Task HandleWebSocketAsync(string userId, WebSocket webSocket)
    {
        _webSockets[userId] = webSocket;

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _webSockets.Remove(userId);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public async Task SendMessageAsync(string userId, string message)
    {
        if (_webSockets.TryGetValue(userId, out WebSocket socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                var messageSegment = new ArraySegment<byte>(messageBuffer);
                await socket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
