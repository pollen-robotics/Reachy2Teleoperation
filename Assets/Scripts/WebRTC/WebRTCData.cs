using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Bridge;
using Reachy;

public class WebRTCData : WebRTCBase
{
    private RTCDataChannel _serviceChannel = null;
    private RTCDataChannel _reachyStateChannel = null;
    private RTCDataChannel _reachyCommandChannel = null;

    private Bridge.AnyCommands _commands;
    private Bridge.ConnectionStatus _connectionStatus = null;

    private ReachyState _reachyState = null;

    private TeleopReachy.DataMessageManager dataMessageManager;

    public UnityEvent<bool> event_DataControllerStatusHasChanged;
    private bool isRobotInRoom = false;

    protected override void Start()
    {
        base.Start();
        dataMessageManager = TeleopReachy.DataMessageManager.Instance;
        _commands = new Bridge.AnyCommands
        {
            Commands = {
                new Bridge.AnyCommand
                {
                    HandCommand = new Bridge.HandCommand{
                        HandGoal = new Reachy.Part.Hand.HandPositionRequest{
                            Id = new Reachy.Part.PartId
                            {
                                Id = 5
                            },
                            Position = new Reachy.Part.Hand.HandPosition{
                                ParallelGripper = new Reachy.Part.Hand.ParallelGripperPosition{
                                    Position = 50.0f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void WebRTCCall()
    {
        base.WebRTCCall();

        if (_pc != null)
        {
            _pc.OnDataChannel = channel =>
            {
                Debug.Log($"Receiving new channel: {channel.Label}");
                if (channel.Label == "service")
                {
                    SetupConnection(channel);
                }
                else if (channel.Label.StartsWith("reachy_state"))
                {
                    SetupStateChannel(channel);
                }
                else if (channel.Label.StartsWith("reachy_command"))
                {
                    SetupCommandChannel(channel);
                }
                else
                {
                    Debug.LogWarning($"Channel {channel.Label} is unknown");
                }
            };
        }
    }

    void SetupStateChannel(RTCDataChannel channel)
    {
        _reachyStateChannel = channel;
        _reachyStateChannel.OnMessage = OnDataChannelStateMessage;
    }

    void OnDataChannelStateMessage(byte[] data)
    {
        _reachyState = ReachyState.Parser.ParseFrom(data);

        dataMessageManager.StreamReachyState(_reachyState);
    }

    void SetupCommandChannel(RTCDataChannel channel)
    {
        _reachyCommandChannel = channel;
    }

    void SetupConnection(RTCDataChannel channel)
    {
        _serviceChannel = channel;
        _serviceChannel.OnMessage = OnDataChannelServiceMessage;
        var req = new ServiceRequest
        {
            GetReachy = new GetReachy()
        };
        _serviceChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(req));
    }

    void OnDataChannelServiceMessage(byte[] data)
    {
        ServiceResponse response = ServiceResponse.Parser.ParseFrom(data);
        Debug.LogWarning(response);

        if (response.ConnectionStatus != null)
        {
            _connectionStatus = response.ConnectionStatus;
            Debug.LogError(_connectionStatus.ToString());

            if (response.ConnectionStatus.Connected)
            {
                dataMessageManager.GetReachyId(response.ConnectionStatus.Reachy);
                isRobotInRoom = true;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }

            var req = new ServiceRequest
            {
                Connect = new Connect
                {
                    ReachyId = _connectionStatus.Reachy.Id,
                    UpdateFrequency = 50 //FixedUpdate refresh rate is 0.02 sec
                }
            };
            _serviceChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(req));
        }

        if (response.Error != null && !string.IsNullOrEmpty(response.Error.ToString()))
        {
            Debug.LogError($"Received error message: {response.Error.ToString()}");
        }

    }

    public void SendCommandMessage(Bridge.AnyCommands _commands)
    {
        _reachyCommandChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(_commands));
    }
}

