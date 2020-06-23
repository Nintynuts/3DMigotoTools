﻿using System.Collections.Generic;

namespace Migoto.Log.Parser.DriverCall
{
    public interface IShaderCall
    {
        ShaderType ShaderType { get; }
    }

    public abstract class ShaderSlots<This, TSlotType> : SlotsBase<This, TSlotType, ShaderContext>, IShaderCall
        where This : SlotsBase<This, TSlotType, ShaderContext>
        where TSlotType : Slot.Base
    {
        public static Dictionary<ShaderType, List<int>> UsedSlots { get; } = new Dictionary<ShaderType, List<int>>();

        protected ShaderSlots(uint order) : base(order) { }

        public override List<int> SlotsUsed => UsedSlots.GetOrAdd(ShaderType);

        protected override Deferred<ShaderContext, DrawCall> Deferred => Previous?.Deferred;

        public override string Name => $"{ShaderType.ToString()[0]}S{base.Name}";

        public ShaderType ShaderType { get; set; }

        private ShaderContext previous;

        private ShaderContext Previous => previous ??= Owner.Previous?.Shader(ShaderType);
    }
}
