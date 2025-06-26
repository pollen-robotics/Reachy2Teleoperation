using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Grpc.Core;
using System.Threading.Tasks;

using Data.Acquisition;


namespace DataAcquisition
{
    public class gRPCDataController : gRPCBase
    {
        RecordingSessionParameters recordingParams;
        DataAcquisitionService.DataAcquisitionServiceClient client;

        void Start()
        {
            recordingParams = DataAcquisitionManager.Instance.RecordingSessionParameters;
            // PlayerPrefs.SetString("robot_ip", "192.168.1.10");
            PlayerPrefs.SetString("server_data_port", "50062");

            InitChannel("server_data_port");
            if (channel != null)
            {
                client = new DataAcquisitionService.DataAcquisitionServiceClient(channel);
            }
        }

        public async Task<ActionAck> StartSession()
        {
            try
            {
                SessionParams sessionParams = new SessionParams {
                    SessionName="test_session_from_unity",
                    TaskDescription=recordingParams.TaskDescription,
                    DatasetName=recordingParams.DatasetName,
                    WarmupTimeDuration=3,
                    NbEpisodesGoal=recordingParams.NbEpisodesGoal,
                    BreakTimeDuration=recordingParams.BreakTimeDuration,
                    EpisodeDuration=recordingParams.EpisodeDuration,
                    Resume=!recordingParams.IsNewDataset,
                    Offline=!recordingParams.IsDatasetOnline,
                };
                return await client.StartSessionAsync(sessionParams);
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in StartSession():" + e);
                rpcException = "Error in StartSession():\n" + e.ToString();
                return new ActionAck { SuccessAck=false };
            }
        }

        public async Task<DatasetList> GetDatasetList()
        {
            try
            {
                DatasetList datasetList = await client.GetDatasetListAsync(new Google.Protobuf.WellKnownTypes.Empty());
                return datasetList;
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in StopSession():" + e);
                rpcException = "Error in StopSession():\n" + e.ToString();
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
                return await client.AddDatasetAsync(dataset);
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
                };
                ActionAck ack = await client.AddDatasetAsync(dataset);
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
    }
}