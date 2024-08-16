using System.Net.Sockets;
using System.Text;

namespace TzeNetworking;

/// <summary>
/// An event-based TCP client.
/// </summary>
public class TzeTcpClient
{
	/// <summary>
	/// The actual TcpClient being used.
	/// </summary>
	public TcpClient Client { get; private set; }

	private CancellationTokenSource cancellationSource = new();

	#region Event Definitions
	/// <summary>
	/// Called when a TzePacket has been received from the listener this client is connected to.
	/// </summary>
	public event Action<TzePacket>? OnReceive;
	#endregion

	#region Constructors
	/// <summary>
	/// Creates a new TzeTcpClient. Doesn't connect to anything by default.
	/// </summary>
	public TzeTcpClient()
	{
		Client = new();
		StartAsyncTasks();
	}

	/// <summary>
	/// Creates a new TzeTcpClient and connects to the provided address.
	/// </summary>
	/// <param name="connectionAddress">The URI of the address to connect to. The URI must include the port.</param>
	public TzeTcpClient(Uri connectionAddress)
	{
		Client = new();
		Client.Connect(connectionAddress.Host, connectionAddress.Port);
		StartAsyncTasks();
	}

	/// <summary>
	/// Creates a new TzeTcpClient and connects to the provided address.
	/// </summary>
	/// <param name="hostname">The address to connect to. (Excluding the port.)</param>
	/// <param name="port">The port to connect to.</param>
	public TzeTcpClient(string hostname, int port)
	{
		Client = new();
		Client.Connect(hostname, port);
		StartAsyncTasks();
	}
	#endregion

	#region Connection Methods
	/// <summary>
	/// Connects to the provided address.
	/// </summary>
	/// <param name="connectionAddress">The URI of the address to connect to. The URI must include the port.</param>
	public void Connect(Uri connectionAddress)
	{
		Client.Connect(connectionAddress.Host, connectionAddress.Port);
	}

	/// <summary>
	/// Connects to the provided address.
	/// </summary>
	/// <param name="hostname">The address to connect to. (Excluding the port.)</param>
	/// <param name="port">The port to connect to.</param>
	public void Connect(string hostname, int port)
	{
		Client.Connect(hostname, port);
	}

	/// <summary>
	/// Sends a disconnect TzePacket. Does NOT dispose of the internal TcpClient object.
	/// </summary>
	public void Disconnect()
	{
		TzePacket disconnectPacket = new(TzePacket.TzePacketType.Disconnect, null);
		Send(disconnectPacket);
	}

	/// <summary>
	/// Sends a disconnect TzePacket and disposes of the internal TcpClient object.
	/// </summary>
	public void DisconnectAndDispose()
	{
		Disconnect();
		Dispose();
	}
	#endregion

	#region Dispose
	/// <summary>
	/// Disposes of the internal TcpClient object.
	/// </summary>
	public void Dispose()
	{
		cancellationSource.Cancel();
		Client.Dispose();
		cancellationSource.Dispose();
	}
	#endregion

	#region Packet Methods
	/// <summary>
	/// Sends the provided TzePacket to the server.
	/// </summary>
	/// <param name="packet">The TzePacket to send.</param>
	public void Send(TzePacket packet)
	{
		NetworkStream stream = Client.GetStream();
		stream.Write(packet.SerializedPacket);
	}

	/// <summary>
	/// Sends the provided data to the server, not formatted as a TzePacket.
	/// </summary>
	/// <param name="data">The data to send as a byte array.</param>
	public void SendRaw(byte[] data)
	{
		NetworkStream stream = Client.GetStream();
		stream.Write(data);
	}

	/// <summary>
	/// Sends the provided data to the server, not formatted as a TzePacket.
	/// </summary>
	/// <param name="data">The data to send as a string.</param>
	public void SendRaw(string data)
	{
		byte[] byteData = Encoding.UTF8.GetBytes(data);
		NetworkStream stream = Client.GetStream();
		stream.Write(byteData);
	}
	#endregion

	#region Internal Methods
	private void StartAsyncTasks()
	{
		Task.Run(async () => {
			while (true)
			{
				byte[] buffer = new byte[Client.ReceiveBufferSize];
				await Task.Yield(); // Without this, the below line doesn't seem to work. I don't freaking know why this fixes it but it does
				int receivedBytes = await Client.Client.ReceiveAsync(buffer, SocketFlags.None); // Client.Client is the TcpClient's Socket class

				List<byte> bufferAsList = buffer.ToList();
				bufferAsList.RemoveRange(receivedBytes, bufferAsList.Count - receivedBytes);
				buffer = bufferAsList.ToArray();

				TzePacket? packet = TzePacket.FromSerializedPacket(buffer);
				OnReceive?.Invoke(packet ?? new TzePacket(TzePacket.TzePacketType.Message, buffer));
			}
		}, cancellationSource.Token);
	}
	#endregion
}
