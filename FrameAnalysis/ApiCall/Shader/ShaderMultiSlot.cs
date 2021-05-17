using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public interface IShaderCall
    {
        ShaderType ShaderType { get; }
    }

    public abstract class ShaderMultiSlot<This, TSlot> : MultiSlotBase<This, TSlot, IApiCall, DrawCall, ShaderContext>, IApiCall, IShaderCall
        where This : MultiSlotBase<This, TSlot, IApiCall, DrawCall, ShaderContext>, IApiCall
        where TSlot : class, ISlot<IApiCall>, IOwned<IApiCall>
    {
        public static Dictionary<ShaderType, List<int>> UsedSlots { get; } = new Dictionary<ShaderType, List<int>>();

        protected ShaderMultiSlot(uint order) => Order = order;

        public uint Order { get; }

        public ShaderType ShaderType { get; set; }

        string INamed.Name => $"{ShaderType.Letter()}S{GetType().Name}";

        public override List<int> GlobalSlotsMask => UsedSlots.GetOrAdd(ShaderType);

        protected override Deferred<ShaderContext, DrawCall>? PreviousDeferred => Owner?.Fallback?.Shader(ShaderType).Deferred;
    }
}
