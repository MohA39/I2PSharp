using I2PSharp;
using System.Threading;

// Default SAM port: 7656
SAMSession session = new SAMSession(7656);

// List of all accepted peers.
List<PeerConnection> connections = new List<PeerConnection>();

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

new Thread(new ThreadStart(async () => 
{
    while (true)
    {
        // Wait for a connection
        PeerConnection cs = await subsession.AcceptConnections();
        cs.OnMessage += Cs_OnMessage;
        cs.OnDisconnect += Cs_OnDisconnect;

        connections.Add(cs);
    }
})).Start();



while (true)
{
    string MessageToSend = Console.ReadLine();
    foreach (PeerConnection cs in connections)
    {
        // Send the message
        cs.SendString(MessageToSend);
    }
}

void Cs_OnDisconnect(object sender, DisconnectEventArgs e)
{
    Console.WriteLine("Peer disconnected");
}

void Cs_OnMessage(object sender, MessageEventArgs e)
{
    Console.WriteLine("Recieved: " + e.Message);
}


void subsession_OnConnect(object sender, ConnectionEventArgs e)
{
    Console.WriteLine("Subsession connected by " + e.ConnectionType);
}