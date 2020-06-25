using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public interface IShaderCall
    {
        ShaderType ShaderType { get; }
    }

    public abstract class ShaderMultiSlot<This, TSlot> : MultiSlotBase<This, TSlot, ShaderContext>, IShaderCall
        where This : MultiSlotBase<This, TSlot, ShaderContext>
        where TSlot : Slot
    {
        public static Dictionary<ShaderType, List<int>> UsedSlots { get; } = new Dictionary<ShaderType, List<int>>();

        protected ShaderMultiSlot(uint order) : base(order) { }

        public override List<int> GlobalSlotsMask => UsedSlots.GetOrAdd(ShaderType);

        protected override Deferred<ShaderContext, DrawCall> PreviousDeferred => Owner.Previous?.Shader(ShaderType).Deferred;

        public override string Name => $"{ShaderType.ToString()[0]}S{base.Name}";

        public ShaderType ShaderType { get; set; }
    }
}
