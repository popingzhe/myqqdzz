using System;
using System.Text;
using System.Collections.Generic;
using Erpc;



class LogicMsg : Proto, BaseMsg
{
    public string protoName;
    public string protoBody;
    override public void Encode(WriteBuffer wb)
    {
        Eproto.PackArray(wb, 2);
        Eproto.PackString(wb, this.protoName);
        Eproto.PackString(wb, this.protoBody);
    }
    override public void Decode(ReadBuffer rb)
    {
        long c = Eproto.UnpackArray(rb);
        if (c <= 0) { return; }
        Eproto.UnpackString(rb, ref this.protoName);
        if (--c <= 0) { return; }
        Eproto.UnpackString(rb, ref this.protoBody);
        if (--c <= 0) { return; }
        Eproto.UnpackDiscard(rb, c);
    }
    override public Proto Create() { return new LogicMsg(); }
}

