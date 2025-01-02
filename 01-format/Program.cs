﻿using Grpc.Net.Client;
using GrpcGeyser;

using var channel = GrpcChannel.ForAddress("https://solana-yellowstone-grpc.publicnode.com:443");
var client = new Geyser.GeyserClient(channel);
var request = new SubscribeRequest
{
    Commitment = CommitmentLevel.Confirmed
};
request.Blocks.Add("block", new SubscribeRequestFilterBlocks { AccountInclude = { "GuiU6MpLahPHSHYcsfSRjwLUm1AtZ9zP2eiLAkJMBjg" }, IncludeTransactions = true, IncludeAccounts = false, IncludeEntries = false });


var pingRequest = new SubscribeRequest
{
    Ping = new SubscribeRequestPing
    {
        Id = 1
    }
};

using var stream = client.Subscribe();

var cancellationToken = new CancellationToken();
await stream.RequestStream.WriteAsync(request);
Task responseTask = Task.Run(async () =>
{
    while (await stream.ResponseStream.MoveNext(cancellationToken))
    {
        var message = stream.ResponseStream.Current;
        Console.WriteLine(message.ToString());
    }
});

Timer timer = new(async (state) =>
{
    await stream.RequestStream.WriteAsync(pingRequest);
    ;
}, null, 1000, 5000);

// 等待响应流处理完成或用户按键退出
await Task.WhenAny(responseTask, Task.Run(() => Console.ReadKey()));