// Generated by SpatialOS codegen. DO NOT EDIT!
// source: schema.ragan.health in improbable/player/name.proto.

namespace Ragan
{

public static class Health_Extensions
{
  public static Health.Data Get(this global::Improbable.Worker.IComponentData<Health> data)
  {
    return (Health.Data) data;
  }

  public static Health.Update Get(this global::Improbable.Worker.IComponentUpdate<Health> update)
  {
    return (Health.Update) update;
  }
}

public partial class Health : global::Improbable.Worker.IComponentMetaclass
{
  public uint ComponentId
  {
    get { return 1234; }
  }

  // Concrete data type for the Health state.
  public class Data : global::Improbable.Worker.IComponentData<Health>
  {
    public global::Ragan.HealthData Value;

    public Data(global::Ragan.HealthData value)
    {
      Value = value;
    }

    public Data DeepCopy()
    {
      return new Data(Value.DeepCopy());
    }
  }

  // Concrete update type for the Health state.
  public class Update : global::Improbable.Worker.IComponentUpdate<Health>
  {
    /// <summary>
    /// Field current_health = 1.
    /// </summary>
    public global::Improbable.Collections.Option<uint> currentHealth;
    public Update SetCurrentHealth(uint _value)
    {
      currentHealth.Set(_value);
      return this;
    }

    /// <summary>
    /// Field max_health = 2.
    /// </summary>
    public global::Improbable.Collections.Option<uint> maxHealth;
    public Update SetMaxHealth(uint _value)
    {
      maxHealth.Set(_value);
      return this;
    }

    public Update DeepCopy()
    {
      var _result = new Update();
      if (currentHealth.HasValue)
      {
        uint field;
        field = currentHealth.Value;
        _result.currentHealth.Set(field);
      }
      if (maxHealth.HasValue)
      {
        uint field;
        field = maxHealth.Value;
        _result.maxHealth.Set(field);
      }
      return _result;
    }

    public global::Improbable.Worker.IComponentData<Health> ToInitialData()
    {
      return new Data(new global::Ragan.HealthData(
          currentHealth.Value,
          maxHealth.Value));
    }

    public void ApplyTo(global::Improbable.Worker.IComponentData<Health> _data)
    {
      var _concrete = _data.Get();
      if (currentHealth.HasValue)
      {
        _concrete.Value.currentHealth = currentHealth.Value;
      }
      if (maxHealth.HasValue)
      {
        _concrete.Value.maxHealth = maxHealth.Value;
      }
    }
  }

  // Implementation details below here.
  //----------------------------------------------------------------

  public global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentVtable Vtable {
    get {
      global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentVtable vtable;
      vtable.ComponentId = ComponentId;
      vtable.Free = global::System.Runtime.InteropServices.Marshal
          .GetFunctionPointerForDelegate(global::Improbable.Worker.Internal.ClientComponents.ClientComponentFree);
      vtable.BufferFree = global::System.Runtime.InteropServices.Marshal
          .GetFunctionPointerForDelegate(global::Improbable.Worker.Internal.ClientComponents.ClientComponentBufferFree);
      vtable.Copy = global::System.Runtime.InteropServices.Marshal
          .GetFunctionPointerForDelegate(clientComponentCopy);
      vtable.Deserialize = global::System.Runtime.InteropServices.Marshal
          .GetFunctionPointerForDelegate(clientComponentDeserialize);
      vtable.Serialize = global::System.Runtime.InteropServices.Marshal
          .GetFunctionPointerForDelegate(clientComponentSerialize);
      return vtable;
    }
  }

  public void ExtractInitialData(global::Improbable.Worker.Entity entity,
                                 global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate update)
  {
    var dereferenced = global::Improbable.Worker.Internal.ClientComponents.Instance
        .Dereference<Health>(update.Reference);
    entity.Add<Health>(dereferenced.ToInitialData());
  }

  private static unsafe readonly global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentCopy
      clientComponentCopy = ClientComponentCopy;
  private static unsafe readonly global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentDeserialize
      clientComponentDeserialize = ClientComponentDeserialize;
  private static unsafe readonly global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentSerialize
      clientComponentSerialize = ClientComponentSerialize;

  private static unsafe global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate*
  ClientComponentCopy(global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate* update)
  {
    global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate* copy = null;
    try
    {
      var dereferenced = global::Improbable.Worker.Internal.ClientComponents.Instance
          .Dereference<Health>(update->Reference);
      copy = global::Improbable.Worker.Internal.ClientComponents.UpdateAlloc();
      copy->Reference = global::Improbable.Worker.Internal.ClientComponents.Instance
          .CreateReference(dereferenced);
    }
    catch (global::System.Exception e)
    {
      global::Improbable.Worker.ClientError.LogClientException(e);
    }
    return copy;
  }

  private static unsafe bool
  ClientComponentDeserialize(global::System.Byte* buffer,
                             System.UInt32 length,
                             global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate** update)
  {
    *update = null;
    try
    {
      var data = new Update();
      *update = global::Improbable.Worker.Internal.ClientComponents.UpdateAlloc();
      (*update)->Reference = global::Improbable.Worker.Internal.ClientComponents.Instance
          .CreateReference<Health>(data);
      var stream = new global::System.IO.UnmanagedMemoryStream(buffer, (long) length);
      var message = global::ProtoBuf.Serializer
          .Deserialize<global::Improbable.Protocol.ComponentUpdate>(stream);
      var _proto = global::ProtoBuf.Extensible.GetValue<global::Schema.Ragan.HealthData>(
          message.ComponentData, (int) 1234);
      if (_proto.CurrentHealthSpecified)
      {
        uint field;
        field = _proto.CurrentHealth;
        data.currentHealth.Set(field);
      }
      if (_proto.MaxHealthSpecified)
      {
        uint field;
        field = _proto.MaxHealth;
        data.maxHealth.Set(field);
      }
    }
    catch (global::System.Exception e)
    {
      global::Improbable.Worker.ClientError.LogClientException(e);
      return false;
    }
    return true;
  }

  private static unsafe void
  ClientComponentSerialize(global::Improbable.Worker.Internal.ComponentProtocol.ClientComponentUpdate* update,
                           global::System.Int64 entityId,
                           global::System.Byte** buffer,
                           global::System.UInt32* length)
  {
    *buffer = null;
    try
    {
      var message = new global::Improbable.Protocol.FromEngineMsg();
      message.ComponentUpdate = new global::Improbable.Protocol.ComponentUpdate();
      message.ComponentUpdate.EntityId = entityId;
      message.ComponentUpdate.ComponentId = 1234;
      message.ComponentUpdate.ComponentData = new global::Schema.Improbable.EntityState();
      var _proto = new global::Schema.Ragan.HealthData();
      var data = global::Improbable.Worker.Internal.ClientComponents.Instance
          .Dereference<Health>(update->Reference).Get();
      if (data.currentHealth.HasValue)
      {
        _proto.CurrentHealth = data.currentHealth.Value;
      }
      if (data.maxHealth.HasValue)
      {
        _proto.MaxHealth = data.maxHealth.Value;
      }
      global::ProtoBuf.Extensible.AppendValue(
          message.ComponentUpdate.ComponentData, 1234, _proto);
      using (var stream = new global::Improbable.Worker.Internal.ExpandableUnmanagedMemoryStream())
      {
        global::ProtoBuf.Serializer.Serialize(stream, message);
        *buffer = stream.TakeOwnershipOfBuffer();
        *length = (uint) stream.Length;
      }
    }
    catch (global::System.Exception e)
    {
      global::Improbable.Worker.ClientError.LogClientException(e);
    }
  }
}

}
