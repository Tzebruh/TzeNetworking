using System.Text.Json;
using System.Text.Json.Serialization;

namespace TzeNetworking;

/// <summary>
/// A packet of data that can be decoded to a byte array.
/// </summary>
public struct TzePacket
{
	/// <summary>
	/// The type of this TzePacket.
	/// </summary>
	public TzePacketType PacketType { get; set; }

	/// <summary>
	/// The data of this TzePacket.
	/// </summary>
	public string? Data { get; set; }

	/// <summary>
	/// Gets the serialized data of this TzePacket. This is a JSON byte array that represents the PacketType and Data properties.
	/// See also: TzePacket.FromSerializedData(byte[])
	/// </summary>
	[JsonIgnore] public readonly byte[] SerializedPacket => JsonSerializer.SerializeToUtf8Bytes(this);

	#region Constructors
	/// <summary>
	/// Creates a new TzePacket with the provided type and data.
	/// </summary>
	/// <param name="packetType">The type of the TzePacket.</param>
	/// <param name="data">The data of the TzePacket as a string. May be null.</param>
	public TzePacket(TzePacketType packetType, string? data)
	{
		PacketType = packetType;
		Data = data;
	}

	/// <summary>
	/// Creates a new TzePacket with the provided data.
	/// NOTE: This serializes as JSON, which doesn't work with all objects! If you must, you can use your own serialization algorithm and use the string overload of this constructor.
	/// </summary>
	/// <param name="packetType">The type of the TzePacket.</param>
	/// <param name="data">The data of the TzePacket as an object.</param>
	public TzePacket(TzePacketType packetType, object data)
	{
		PacketType = packetType;
		Data = JsonSerializer.Serialize(data);
	}
	#endregion Constructors

	#region Deserialization Methods
	/// <summary>
	/// Deserializes this TzePacket's Data property from JSON into an object.
	/// </summary>
	/// <param name="type">The type of the object to deserialize to.</param>
	public object? GetDataAsObject(Type type)
	{
		if (Data == null) return null;
		return JsonSerializer.Deserialize(Data, type);
	}
	#endregion

	#region Static Methods
	/// <summary>
	/// Creates a Disconnect TzePacket with an empty byte array as the Data.
	/// </summary>
	public static TzePacket Disconnect() => new TzePacket(TzePacketType.Disconnect, Array.Empty<byte>());

	/// <summary>
	/// Creates a TzePacket from a a JSON byte array. Returns null if the JSON can't be converted.
	/// </summary>
	/// <param name="serializedPacket">The serialized data to create the TzePacket from. (JSON form)</param>
	public static TzePacket? FromSerializedPacket(byte[] serializedPacket)
	{
		try
		{
			return JsonSerializer.Deserialize<TzePacket>(serializedPacket);
		}
		catch (Exception)
		{
			return null;
		}
	}
	#endregion
	
	/// <summary>
	/// An enum of all TzePacket types.
	/// </summary>
	public enum TzePacketType
	{
		/// <summary>
		/// A regular message packet. You'll be using this 90% of the time.
		/// </summary>
		Message = 0,
		/// <summary>
		/// A disconnect packet which tells the server you have disconnected. This is automatically sent for you if you disconnect with the helper method.
		/// </summary>
		Disconnect = 1
	}
}
