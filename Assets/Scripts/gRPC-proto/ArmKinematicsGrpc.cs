// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: arm_kinematics.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Reachy.Sdk.Kinematics {
  public static partial class ArmKinematics
  {
    static readonly string __ServiceName = "reachy.sdk.kinematics.ArmKinematics";

    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    static readonly grpc::Marshaller<global::Reachy.Sdk.Kinematics.ArmFKRequest> __Marshaller_reachy_sdk_kinematics_ArmFKRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Kinematics.ArmFKRequest.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Kinematics.ArmFKSolution> __Marshaller_reachy_sdk_kinematics_ArmFKSolution = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Kinematics.ArmFKSolution.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Kinematics.ArmIKRequest> __Marshaller_reachy_sdk_kinematics_ArmIKRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Kinematics.ArmIKRequest.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Kinematics.ArmIKSolution> __Marshaller_reachy_sdk_kinematics_ArmIKSolution = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Kinematics.ArmIKSolution.Parser));

    static readonly grpc::Method<global::Reachy.Sdk.Kinematics.ArmFKRequest, global::Reachy.Sdk.Kinematics.ArmFKSolution> __Method_ComputeArmFK = new grpc::Method<global::Reachy.Sdk.Kinematics.ArmFKRequest, global::Reachy.Sdk.Kinematics.ArmFKSolution>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ComputeArmFK",
        __Marshaller_reachy_sdk_kinematics_ArmFKRequest,
        __Marshaller_reachy_sdk_kinematics_ArmFKSolution);

    static readonly grpc::Method<global::Reachy.Sdk.Kinematics.ArmIKRequest, global::Reachy.Sdk.Kinematics.ArmIKSolution> __Method_ComputeArmIK = new grpc::Method<global::Reachy.Sdk.Kinematics.ArmIKRequest, global::Reachy.Sdk.Kinematics.ArmIKSolution>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ComputeArmIK",
        __Marshaller_reachy_sdk_kinematics_ArmIKRequest,
        __Marshaller_reachy_sdk_kinematics_ArmIKSolution);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Reachy.Sdk.Kinematics.ArmKinematicsReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of ArmKinematics</summary>
    [grpc::BindServiceMethod(typeof(ArmKinematics), "BindService")]
    public abstract partial class ArmKinematicsBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Kinematics.ArmFKSolution> ComputeArmFK(global::Reachy.Sdk.Kinematics.ArmFKRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Kinematics.ArmIKSolution> ComputeArmIK(global::Reachy.Sdk.Kinematics.ArmIKRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for ArmKinematics</summary>
    public partial class ArmKinematicsClient : grpc::ClientBase<ArmKinematicsClient>
    {
      /// <summary>Creates a new client for ArmKinematics</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public ArmKinematicsClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for ArmKinematics that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public ArmKinematicsClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected ArmKinematicsClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected ArmKinematicsClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Reachy.Sdk.Kinematics.ArmFKSolution ComputeArmFK(global::Reachy.Sdk.Kinematics.ArmFKRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ComputeArmFK(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Kinematics.ArmFKSolution ComputeArmFK(global::Reachy.Sdk.Kinematics.ArmFKRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ComputeArmFK, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Kinematics.ArmFKSolution> ComputeArmFKAsync(global::Reachy.Sdk.Kinematics.ArmFKRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ComputeArmFKAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Kinematics.ArmFKSolution> ComputeArmFKAsync(global::Reachy.Sdk.Kinematics.ArmFKRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ComputeArmFK, null, options, request);
      }
      public virtual global::Reachy.Sdk.Kinematics.ArmIKSolution ComputeArmIK(global::Reachy.Sdk.Kinematics.ArmIKRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ComputeArmIK(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Kinematics.ArmIKSolution ComputeArmIK(global::Reachy.Sdk.Kinematics.ArmIKRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ComputeArmIK, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Kinematics.ArmIKSolution> ComputeArmIKAsync(global::Reachy.Sdk.Kinematics.ArmIKRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ComputeArmIKAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Kinematics.ArmIKSolution> ComputeArmIKAsync(global::Reachy.Sdk.Kinematics.ArmIKRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ComputeArmIK, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override ArmKinematicsClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new ArmKinematicsClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(ArmKinematicsBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_ComputeArmFK, serviceImpl.ComputeArmFK)
          .AddMethod(__Method_ComputeArmIK, serviceImpl.ComputeArmIK).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, ArmKinematicsBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_ComputeArmFK, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Kinematics.ArmFKRequest, global::Reachy.Sdk.Kinematics.ArmFKSolution>(serviceImpl.ComputeArmFK));
      serviceBinder.AddMethod(__Method_ComputeArmIK, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Kinematics.ArmIKRequest, global::Reachy.Sdk.Kinematics.ArmIKSolution>(serviceImpl.ComputeArmIK));
    }

  }
}
#endregion
