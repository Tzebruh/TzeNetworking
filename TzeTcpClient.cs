using System.Net.Sockets;
using System.Text;

namespace TzeNetworking;

/// <summary>
/// The TzeNetworking abstraction of `System.Net.Sockets.TcpClient`.
/// </summary>
public class TzeTcpClient
{
	/// <summary>
	/// The actual TcpClient being used.
	/// </summary>
	public TcpClient Client { get; private set; }

	#region Constructors
	/// <summary>
	/// Creates a new TzeTcpClient. Doesn't connect to anything by default.
	/// </summary>
	public TzeTcpClient()
	{
		Client = new();
	}

	/// <summary>
	/// Creates a new TzeTcpClient and connects to the provided address.
	/// </summary>
	/// <param name="connectionAddress">The URI of the address to connect to. The URI must include the port.</param>
	public TzeTcpClient(Uri connectionAddress)
	{
		Client = new();
		Client.Connect(connectionAddress.Host, connectionAddress.Port);
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
	/// Sends a disconnect TzePacket and then disposes of the internal TcpClient object.
	/// </summary>
	public void Disconnect()
	{
		TzePacket disconnectPacket = new(TzePacket.TzePacketType.Disconnect, null);
		Send(disconnectPacket);
		Client.Dispose();
	}
	#endregion

	#region Dispose
	/// <summary>
	/// Disposes of the internal TcpClient object.
	/// </summary>
	public void Dispose() => Client.Dispose();
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
}
