using System.Text.Json.Serialization;

namespace myNOC.Remootio
{
	internal enum FrameTypes
	{
		AUTH,
		HELLO,
		PING,
		ENCRYPTED,
		ERROR,
		PONG,
		SERVER_HELLO
	}

	internal enum PayloadTypes
	{
		CHALLENGE,
		RESPONSE
	}

	[JsonConverter(typeof(JsonStringEnumConverter))]
	internal enum ActionTypes
	{
		TRIGGER,
		TRIGGER_SECONDARY,
		OPEN,
		CLOSE,
		QUERY,
		RESTART
	}

	[JsonConverter(typeof(JsonStringEnumConverter))]
	internal enum SensorStates
	{
		Closed,
		Open,
		NoSensor
	}

	internal enum KeyTypes
	{
		Master,
		Unique,
		Guest,
		API,
		SmartHome,
		Automation
	}

	internal enum ConnectionTypes
	{
		Unknown,
		None,
		Bluetooth,
		WiFi,
		Internet,
		AutoOpen
	}

	internal enum EventTypes
	{
		StateChange,
		Restart,
		ManualButtonPushed,
		ManualButtonEnabled,
		ManualButtonDisabled,
		DoorbellPushed,
		DoorbellEnabled,
		DoorbellDisabled,
		SensorEnabled,
		SensorFlipped,
		SensorDisabled,
		RelayTrigger,
		SecondaryRelayTrigger,
		Connected,
		LeftOpen,
		KeyManagement
	}
}
