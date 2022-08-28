namespace Migoto.Log.Parser.ApiCalls;

public class IASetInputLayout : ApiCall, IInputAssembler
{
    public IASetInputLayout(uint order) : base(order) { }

    public ulong pInputLayout { get; set; }
}
