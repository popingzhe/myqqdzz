using Erpc;
using System;
using System.Text;
using System.Collections.Generic;


class EatMsg : Proto,BaseMsg
{
    public int eatId;
    public int eatedId;
    public float px;
    public float py;
    public float raduis;
    public int timeStemp;
    override public void Encode(WriteBuffer wb)
    {
        Eproto.PackArray(wb, 6);
        Eproto.PackInteger(wb, this.eatId);
        Eproto.PackInteger(wb, this.eatedId);
        Eproto.PackDouble(wb, this.px);
        Eproto.PackDouble(wb, this.py);
        Eproto.PackDouble(wb, this.raduis);
        Eproto.PackInteger(wb, this.timeStemp);
    }
    override public void Decode(ReadBuffer rb)
    {
        long c = Eproto.UnpackArray(rb);
        if (c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.eatId);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.eatedId);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.px);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.py);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.raduis);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.timeStemp);
        if (--c <= 0) { return; }
        Eproto.UnpackDiscard(rb, c);
    }
    override public Proto Create() { return new EatMsg(); }
}