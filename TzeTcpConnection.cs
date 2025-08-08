using System.Net.Sockets;
using System.Text;

namespace TzeNetworking;

/// <summary>
/// An object that represents a client from the listener's perspective.
/// </summary>
public class TzeTcpConnection
{
	/// <summary>
	/// The actual Socket being used.
	/// </summary>
	public Socket ClientSocket { get; private set; }

	/// <summary>
	/// Whether the Socket appears to be disconnected or not.
	/// </summary>
	public bool Disconnected { get; private set; } = false;

	private CancellationTokenSource cancellationSource = new();

	#region Event Definitions
	/// <summary>
	/// Called when a TzePacket has been received from the client to which this TzeTcpConnection refers.
	/// </summary>
	public event Action<TzePacket, TzeTcpConnection>? OnReceive;

	/// <summary>
	/// Called when the client to which this TzeTcpConnection refers gets disconnected.
	/// </summary>
	public event Action<TzeTcpConnection>? OnDisconnect;
	#endregion

	#region Constructor/Destructor
	/// <summary>
	/// Creates a TzeTcpConnection from a Socket.
	/// </summary>
	/// <param name="clientSocket">The Socket to create the TzeTcpConnection from.</param>
	public TzeTcpConnection(Socket clientSocket)
	{
		ClientSocket = clientSocket;
		StartAsyncTasks();
	}

	/// <summary>
	/// Disconnects from and disposes of the connection when the object is removed.
	/// </summary>
	~TzeTcpConnection() => DisconnectAndDispose();
	#endregion

	#region Packet Methods
	/// <summary>
	/// Sends a TzePacket to the client to which this TzeTcpConnection refers.
	/// </summary>
	/// <param name="packet">The TzePacket to send.</param>
	public void Send(TzePacket packet)
	{
		Task.Run(async () => await ClientSocket.SendAsync(packet.SerializedPacket, SocketFlags.None, cancellationSource.Token));
	}

	/// <summary>
	/// Sends the provided data to the client to which this TzeTcpConnection refers, not formatted as a TzePacket.
	/// </summary>
	/// <param name="data">The data to send as a byte array.</param>
	public void SendRaw(byte[] data)
	{
		Task.Run(async () => await ClientSocket.SendAsync(data, SocketFlags.None, cancellationSource.Token));
	}

	/// <summary>
	/// Sends the provided data to the client to which this TzeTcpConnection refers, not formatted as a TzePacket.
	/// </summary>
	/// <param name="data">The data to send as a string.</param>
	public void SendRaw(string data)
	{
		byte[] byteData = Encoding.UTF8.GetBytes(data);
		Task.Run(async () => await ClientSocket.SendAsync(byteData, SocketFlags.None, cancellationSource.Token));
	}
	#endregion

	#region DisconnectAndDispose
	/// <summary>
	/// Disconnects, unsubscribes all handlers from all events, and disposes of the internal Socket object.
	/// </summary>
	public void DisconnectAndDispose()
	{
		if (OnReceive != null)
		{
			foreach (Delegate del in OnReceive.GetInvocationList()) OnReceive -= (Action<TzePacket, TzeTcpConnection>)del;
		}
		if (OnDisconnect != null)
		{
			foreach (Delegate del in OnDisconnect.GetInvocationList()) OnDisconnect -= (Action<TzeTcpConnection>)del;
		}

		Disconnected = true;
		cancellationSource.Cancel();
		ClientSocket.Disconnect(false);
		ClientSocket.Dispose();
		cancellationSource.Dispose();
	}
	#endregion

	#region Internal Methods
	private void StartAsyncTasks()
	{
		Task.Run(() => {
			try
			{
				while (true)
				{
					byte[] buffer = new byte[ClientSocket.ReceiveBufferSize];
					int receivedBytes = ClientSocket.Receive(buffer, SocketFlags.None);

					if (buffer[0] == default)
					{
						if (!Disconnected)
						{
							OnDisconnect?.Invoke(this);
							DisconnectAndDispose();
						}
					}

					List<byte> bufferAsList = buffer.ToList();
					bufferAsList.RemoveRange(receivedBytes, bufferAsList.Count - receivedBytes);
					buffer = bufferAsList.ToArray();

					TzePacket? packet = TzePacket.FromSerializedPacket(buffer);
					if (packet != null && packet.Value.PacketType == TzePacket.TzePacketType.Disconnect)
					{
						OnDisconnect?.Invoke(this);
						DisconnectAndDispose();
						continue;
					}
					OnReceive?.Invoke(packet ?? new TzePacket(TzePacket.TzePacketType.Message, buffer), this);
				}
			}
			catch (Exception ex)
			{
				if (ex is SocketException || ex is ObjectDisposedException)
				{
					if (!Disconnected)
					{
						OnDisconnect?.Invoke(this);
						DisconnectAndDispose();
					}
				}
				else throw;
			}
		}, cancellationSource.Token);
	}
	#endregion
}
