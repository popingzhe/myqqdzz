using Erpc;



class FoodMsg : Proto,BaseMsg
{
    public int foodId;
    public float raduis;
    public float px;
    public float py;
    public int colorR;
    public int colorG;
    public int colorB;
    public int timeStemp;
    override public void Encode(WriteBuffer wb)
    {
        Eproto.PackArray(wb, 8);
        Eproto.PackInteger(wb, this.foodId);
        Eproto.PackDouble(wb, this.raduis);
        Eproto.PackDouble(wb, this.px);
        Eproto.PackDouble(wb, this.py);
        Eproto.PackInteger(wb, this.colorR);
        Eproto.PackInteger(wb, this.colorG);
        Eproto.PackInteger(wb, this.colorB);
        Eproto.PackInteger(wb, this.timeStemp);
    }
    override public void Decode(ReadBuffer rb)
    {
        long c = Eproto.UnpackArray(rb);
        if (c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.foodId);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.raduis);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.px);
        if (--c <= 0) { return; }
        Eproto.UnpackDouble(rb, ref this.py);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.colorR);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.colorG);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.colorB);
        if (--c <= 0) { return; }
        Eproto.UnpackInteger(rb, ref this.timeStemp);
        if (--c <= 0) { return; }
        Eproto.UnpackDiscard(rb, c);
    }
    override public Proto Create() { return new FoodMsg(); }
}

