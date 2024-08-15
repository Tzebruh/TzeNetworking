using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TzeNetworking;

/// <summary>
/// The TzeNetworking abstraction of `System.Net.Sockets.TcpListener`.
/// </summary>
public class TzeTcpListener
{
	/// <summary>
	/// The actual TcpListener being used.
	/// </summary>
	public TcpListener Listener { get; }

	/// <summary>
	/// Whether the listener is currently listening.
	/// </summary>
	public bool Listening { get; private set; }

	private CancellationTokenSource cancellationSource = new();

	#region Event Definitions
	/// <summary>
	/// Called when the listener receives a connection.
	/// </summary>
	public event Action<TzeTcpConnection>? OnConnection;
	#endregion

	#region Constructors/Destructor
	/// <summary>
	/// Creates a new TzeTcpListener for listening on the provided port. You may want to use a different overload with IPAddress.Loopback or 127.0.0.1 for development as to not trigger the firewall.
	/// </summary>
	/// <param name="port">The port to listen on.</param>
	public TzeTcpListener(int port)
	{
		Listener = new(IPAddress.Any, port);
	}

	/// <summary>
	/// Creates a new TzeTcpListener for listening on the provided interface and port. You can use this to make sure you're broadcasting on IPv6, for example.
	/// </summary>
	/// <param name="host">The interface to listen on. (Excluding the port.) Normally this is `0.0.0.0`.</param>
	/// <param name="port">The port to listen on.</param>
	public TzeTcpListener(string host, int port)
	{
		Listener = new(
			new IPAddress(Encoding.UTF8.GetBytes(host)),
			port
		);
	}

	/// <summary>
	/// Creates a new TzeTcpListener for listening on the provided interface and port. You can use this to make sure you're broadcasting on IPv6, for example.
	/// </summary>
	/// <param name="host">The interface to listen on. Normally this is `IPAddress.Any`.</param>
	/// <param name="port">The port to listen on.</param>
	public TzeTcpListener(IPAddress host, int port)
	{
		Listener = new(host, port);
	}

	/// <summary>
	/// Creates a new TzeTcpListener for listening on the provided endpoint.
	/// </summary>
	/// <param name="endpoint">The endpoint to listen on.</param>
	public TzeTcpListener(IPEndPoint endpoint)
	{
		Listener = new(endpoint);
	}

	/// <summary>
	/// Stops and disposes of the listener when the object is removed.
	/// </summary>
	~TzeTcpListener() => StopAndDispose();
	#endregion

	#region Start/Stop/Dispose
	/// <summary>
	/// Starts listening.
	/// </summary>
	public void Start()
	{
		Listening = true;
		Listener.Start();
		StartAsyncTasks();
	} 	

	/// <summary>
	/// Stops listening. Call Start() to start listening again. Call StopAndDispose() to both stop and dispose of the listener.
	/// </summary>
	public void Stop()
	{
		Listening = false;
		cancellationSource.Cancel();
		Listener.Stop();
	}

	/// <summary>
	/// Disposes of the internal TcpListener object.
	/// </summary>
	public void Dispose()
	{
		cancellationSource.Cancel();
		#if NET8_0_OR_GREATER
		Listener.Dispose();
		#else
		Listener.Server.Dispose();
		#endif
		cancellationSource.Dispose();
	}

	/// <summary>
	/// Stops the listener and then disposes of the internal TcpListener object.
	/// </summary>
	public void StopAndDispose()
	{
		Stop();
		Dispose();
	}
	#endregion

	#region Internal Methods
	private void StartAsyncTasks()
	{
		Task.Run(async () => {
			while (true)
			{
				Socket newSocket = await Listener.AcceptSocketAsync();
				TzeTcpConnection connection = new(newSocket);
				OnConnection?.Invoke(connection);
			}
		}, cancellationSource.Token);
	}
	#endregion
}
