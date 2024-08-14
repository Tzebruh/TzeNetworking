using System.Net.Sockets;
using System.Text;

namespace TzeNetworking;

/// <summary>
/// An object used to represent a client connected to a listener.
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
	/// Called when a TzePacket has been received from the client on the other end of this connection.
	/// </summary>
	public event Action<TzePacket>? OnReceive;

	/// <summary>
	/// Called when the client on the other end of this connection gets disconnected.
	/// </summary>
	public event Action? OnDisconnect;
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
	/// Sends a TzePacket to the client on the other end of this connection.
	/// </summary>
	/// <param name="packet">The TzePacket to send.</param>
	public void Send(TzePacket packet)
	{
		Task.Run(async () => await ClientSocket.SendAsync(packet.SerializedPacket, SocketFlags.None, cancellationSource.Token));
	}

	/// <summary>
	/// Sends an array of bytes to the client on the other end of this connection.
	/// </summary>
	/// <param name="buffer">The bytes to send.</param>
	public void Send(byte[] buffer)
	{
		Task.Run(async () => await ClientSocket.SendAsync(buffer, SocketFlags.None, cancellationSource.Token));
	}
	#endregion

	#region DisconnectAndDispose
	/// <summary>
	/// Disconnects the and disposes of the internal Socket object.
	/// </summary>
	public void DisconnectAndDispose()
	{
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
							OnDisconnect?.Invoke();
							DisconnectAndDispose();
						}
					}

					List<byte> bufferAsList = buffer.ToList();
					bufferAsList.RemoveRange(receivedBytes, bufferAsList.Count - receivedBytes);
					buffer = bufferAsList.ToArray();

					TzePacket? packet = TzePacket.FromSerializedPacket(buffer);
					OnReceive?.Invoke(packet ?? new TzePacket(TzePacket.TzePacketType.Message, buffer));
				}
			}
			catch (Exception ex)
			{
				if (ex is SocketException || ex is ObjectDisposedException)
				{
					if (!Disconnected)
					{
						OnDisconnect?.Invoke();
						DisconnectAndDispose();
					}
				}
				else throw;
			}
		}, cancellationSource.Token);
	}
	#endregion
}
