using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using System;
using System.Collections;
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

    bool _streamCommands = false;

    protected override void Start()
    {
        base.Start();
        _streamCommands = false;
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
        Debug.Log(_reachyState.ToString());
    }

    void SetupCommandChannel(RTCDataChannel channel)
    {
        _reachyCommandChannel = channel;
        _streamCommands = true;
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
            Debug.Log(_connectionStatus.ToString());

            //For testing purposes
            _commands.Commands[0].HandCommand.HandGoal.Id = _connectionStatus.Reachy.RHand.PartId;

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

    void FixedUpdate()
    {
        if (_streamCommands)
        {
            //Get controller position and send it
            float target = 0.5f - 0.5f * Mathf.Sin(2 * Mathf.PI * 1 * Time.time);
            _commands.Commands[0].HandCommand.HandGoal.Position.ParallelGripper.Position = target;
            _reachyCommandChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(_commands));
            Debug.Log("Send: " + _commands.ToString());
        }
    }

}

