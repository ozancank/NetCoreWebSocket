using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

app.UseWebSockets();

app.Use(async (httpContext, nextMsg) =>
{
    Console.WriteLine("Web Socket is Listening");
    if (httpContext.Request.Path == "/message")
    {
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await Talk(httpContext, webSocket);
        }
        else
        {
            httpContext.Response.StatusCode = 400;
        }

    }
    else
    {
        await nextMsg();
    }

});

app.Run();

static async Task Talk(HttpContext hContext, WebSocket wSocket)
{
    var bag = new byte[1024];
    WebSocketReceiveResult result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);

    while (!result.CloseStatus.HasValue)
    {
        var inComingMesage = Encoding.UTF8.GetString(bag, 0, result.Count);
        Console.WriteLine("\nClients says that: '{0}'", inComingMesage);
        var rnd = new Random();
        var number = rnd.Next(1, 100);
        string message = string.Format("You luck Number is '{0}'. Dont't remember that", number.ToString());
        byte[] outGoingMeesage = Encoding.UTF8.GetBytes(message);
        await wSocket.SendAsync(new ArraySegment<byte>(outGoingMeesage, 0, outGoingMeesage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

        result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);

    }
    await wSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

}