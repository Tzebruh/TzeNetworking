namespace TzeNetworking.Interfaces;

/// <summary>
/// Internally used interface for serializing a TzePacket to JSON. (Prevents a stack overflow that was caused by trying to serialize the property that itself serializes the packet)
/// </summary>
public interface ITzePacket
{
	/// <summary>
	/// The type of this TzePacket.
	/// </summary>
	public TzePacket.TzePacketType PacketType { get; }

	/// <summary>
	/// The data of this TzePacket.
	/// </summary>
	public string Data { get; }
}
