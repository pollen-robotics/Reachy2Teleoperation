using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Grpc.Core;

using Ah.Teleoperation;

public class gRPCBase : MonoBehaviour
{
    protected Channel channel = null;
    AHTeleoperationService.AHTeleoperationServiceClient client;
    private CancellationTokenSource _cts;

    private bool needUpdateCommand;


    [SerializeField] private HandJoints r_handJoints;
    [SerializeField] private HandJoints l_handJoints;

    void Start()
    {
        needUpdateCommand = true;

        string ip_address = "192.168.0.199";
        string port = "50065";
        string address = ip_address + ":" + port;
        channel = new Channel(address, ChannelCredentials.Insecure);
        _cts = new CancellationTokenSource();
      
        if (channel != null)
        {
            client = new AHTeleoperationService.AHTeleoperationServiceClient(channel);
        }
    }

    public async void SendHandCommands(HandTeleoperationCommand commands)
    {
        try
        {
            if (needUpdateCommand)
            {
                needUpdateCommand = false;
                var options = new CallOptions(
                    deadline: DateTime.UtcNow.AddSeconds(10),
                    cancellationToken: _cts.Token
                );
                await client.SendHandCommandAsync(commands, options);
                needUpdateCommand = true;
            }
        }
        catch (RpcException e)
        {
            Debug.LogError("Communication RPC failed: in SendHandCommands():" + e);
        }
    }

    private async Task StopGrpcAsync()
    {
        if (_cts == null) return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;

        if (channel != null)
        {
            await channel.ShutdownAsync();
            channel = null;
        }
    }

    void Update ()
    {
        if (r_handJoints.IsInitialized && l_handJoints.IsInitialized)
        {
            SendHandCommands(
                new HandTeleoperationCommand { 
                    RightHand = new HandConfiguration {
                        Thumb = new FingerTipPosition { 
                            X = -r_handJoints.thumbInWrist.y,
                            Y = -r_handJoints.thumbInWrist.x,
                            Z = r_handJoints.thumbInWrist.z
                        },
                        Index = new FingerTipPosition {
                            X = -r_handJoints.indexInWrist.y,
                            Y = -r_handJoints.indexInWrist.x,
                            Z = r_handJoints.indexInWrist.z
                        },
                        Middle = new FingerTipPosition {
                            X = -r_handJoints.middleInWrist.y,
                            Y = -r_handJoints.middleInWrist.x,
                            Z = r_handJoints.middleInWrist.z
                        },
                        Ring = new FingerTipPosition {
                            X = -r_handJoints.ringInWrist.y,
                            Y = -r_handJoints.ringInWrist.x,
                            Z = r_handJoints.ringInWrist.z
                        },
                        IndexPinch = new PinchData {
                            PinchValue = r_handJoints.indexPinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -r_handJoints.indexToThumbDistance.y,
                                Y = -r_handJoints.indexToThumbDistance.x,
                                Z = r_handJoints.indexToThumbDistance.z
                            }
                        },
                        MiddlePinch = new PinchData {
                            PinchValue = r_handJoints.middlePinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -r_handJoints.middleToThumbDistance.y,
                                Y = -r_handJoints.middleToThumbDistance.x,
                                Z = r_handJoints.middleToThumbDistance.z
                            }
                        },
                        RingPinch = new PinchData {
                            PinchValue = r_handJoints.ringPinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -r_handJoints.ringToThumbDistance.y,
                                Y = -r_handJoints.ringToThumbDistance.x,
                                Z = r_handJoints.ringToThumbDistance.z
                            }
                        }
                    },
                    LeftHand = new HandConfiguration {
                        Thumb = new FingerTipPosition { 
                            X = -l_handJoints.thumbInWrist.y,
                            Y = -l_handJoints.thumbInWrist.x,
                            Z = l_handJoints.thumbInWrist.z
                        },
                        Index = new FingerTipPosition {
                            X = -l_handJoints.indexInWrist.y,
                            Y = -l_handJoints.indexInWrist.x,
                            Z = l_handJoints.indexInWrist.z
                        },
                        Middle = new FingerTipPosition {
                            X = -l_handJoints.middleInWrist.y,
                            Y = -l_handJoints.middleInWrist.x,
                            Z = l_handJoints.middleInWrist.z
                        },
                        Ring = new FingerTipPosition {
                            X = -l_handJoints.ringInWrist.y,
                            Y = -l_handJoints.ringInWrist.x,
                            Z = l_handJoints.ringInWrist.z
                        },
                        IndexPinch = new PinchData {
                            PinchValue = l_handJoints.indexPinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -l_handJoints.indexToThumbDistance.y,
                                Y = -l_handJoints.indexToThumbDistance.x,
                                Z = l_handJoints.indexToThumbDistance.z
                            }
                        },
                        MiddlePinch = new PinchData {
                            PinchValue = l_handJoints.middlePinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -l_handJoints.middleToThumbDistance.y,
                                Y = -l_handJoints.middleToThumbDistance.x,
                                Z = l_handJoints.middleToThumbDistance.z
                            }
                        },
                        RingPinch = new PinchData {
                            PinchValue = l_handJoints.ringPinch,
                            DistanceToThumb = new FingerTipPosition {
                                X = -l_handJoints.ringToThumbDistance.y,
                                Y = -l_handJoints.ringToThumbDistance.x,
                                Z = l_handJoints.ringToThumbDistance.z
                            }
                        }
                    }
                }
            );
        }
    }

    async void OnApplicationQuit() => await StopGrpcAsync();
    async void OnDestroy() => await StopGrpcAsync();
}