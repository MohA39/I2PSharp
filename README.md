
![Banner](https://i.postimg.cc/TPCt0qc5/Untitled-1.png)
I2PSharp is a .NETStandard 2.0 implementation of the I2P anonymous network layer SAM v3.3 protocol outlined on the [official I2P website](https://geti2p.net/en/docs/api/samv3). It aims to provide developers with a jumpstart into the world of censorship-resistant, peer-to-peer applications with the least-possible learning curve and the most flexibility.

## Documentation
I2PSharp applications must first create a primary session and connect it to SAM using the correct port number (default: 7656) then create a sub-session (which does the actual communication) as below:
```C#
// Default SAM port: 7656
SAMSession session = new SAMSession(7656);
// Connect to SAM
await session.ConnectAsync();
// Create a primary session 
await session.CreateSessionAsync();
// Create STREAM subsession
SAMSubsession subsession = await session.CreateSTREAMSubsessionAsync();
// Event when subsession connects
subsession.OnConnect += subsession_OnConnect;
```

Please note that using negative ports would generate an unused port in `session.CreateSTREAMSubsessionAsync`. `Utils.GenID(int MinLength, int MaxLength);` may be used to generate a random subsession ID. You may also prefer to generate your own keys for applications where, for example, you need to consistently connect the same two users together. In which case, you can use the `session.GenerateDestinationAsync()` function, which could be used as shown below:

```C# 
// Connect to SAM
// Generate keys
(string PrivateKey, string PublicKey) Keys = await session.GenerateDestinationAsync();

// Create a primary session using the private key & print the public key
await session.CreateSessionAsync(Keys.PrivateKey);
Console.WriteLine("Public key: " + Keys.PublicKey); // To be sent to another client to allow them to connect

// Create STREAM subsession as normal
SAMSubsession subsession = await session.CreateSTREAMSubsessionAsync();
```

After creating a subsession, you may end it using `session.EndSubsession(SubsessionID)` function. To begin communicating, you must either connect to a peer or begin accepting connections and have a peer connect to you. A single subsession can connect to multiple peers and accept multiple peers. To connect to a peer, use `subsession.ConnectAsync(PublicKey);`, which, once accepted would return a `PeerConnection`. To begin accepting a peer, use `subsession.AcceptConnectionAsync();`, which, once accepted would also return a `PeerConnection`.  

PeerConnection allows you to communicate with peers and has two events, `peerConnection.OnMessage`, which fires whenever you receive a message if you're waiting for messages (always assumes strings, stop listening for messages if you'd like to read bytes), and `peerConnection.OnDisconnect`, which fires whenever connection to the peer is lost.  To send a message, use `peerConnection.SendString();` or `peerConnection.SendBytes();` for bytes. You may stop listening for messages using `peerconnection.StopWaitingForMessages();`. 

For further information, check out the official [I2P SAM v3.3 protocol documentation](https://geti2p.net/en/docs/api/samv3) and the examples provided in the project.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
