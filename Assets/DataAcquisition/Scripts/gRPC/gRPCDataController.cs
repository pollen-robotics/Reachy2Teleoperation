using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Grpc.Core;
using System.Threading.Tasks;

using Data.Acquisition;
using Data.Acquisition.Robot;
using Data.Acquisition.Teleoperator;


namespace DataAcquisition
{
    public class gRPCDataController : gRPCBase
    {
        RecordingSessionParameters recordingParams;
        DataAcquisitionService.DataAcquisitionServiceClient client;

        public UnityEvent event_DataAcquisitionServerConnectionFailed;

        void Start()
        {
            recordingParams = DataAcquisitionManager.Instance.RecordingSessionParameters;

            InitCustomChannel(DataAcquisitionServer.Instance.GetServerIpAddress(), DataAcquisitionServer.Instance.GetServerDataPort());

            if (channel != null)
            {
                client = new DataAcquisitionService.DataAcquisitionServiceClient(channel);
            }
        }

        public async Task<ActionAck> StartSession()
        {
            try
            {
                Robot reachy2 = new Robot
                {
                    RobotType = RobotType.Reachy2,
                    RobotId = "reachy2-pvt04",
                    Reachy2 = new Reachy2
                    {
                        IpAddress = PlayerPrefs.GetString("robot_ip"),
                        UseExternalCommands = true,
                        WithMobileBase = recordingParams.IsMobileBaseRecorded,
                        WithLArm = recordingParams.IsLArmRecorded,
                        WithRArm = recordingParams.IsRArmRecorded,
                        WithNeck = recordingParams.IsNeckRecorded,
                        WithAntennas = recordingParams.AreAntennasRecorded,
                        WithLeftTeleopCamera = recordingParams.IsLTeleopCamRecorded,
                        WithRightTeleopCamera = recordingParams.IsRTeleopCamRecorded,
                        WithTorsoCamera = recordingParams.IsTorsoCamRecorded,
                        DisableTorqueOnDisconnect = false,
                    }
                };

                Teleoperator reachy2_teleop = new Teleoperator {
                    TeleoperatorType = TeleoperatorType.Reachy2,
                    Reachy2 = new Reachy2Teleoperator
                    {
                        IpAddress = PlayerPrefs.GetString("robot_ip"),
                        UsePresentPosition = false,
                        WithMobileBase = recordingParams.IsMobileBaseRecorded,
                        WithLArm = recordingParams.IsLArmRecorded,
                        WithRArm = recordingParams.IsRArmRecorded,
                        WithNeck = recordingParams.IsNeckRecorded,
                        WithAntennas = recordingParams.AreAntennasRecorded,
                    }
                };

                SessionParams sessionParams = new SessionParams
                {
                    Robot = reachy2,
                    Teleoperator = reachy2_teleop,
                    RepoId = recordingParams.DatasetName,
                    // RepoId = "glannuzel/test_torso_cam",
                    // TaskDescription = recordingParams.TaskDescription,
                    TaskDescription = "White box in black container",
                    NbEpisodesGoal = recordingParams.NbEpisodesGoal,
                    EpisodeTimeS = recordingParams.EpisodeDuration,
                    ResetTimeS = recordingParams.BreakTimeDuration,
                    // Fps = recordingParams.RecordFrequency,
                    Fps = 10,
                    // UseVideos = recordingParams.UseVideos,
                    Resume=!recordingParams.IsNewDataset,
                    // Offline=!recordingParams.IsDatasetOnline,
                };
                var test = await client.StartSessionAsync(sessionParams);
                Debug.LogError(sessionParams);
                return test;
            }
            catch (RpcException e)
            {
                Debug.LogError("Communication RPC failed: in StartSession():" + e);
                rpcException = "Error in StartSession():\n" + e.ToString();
                event_DataAcquisitionServerConnectionFailed.Invoke();
                return new ActionAck { SuccessAck=false };
            }
        }

        public async Task<DatasetList> GetDatasetList()
        {
            Debug.LogError("[gRPCDataController] GetDatasetList");
            try
            {
                DatasetList datasetList = await client.GetDatasetListAsync(new DatasetRoot {});
                Debug.LogError("datasetList: " + datasetList);
                return datasetList;
            }
            catch (RpcException e)
            {
                Debug.LogError("Communication RPC failed: in GetDatasetList():" + e);
                rpcException = "Error in GetDatasetList():\n" + e.ToString();
                event_DataAcquisitionServerConnectionFailed.Invoke();
                return null;
            }
        }

        public async void AddDataset()
        {
            await AddDataset(recordingParams.DatasetName);
        }

        public async Task<ActionAck> AddDataset(string datasetName)
        {
            try
            {
                Dataset dataset = new Dataset {
                    DatasetName = datasetName,
                    Pushed = DatasetPushState.LocalOnly,
                };
                return await client.AddDatasetToListAsync(dataset);
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in AddDataset():" + e);
                rpcException = "Error in AddDataset():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public void UpdateDataset(DatasetPushState state)
        {
            Task.Run(() => UpdateDataset(recordingParams.DatasetName, state));
        }

        public async Task<ActionAck> UpdateDataset(string datasetName, DatasetPushState state)
        {
            try
            {
                Dataset dataset = new Dataset {
                    DatasetName = datasetName,
                    Pushed = state,
                    NbEpisodes = DataAcquisitionManager.Instance.RecordingSessionManager.GetCurrentEpisode(),
                };
                ActionAck ack = await client.UpdateDatasetInListAsync(dataset);
                return ack;
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in AddDataset():" + e);
                rpcException = "Error in AddDataset():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<ActionAck> StopSession()
        {
            try
            {
                return await client.StopSessionAsync(new Google.Protobuf.WellKnownTypes.Empty());
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in StopSession():" + e);
                rpcException = "Error in StopSession():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<ActionAck> StartEpisode()
        {
            try
            {
                return await client.StartEpisodeAsync(new Google.Protobuf.WellKnownTypes.Empty());
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in StartEpisode():" + e);
                rpcException = "Error in StartEpisode():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<ActionAck> StopEpisode()
        {
            try
            {
                return await client.StopEpisodeAsync(new Google.Protobuf.WellKnownTypes.Empty());
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in StopEpisode():" + e);
                rpcException = "Error in StopEpisode():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<ActionAck> SaveEpisode()
        {
            try
            {
                return await client.SaveEpisodeAsync(new EpisodeRating { Success=true });
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in SaveEpisode():" + e);
                rpcException = "Error in SaveEpisode():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<ActionAck> PushDataFromSession()
        {
            try
            {
                ActionAck ack = await client.UploadSessionAsync(new Google.Protobuf.WellKnownTypes.Empty());
                return ack;
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in PushDataFromSession():" + e);
                rpcException = "Error in PushDataFromSession():\n" + e.ToString();
                return new ActionAck { SuccessAck = false };
            }
        }

        public async Task<Data.Acquisition.Error> AuditSession()
        {
            try
            {
                Data.Acquisition.Error error = await client.AuditSessionAsync(new Google.Protobuf.WellKnownTypes.Empty());
                return error;
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in AuditSession():" + e);
                rpcException = "Error in AuditSession():\n" + e.ToString();
                return new Data.Acquisition.Error {};
            }
        }
    }
}