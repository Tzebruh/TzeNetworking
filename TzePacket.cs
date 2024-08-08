using System.Text;
using System.Text.Json;

namespace TzeNetworking;

/// <summary>
/// A packet of data that can be decoded to a byte array.
/// </summary>
public struct TzePacket
{
	/// <summary>
	/// The data of this TzePacket.
	/// </summary>
	public byte[]? Data { get; } = null;

	#region Constructors
	/// <summary>
	/// Creates a new TzePacket with the provided data.
	/// </summary>
	/// <param name="data">The data of the TzePacket as a byte array.</param>
	public TzePacket(byte[] data)
	{
		Data = data;
	}

	/// <summary>
	/// Creates a new TzePacket with the provided data.
	/// </summary>
	/// <param name="data">The data of the TzePacket as a string.</param>
	public TzePacket(string data)
	{
		Data = Encoding.UTF8.GetBytes(data);
	}

	/// <summary>
	/// Creates a new TzePacket with the provided data.
	/// NOTE: This overload serializes the object into JSON, which may be considered inefficient. If you need speed, you should use a different algorithm to encode the object into a byte array.
	/// </summary>
	/// <param name="data">The data of the TzePacket as an object.</param>
	public TzePacket(object data)
	{
		Data = JsonSerializer.SerializeToUtf8Bytes(data);
	}
	#endregion Constructors

	#region Decoding Methods
	/// <summary>
	/// Decodes this TzePacket into an object.
	/// </summary>
	/// <param name="type">The type of the object to decode to.</param>
	public object? DecodeObject(Type type)
	{
		return JsonSerializer.Deserialize(Data, type);
	}

	/// <summary>
	/// Decodes this TzePacket into a string.
	/// </summary>
	public string? DecodeString()
	{
		return Data == null ? null : Encoding.UTF8.GetString(Data);
	}
	#endregion
}
