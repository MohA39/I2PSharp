using I2PSharp;

SAMSession session2 = new SAMSession(7656);

// Connect to SAM
await session2.ConnectAsync();
var Dests2 = await session2.GenerateDestinationAsync();
await session2.CreateSessionAsync(Dests2.PrivateKey);
Console.WriteLine("Public key: " + Dests2.PublicKey);

SAMSubsession sub2 = await session2.CreateSTREAMSubsessionAsync();
sub2.OnConnect += Sub2_OnConnect;

PeerConnection cs = await sub2.AcceptConnections();
cs.OnMessage += Sub2_OnMessage;
cs.OnDisconnect += Cs_OnDisconnect;


while (true)
{
    Console.Write("Send: ");
    cs.SendString(Console.ReadLine());
}

void Cs_OnDisconnect(object sender, DisconnectEventArgs e)
{
    Console.WriteLine("Peer disconnected");
}

void Sub2_OnMessage(object sender, MessageEventArgs e)
{
    Console.WriteLine("Recieved: " + e.Message);
}


void Sub2_OnConnect(object sender, ConnectionEventArgs e)
{
    Console.WriteLine("Subsession connected by " + e.ConnectionType);
}