using I2PSharp;

SAMSession session = new SAMSession(7656);

// Connect to SAM
await session.ConnectAsync();

// Generate keys
(string PrivateKey, string PublicKey) Keys = await session.GenerateDestinationAsync();

// Create a primary session using the private key & print the public key
await session.CreateSessionAsync(Keys.PrivateKey);
Console.WriteLine("Public key: " + Keys.PublicKey);

// Create STREAM subsession
SAMSubsession subsession = await session.CreateSTREAMSubsessionAsync();
subsession.OnConnect += subsession_OnConnect;

// Get server public key
Console.Write("Server public key: ");
string PublicKey = Console.ReadLine();

// Connect to server
PeerConnection cs = await subsession.Connect(PublicKey);
cs.OnMessage += Cs_OnMessage;
cs.OnDisconnect += Cs_OnDisconnect;


while (true)
{
    cs.SendString(Console.ReadLine());
}

void Cs_OnMessage(object sender, MessageEventArgs e)
{
    Console.WriteLine("Recieved: " + e.Message);
}


void subsession_OnConnect(object sender, ConnectionEventArgs e)
{
    Console.WriteLine("Subsession connected to by " + e.ConnectionType);
}

void Cs_OnDisconnect(object sender, DisconnectEventArgs e)
{
    Console.WriteLine("Peer disconnected");
}

