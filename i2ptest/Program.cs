using I2PSharp;

SAMSession session = new SAMSession(7656);

await session.ConnectAsync();
var Dests = await session.GenerateDestinationAsync();
await session.CreateSessionAsync(Dests.PrivateKey);

Console.WriteLine("Public key: " + Dests.PublicKey);

SAMSubsession sub = await session.CreateSTREAMSubsessionAsync();
sub.OnConnect += Sub_OnConnect;

Console.Write("Connect to key: ");
string PublicKey = Console.ReadLine();

PeerConnection cs = await sub.Connect(PublicKey);
cs.OnMessage += Sub_OnMessage;
cs.OnDisconnect += Cs_OnDisconnect;

void Cs_OnDisconnect(object sender, DisconnectEventArgs e)
{
    Console.WriteLine("Peer disconnected");
}

while (true)
{
    Console.Write("Send: ");
    string Message = Console.ReadLine();
    cs.SendString(Message);
}

void Sub_OnMessage(object sender, MessageEventArgs e)
{
    Console.WriteLine("Recieved: " + e.Message);
}


void Sub_OnConnect(object sender, ConnectionEventArgs e)
{
    Console.WriteLine("Subsession connected to by " + e.ConnectionType);
}