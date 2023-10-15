
using Erpc;



class MoveMsg : Proto,BaseMsg
{
    public float px;
    public float py;
    public float pz;
    public int timeStemp;
    public int playerid;
    public float raduis;
    override public void Encode(WriteBuffer wb)
    {
        Eproto.PackArray(wb, 6);
        Eproto.PackDouble(wb, this.px);
        Eproto.PackDouble(wb, this.py);
        Eproto.PackDouble(wb, this.pz);
        Eproto.PackInteger(wb, this.timeStemp);
        Eproto.PackInteger(wb, this.playerid);
        Eproto.PackDouble(wb, this.raduis);
    }
    override public void Decode(ReadBuffer rb)
    {
        long c = Eproto.UnpackArray(rb);
        if (c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.px);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.py);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.pz);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.timeStemp);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.playerid);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.raduis);
        if (--c <= 0) { return; }
        Eproto.UnpackDiscard(rb, c);
    }
    override public Proto Create() { return new MoveMsg(); }
}

