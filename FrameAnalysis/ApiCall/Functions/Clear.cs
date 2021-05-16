namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public abstract class Clear : ApiCall, ISingleSlot
    {
        protected Clear(uint order) : base(order) { }

        public ResourceView? Target { get; set; }

        IResource? ISingleSlot.Slot => Target;
    }
}
